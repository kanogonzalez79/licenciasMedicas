using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LicenciasMedicas.Core.Tests;

/// <summary>
/// Servidor SMTP mínimo (loopback, en memoria) que habla lo justo del protocolo para que
/// MailKit complete EHLO/AUTH/MAIL/RCPT/DATA/QUIT, usado solo para probar un envío exitoso
/// sin depender de un servidor SMTP real (ver tasks.md 3.1 del cambio "enviar-correo-smtp").
/// </summary>
internal sealed class ServidorSmtpFalso : IDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _tareaAtencion;

    public int Puerto { get; }
    public string? UltimoMensajeRecibido { get; private set; }

    public ServidorSmtpFalso()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Puerto = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _tareaAtencion = Task.Run(() => AtenderAsync(_cts.Token));
    }

    private async Task AtenderAsync(CancellationToken token)
    {
        try
        {
            using var cliente = await _listener.AcceptTcpClientAsync(token);
            await using var stream = cliente.GetStream();
            using var lector = new StreamReader(stream, Encoding.ASCII);
            await using var escritor = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true, NewLine = "\r\n" };

            await escritor.WriteLineAsync("220 fake.local ESMTP");

            var enData = false;
            var datosMensaje = new StringBuilder();

            while (!token.IsCancellationRequested)
            {
                var linea = await lector.ReadLineAsync(token);
                if (linea is null)
                    break;

                if (enData)
                {
                    if (linea == ".")
                    {
                        enData = false;
                        UltimoMensajeRecibido = datosMensaje.ToString();
                        await escritor.WriteLineAsync("250 OK: mensaje encolado");
                    }
                    else
                    {
                        datosMensaje.AppendLine(linea);
                    }
                    continue;
                }

                if (linea.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase))
                {
                    await escritor.WriteLineAsync("250-fake.local saluda");
                    await escritor.WriteLineAsync("250 AUTH PLAIN LOGIN");
                }
                else if (linea.StartsWith("AUTH PLAIN", StringComparison.OrdinalIgnoreCase))
                {
                    if (linea.Length > "AUTH PLAIN".Length + 1)
                    {
                        await escritor.WriteLineAsync("235 Autenticación exitosa");
                    }
                    else
                    {
                        await escritor.WriteLineAsync("334 ");
                        await lector.ReadLineAsync(token);
                        await escritor.WriteLineAsync("235 Autenticación exitosa");
                    }
                }
                else if (linea.StartsWith("AUTH LOGIN", StringComparison.OrdinalIgnoreCase))
                {
                    await escritor.WriteLineAsync("334 VXNlcm5hbWU6");
                    await lector.ReadLineAsync(token);
                    await escritor.WriteLineAsync("334 UGFzc3dvcmQ6");
                    await lector.ReadLineAsync(token);
                    await escritor.WriteLineAsync("235 Autenticación exitosa");
                }
                else if (linea.StartsWith("MAIL FROM", StringComparison.OrdinalIgnoreCase))
                {
                    await escritor.WriteLineAsync("250 OK");
                }
                else if (linea.StartsWith("RCPT TO", StringComparison.OrdinalIgnoreCase))
                {
                    await escritor.WriteLineAsync("250 OK");
                }
                else if (linea.Equals("DATA", StringComparison.OrdinalIgnoreCase))
                {
                    enData = true;
                    await escritor.WriteLineAsync("354 Adelante");
                }
                else if (linea.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    await escritor.WriteLineAsync("221 Adiós");
                    break;
                }
                else
                {
                    await escritor.WriteLineAsync("250 OK");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cierre normal del servidor de prueba (Dispose) mientras esperaba una línea/conexión.
        }
        catch (IOException)
        {
            // El cliente cerró el socket (ej. Disconnect) mientras el servidor escribía/leía.
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        try { _tareaAtencion.Wait(TimeSpan.FromSeconds(2)); } catch { /* best effort en limpieza de test */ }
        _cts.Dispose();
    }
}
