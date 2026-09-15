import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
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
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { LicenciaDetalleModal } from "@/components/licencia-detalle-modal";
import { type FiltroLicencias, type Licencia, licenciasApi, unidadesApi } from "@/lib/api";

const TODAS_LAS_UNIDADES = "__todas__";

const filtroVacio: FiltroLicencias = {
  fechaDesde: "",
  fechaHasta: "",
  rut: "",
  nombre: "",
  unidadId: "",
};

export function BuscarPage() {
  const queryClient = useQueryClient();
  const [filtro, setFiltro] = useState<FiltroLicencias>(filtroVacio);
  const [filtroBuscado, setFiltroBuscado] = useState<FiltroLicencias>(filtroVacio);
  const [licenciaSeleccionada, setLicenciaSeleccionada] = useState<Licencia | null>(null);

  const unidadesQuery = useQuery({ queryKey: ["unidades"], queryFn: unidadesApi.listar });
  const resultadosQuery = useQuery({
    queryKey: ["licencias", "buscar", filtroBuscado],
    queryFn: () => licenciasApi.buscar(filtroBuscado),
  });

  const eliminar = useMutation({
    mutationFn: (id: number) => licenciasApi.eliminar(id),
    onSuccess: () => {
      toast.success("Licencia eliminada.");
      queryClient.invalidateQueries({ queryKey: ["licencias", "buscar"] });
    },
  });

  const unidades = unidadesQuery.data ?? [];
  const items = resultadosQuery.data?.items ?? [];
  const total = resultadosQuery.data?.total ?? 0;

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Buscar licencia</h2>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap items-end gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="fFechaDesde">Fecha inicio desde</Label>
              <Input
                id="fFechaDesde"
                type="date"
                value={filtro.fechaDesde}
                onChange={(e) => setFiltro({ ...filtro, fechaDesde: e.target.value })}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="fFechaHasta">Fecha inicio hasta</Label>
              <Input
                id="fFechaHasta"
                type="date"
                value={filtro.fechaHasta}
                onChange={(e) => setFiltro({ ...filtro, fechaHasta: e.target.value })}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="fRut">RUT paciente</Label>
              <Input
                id="fRut"
                placeholder="Sin puntos ni DV"
                value={filtro.rut}
                onChange={(e) => setFiltro({ ...filtro, rut: e.target.value })}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="fNombre">Nombre paciente</Label>
              <Input
                id="fNombre"
                value={filtro.nombre}
                onChange={(e) => setFiltro({ ...filtro, nombre: e.target.value })}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="fUnidad">Unidad</Label>
              <Select
                items={{
                  [TODAS_LAS_UNIDADES]: "Todas",
                  ...Object.fromEntries(unidades.map((u) => [String(u.unidadId), u.descripcion])),
                }}
                value={filtro.unidadId || TODAS_LAS_UNIDADES}
                onValueChange={(valor) =>
                  setFiltro({ ...filtro, unidadId: !valor || valor === TODAS_LAS_UNIDADES ? "" : valor })
                }
              >
                <SelectTrigger id="fUnidad">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={TODAS_LAS_UNIDADES}>Todas</SelectItem>
                  {unidades.map((u) => (
                    <SelectItem key={u.unidadId} value={String(u.unidadId)}>
                      {u.descripcion}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <Button onClick={() => setFiltroBuscado(filtro)} disabled={resultadosQuery.isFetching}>
              Buscar
            </Button>
          </div>
        </CardContent>
      </Card>

      {resultadosQuery.data && <p className="text-sm text-muted-foreground">{total} resultado(s).</p>}

      <Card>
        <CardContent className="pt-6">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Folio</TableHead>
                <TableHead>RUT</TableHead>
                <TableHead>Nombre completo</TableHead>
                <TableHead>Tipo de licencia</TableHead>
                <TableHead>Fecha inicio</TableHead>
                <TableHead>Fecha término</TableHead>
                <TableHead>Días</TableHead>
                <TableHead>Unidad</TableHead>
                <TableHead>Fecha ingreso</TableHead>
                <TableHead>Origen</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={11} className="italic text-muted-foreground">
                    Sin resultados.
                  </TableCell>
                </TableRow>
              )}
              {items.map((licencia) => (
                <TableRow key={licencia.licenciaId}>
                  <TableCell>{licencia.folio}</TableCell>
                  <TableCell>
                    {licencia.rutPacienteSinDv}-{licencia.dvPaciente}
                  </TableCell>
                  <TableCell>{licencia.nombreCompletoPaciente}</TableCell>
                  <TableCell>{licencia.descripcionTipoLicencia}</TableCell>
                  <TableCell>{licencia.fechaInicioReposo}</TableCell>
                  <TableCell>{licencia.fechaTerminoReposo}</TableCell>
                  <TableCell>{licencia.cantidadDias}</TableCell>
                  <TableCell>{licencia.unidadDescripcion}</TableCell>
                  <TableCell>{licencia.fechaIngresoSistema.slice(0, 10)}</TableCell>
                  <TableCell>
                    <Badge variant={licencia.esIngresoManual ? "secondary" : "outline"}>
                      {licencia.esIngresoManual ? "Manual" : "Automático"}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => window.open(licenciasApi.pdfUrl(licencia.licenciaId), "_blank")}
                      >
                        Ver PDF
                      </Button>
                      <Button size="sm" variant="outline" onClick={() => setLicenciaSeleccionada(licencia)}>
                        Ver ficha
                      </Button>
                      <AlertDialog>
                        <AlertDialogTrigger
                          render={<Button size="sm" variant="destructive" />}
                        >
                          Eliminar
                        </AlertDialogTrigger>
                        <AlertDialogContent>
                          <AlertDialogHeader>
                            <AlertDialogTitle>Eliminar licencia</AlertDialogTitle>
                            <AlertDialogDescription>
                              Se eliminará de forma permanente la licencia con folio {licencia.folio} de{" "}
                              {licencia.nombreCompletoPaciente}, junto con su PDF archivado. Esta acción no se puede
                              deshacer.
                            </AlertDialogDescription>
                          </AlertDialogHeader>
                          <AlertDialogFooter>
                            <AlertDialogCancel>Cancelar</AlertDialogCancel>
                            <AlertDialogAction
                              variant="destructive"
                              onClick={() => eliminar.mutate(licencia.licenciaId)}
                            >
                              Eliminar
                            </AlertDialogAction>
                          </AlertDialogFooter>
                        </AlertDialogContent>
                      </AlertDialog>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <LicenciaDetalleModal
        licencia={licenciaSeleccionada}
        onOpenChange={(open) => {
          if (!open) setLicenciaSeleccionada(null);
        }}
      />
    </div>
  );
}
