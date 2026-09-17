import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { configuracionApi } from "@/lib/api";

export function ConfiguracionPage() {
  const queryClient = useQueryClient();
  const configuracionQuery = useQuery({ queryKey: ["configuracion", "smtp"], queryFn: configuracionApi.obtenerSmtp });

  const [host, setHost] = useState("");
  const [puerto, setPuerto] = useState("587");
  const [usuario, setUsuario] = useState("");
  const [contrasena, setContrasena] = useState("");
  const [remitente, setRemitente] = useState("");
  const [usaSsl, setUsaSsl] = useState(true);

  useEffect(() => {
    const configuracion = configuracionQuery.data;
    if (!configuracion) return;
    setHost(configuracion.host);
    setPuerto(String(configuracion.puerto));
    setUsuario(configuracion.usuario);
    setRemitente(configuracion.remitente);
    setUsaSsl(configuracion.usaSsl);
  }, [configuracionQuery.data]);

  const datosValidos = host.trim() !== "" && Number(puerto) > 0 && usuario.trim() !== "" && contrasena !== "" && remitente.trim() !== "";

  const guardar = useMutation({
    mutationFn: () =>
      configuracionApi.guardarSmtp({ host: host.trim(), puerto: Number(puerto), usuario: usuario.trim(), contrasena, remitente: remitente.trim(), usaSsl }),
    onSuccess: () => {
      toast.success("Configuración SMTP guardada.");
      queryClient.invalidateQueries({ queryKey: ["configuracion", "smtp"] });
    },
    onError: (error: Error) => toast.error(error.message),
  });

  const probarConexion = useMutation({
    mutationFn: () =>
      configuracionApi.probarSmtp({ host: host.trim(), puerto: Number(puerto), usuario: usuario.trim(), contrasena, usaSsl }),
    onSuccess: (resultado) => {
      if (resultado.exito) toast.success("Conexión y autenticación exitosas.");
      else toast.error(resultado.error ?? "La prueba de conexión falló.");
    },
    onError: (error: Error) => toast.error(error.message),
  });

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Configuración</h2>

      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium">Servidor SMTP</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Servidor usado para enviar directamente los correos de aviso de licencias desde la pantalla "Redactar
            correo". Por seguridad, la contraseña guardada nunca se muestra: si ya hay una configuración guardada,
            hay que volver a escribirla para guardar cambios.
          </p>

          <div className="grid max-w-md gap-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="host">Host</Label>
              <Input id="host" value={host} onChange={(e) => setHost(e.target.value)} placeholder="smtp.ejemplo.cl" />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="puerto">Puerto</Label>
              <Input id="puerto" type="number" value={puerto} onChange={(e) => setPuerto(e.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="usuario">Usuario</Label>
              <Input id="usuario" value={usuario} onChange={(e) => setUsuario(e.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="contrasena">Contraseña</Label>
              <Input id="contrasena" type="password" value={contrasena} onChange={(e) => setContrasena(e.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="remitente">Correo remitente</Label>
              <Input id="remitente" type="email" value={remitente} onChange={(e) => setRemitente(e.target.value)} placeholder="avisos@ejemplo.cl" />
            </div>
            <div className="flex items-center gap-2">
              <input
                id="usaSsl"
                type="checkbox"
                checked={usaSsl}
                onChange={(e) => setUsaSsl(e.target.checked)}
                className="size-4 rounded border-input"
              />
              <Label htmlFor="usaSsl">Usar conexión segura (SSL/TLS)</Label>
            </div>
          </div>

          <div className="flex gap-3">
            <Button
              variant="outline"
              onClick={() => probarConexion.mutate()}
              disabled={!datosValidos || probarConexion.isPending}
            >
              {probarConexion.isPending ? "Probando..." : "Probar conexión"}
            </Button>
            <Button onClick={() => guardar.mutate()} disabled={!datosValidos || guardar.isPending}>
              {guardar.isPending ? "Guardando..." : "Guardar"}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
