import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { IngresoManualModal } from "@/components/ingreso-manual-modal";
import { type FilaFallida, type LicenciaRevision, type Unidad, procesamientoApi, unidadesApi } from "@/lib/api";

const SIN_ASIGNAR = "__sin_asignar__";

const motivosLegibles: Record<string, string> = {
  TIPO_NO_RECONOCIDO: "No se reconoció el formato del PDF",
  RUT_PACIENTE_INVALIDO: "El RUT del paciente no es válido",
  SECCION_TRABAJADOR_NO_ENCONTRADA: "No se encontraron los datos del trabajador",
  FECHAS_INVALIDAS: "Las fechas de reposo no son válidas",
  DIAS_INVALIDOS: "La cantidad de días no es válida",
  TIPO_LICENCIA_INVALIDO: "El código de tipo de licencia no es válido",
  FOLIO_NO_ENCONTRADO: "No se encontró el folio del documento",
  TIPO2_SIN_COMPROBANTE: "El PDF no trae la página de Comprobante",
  ERROR_LECTURA_PDF: "No se pudo leer el archivo PDF",
  DUPLICADO_YA_PROCESADO: "Este PDF ya fue grabado anteriormente",
  DUPLICADO_FOLIO_YA_EXISTENTE: "Ya existe una licencia grabada con este folio",
  NO_ENCONTRADA: "La revisión ya no existe",
  FALTA_UNIDAD: "Falta asignar una unidad",
  FOLIO_DUPLICADO: "Ya existe una licencia grabada con este folio",
};

function motivoLegible(motivo: string): string {
  return motivosLegibles[motivo] ?? motivo;
}

function FilaPendiente({
  revision,
  unidades,
  unidadSeleccionada,
  onCambiarUnidad,
}: {
  revision: LicenciaRevision;
  unidades: Unidad[];
  unidadSeleccionada: string;
  onCambiarUnidad: (unidadId: string) => void;
}) {
  return (
    <TableRow>
      <TableCell>{revision.rutPacienteSinDv}</TableCell>
      <TableCell>{revision.dvPaciente}</TableCell>
      <TableCell>{revision.nombreCompletoPaciente}</TableCell>
      <TableCell>{revision.descripcionTipoLicencia}</TableCell>
      <TableCell>{revision.fechaInicioReposo}</TableCell>
      <TableCell>{revision.fechaTerminoReposo}</TableCell>
      <TableCell>{revision.cantidadDias}</TableCell>
      <TableCell>
        <Badge variant="success">Sí</Badge>
      </TableCell>
      <TableCell>
        <Select
          items={{
            [SIN_ASIGNAR]: "-- Seleccione --",
            ...Object.fromEntries(unidades.map((u) => [String(u.unidadId), u.descripcion])),
          }}
          value={unidadSeleccionada}
          onValueChange={(valor) => onCambiarUnidad(valor ?? SIN_ASIGNAR)}
        >
          <SelectTrigger>
            <SelectValue placeholder="-- Seleccione --" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={SIN_ASIGNAR}>-- Seleccione --</SelectItem>
            {unidades.map((u) => (
              <SelectItem key={u.unidadId} value={String(u.unidadId)}>
                {u.descripcion}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </TableCell>
    </TableRow>
  );
}

export function IngresarPage() {
  const queryClient = useQueryClient();
  const unidadesQuery = useQuery({ queryKey: ["unidades"], queryFn: unidadesApi.listar });
  const pendientesQuery = useQuery({ queryKey: ["procesamiento", "pendientes"], queryFn: procesamientoApi.pendientes });

  const [fallidas, setFallidas] = useState<FilaFallida[]>([]);
  const [asignaciones, setAsignaciones] = useState<Record<number, string>>({});

  const pendientes = pendientesQuery.data ?? [];
  const unidades = unidadesQuery.data ?? [];

  useEffect(() => {
    setAsignaciones((actual) => {
      const siguiente: Record<number, string> = {};
      for (const p of pendientes) {
        siguiente[p.revisionId] = actual[p.revisionId] ?? (p.unidadId ? String(p.unidadId) : SIN_ASIGNAR);
      }
      return siguiente;
    });
  }, [pendientes]);

  const procesar = useMutation({
    mutationFn: procesamientoApi.procesar,
    onSuccess: (resultado) => {
      queryClient.setQueryData(["procesamiento", "pendientes"], resultado.pendientes);
      setFallidas(resultado.fallidas);
      toast.success(`Se leyeron ${resultado.pendientes.length} licencia(s). ${resultado.fallidas.length} archivo(s) no se pudieron procesar.`);
    },
  });

  const grabar = useMutation({
    mutationFn: () =>
      procesamientoApi.grabar(
        pendientes.map((p) => {
          const valor = asignaciones[p.revisionId];
          return { revisionId: p.revisionId, unidadId: valor && valor !== SIN_ASIGNAR ? Number(valor) : null };
        }),
      ),
    onSuccess: async (resultado) => {
      await queryClient.invalidateQueries({ queryKey: ["procesamiento", "pendientes"] });
      if (resultado.noGrabadas.length === 0) {
        toast.success(`Se grabaron ${resultado.grabadas.length} licencia(s).`);
      } else {
        const detalle = resultado.noGrabadas.map((n) => `#${n.revisionId}: ${motivoLegible(n.motivo)}`).join("; ");
        toast.error(`Se grabaron ${resultado.grabadas.length} licencia(s). No se grabaron: ${detalle}`);
      }
    },
  });

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Ingresar licencias</h2>

      <Card>
        <CardContent className="flex gap-3 pt-6">
          <Button onClick={() => procesar.mutate()} disabled={procesar.isPending}>
            Procesar
          </Button>
          {pendientes.length > 0 && (
            <Button onClick={() => grabar.mutate()} disabled={grabar.isPending} variant="outline">
              Grabar
            </Button>
          )}
          <IngresoManualModal unidades={unidades} />
        </CardContent>
      </Card>

      {pendientes.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-medium">Licencias leídas, pendientes de grabar</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>RUT sin DV</TableHead>
                  <TableHead>DV</TableHead>
                  <TableHead>Nombre completo</TableHead>
                  <TableHead>Tipo de licencia</TableHead>
                  <TableHead>Fecha inicio</TableHead>
                  <TableHead>Fecha término</TableHead>
                  <TableHead>Días</TableHead>
                  <TableHead>¿Se leyó bien?</TableHead>
                  <TableHead>Unidad</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {pendientes.map((p) => (
                  <FilaPendiente
                    key={p.revisionId}
                    revision={p}
                    unidades={unidades}
                    unidadSeleccionada={asignaciones[p.revisionId] ?? SIN_ASIGNAR}
                    onCambiarUnidad={(valor) => setAsignaciones((actual) => ({ ...actual, [p.revisionId]: valor }))}
                  />
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {fallidas.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-sm font-medium">PDF que no se pudieron procesar (siguen en incoming)</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Archivo</TableHead>
                  <TableHead>Motivo</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {fallidas.map((f) => (
                  <TableRow key={f.nombreArchivoOriginal}>
                    <TableCell>{f.nombreArchivoOriginal}</TableCell>
                    <TableCell>
                      <Badge variant="destructive">{motivoLegible(f.motivo)}</Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
