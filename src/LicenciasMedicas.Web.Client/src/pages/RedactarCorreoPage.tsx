import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import { correosApi } from "@/lib/api";

function fechaDeHoy(): string {
  const hoy = new Date();
  const mm = String(hoy.getMonth() + 1).padStart(2, "0");
  const dd = String(hoy.getDate()).padStart(2, "0");
  return `${hoy.getFullYear()}-${mm}-${dd}`;
}

export function RedactarCorreoPage() {
  const [fecha, setFecha] = useState(fechaDeHoy());
  const [fechaBuscada, setFechaBuscada] = useState<string | null>(null);
  const [seleccion, setSeleccion] = useState<{ unidadId: number; fecha: string } | null>(null);

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

  function handleBuscar() {
    if (!fecha) {
      toast.error("Seleccione una fecha.");
      return;
    }
    setSeleccion(null);
    setFechaBuscada(fecha);
  }

  async function handleCopiar() {
    const texto = redactarQuery.data?.texto ?? "";
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

      {redactarQuery.data && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-medium">Texto para el correo</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <Textarea readOnly value={redactarQuery.data.texto} className="min-h-56" />
            <Button onClick={handleCopiar}>Copiar</Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
