import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import { correosApi } from "@/lib/api";

const EMAIL_PATTERN = /^\S+@\S+\.\S+$/;

function fechaDeHoy(): string {
  const hoy = new Date();
  const mm = String(hoy.getMonth() + 1).padStart(2, "0");
  const dd = String(hoy.getDate()).padStart(2, "0");
  return `${hoy.getFullYear()}-${mm}-${dd}`;
}

function esCorreoValido(correo: string | null): correo is string {
  return correo !== null && EMAIL_PATTERN.test(correo.trim());
}

export function RedactarCorreoPage() {
  const queryClient = useQueryClient();
  const [fecha, setFecha] = useState(fechaDeHoy());
  const [fechaBuscada, setFechaBuscada] = useState<string | null>(null);
  const [seleccion, setSeleccion] = useState<{ unidadId: number; fecha: string } | null>(null);
  const [texto, setTexto] = useState("");

  const unidadesQuery = useQuery({
    queryKey: ["correos", "unidades", fechaBuscada],
    queryFn: () => correosApi.unidadesEnFecha(fechaBuscada!),
    enabled: fechaBuscada !== null,
  });

  const redactarQuery = useQuery({
    queryKey: ["correos", "redactar", seleccion],
    queryFn: () => correosApi.redactar(seleccion!.unidadId, seleccion!.fecha),
    enabled: seleccion !== null,
  });

  useEffect(() => {
    setTexto(redactarQuery.data?.texto ?? "");
  }, [redactarQuery.data]);

  const enviar = useMutation({
    mutationFn: () => correosApi.enviar(seleccion!.unidadId, seleccion!.fecha, texto),
    onSuccess: () => {
      toast.success("Correo enviado.");
      queryClient.invalidateQueries({ queryKey: ["correos", "unidades", fechaBuscada] });
    },
    onError: (error: Error) => toast.error(error.message),
  });

  function handleBuscar() {
    if (!fecha) {
      toast.error("Seleccione una fecha.");
      return;
    }
    setSeleccion(null);
    setFechaBuscada(fecha);
  }

  async function handleCopiar() {
    try {
      await navigator.clipboard.writeText(texto);
    } catch {
      // Navegadores sin permiso/soporte para la Clipboard API: no hay fallback confiable, se informa el error.
      toast.error("No se pudo copiar el texto al portapapeles.");
      return;
    }
    toast.success("Texto copiado al portapapeles.");
  }

  const unidades = unidadesQuery.data ?? [];
  const mostrarSinResultados = unidadesQuery.isSuccess && unidades.length === 0;
  const unidadSeleccionada = unidades.find((u) => u.unidadId === seleccion?.unidadId) ?? null;
  const puedeEnviar = esCorreoValido(unidadSeleccionada?.correoElectronico ?? null);

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Redactar correo</h2>

      <Card>
        <CardContent className="flex items-end gap-3 pt-6">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="fecha">Fecha de grabado</Label>
            <Input id="fecha" type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} />
          </div>
          <Button onClick={handleBuscar} disabled={unidadesQuery.isFetching}>
            Buscar
          </Button>
        </CardContent>
      </Card>

      {mostrarSinResultados && <p className="text-sm text-muted-foreground">No hay licencias grabadas ese día.</p>}

      {unidades.length > 0 && (
        <Card>
          <CardContent className="pt-6">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Unidad</TableHead>
                  <TableHead>Cantidad de licencias</TableHead>
                  <TableHead />
                </TableRow>
              </TableHeader>
              <TableBody>
                {unidades.map((u) => (
                  <TableRow key={u.unidadId}>
                    <TableCell>{u.descripcion}</TableCell>
                    <TableCell>{u.cantidadLicencias}</TableCell>
                    <TableCell>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => setSeleccion({ unidadId: u.unidadId, fecha: fechaBuscada! })}
                      >
                        Redactar correo
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {redactarQuery.data && unidadSeleccionada && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-medium">Texto para el correo</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <Textarea value={texto} onChange={(e) => setTexto(e.target.value)} className="min-h-56 font-mono" />

            {!puedeEnviar && (
              <p className="text-sm text-destructive">
                Esta unidad no tiene un correo electrónico con formato válido. Complételo en la pantalla Unidades
                antes de enviar.
              </p>
            )}

            <div className="flex gap-3">
              <Button variant="outline" onClick={handleCopiar}>
                Copiar
              </Button>

              <AlertDialog>
                <AlertDialogTrigger render={<Button disabled={!puedeEnviar || enviar.isPending} />}>
                  {enviar.isPending ? "Enviando..." : "Enviar correo"}
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Enviar correo</AlertDialogTitle>
                    <AlertDialogDescription>
                      Se enviará el correo a <strong>{unidadSeleccionada.correoElectronico}</strong> con las{" "}
                      {unidadSeleccionada.cantidadLicencias} licencias de {unidadSeleccionada.descripcion} de esta
                      fecha.
                      {unidadSeleccionada.ultimoEnvio && (
                        <>
                          {" "}
                          Ya se había enviado este correo el {new Date(unidadSeleccionada.ultimoEnvio).toLocaleString("es-CL")}. ¿Reenviar
                          de todas formas?
                        </>
                      )}
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Cancelar</AlertDialogCancel>
                    <AlertDialogAction onClick={() => enviar.mutate()}>Enviar</AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
