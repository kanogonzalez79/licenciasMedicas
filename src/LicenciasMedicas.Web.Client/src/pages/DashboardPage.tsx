import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { Bar, BarChart, CartesianGrid, LabelList, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import {
  calcularProporcionIngreso,
  calcularRankingPorTipo,
  calcularRankingPorUnidad,
  calcularSerieTemporal,
  calcularTotalesVolumen,
  formatearEtiquetaSerie,
  limitarRankingConOtras,
  type ItemRanking,
  type SerieTemporal,
} from "@/lib/dashboard-agregaciones";
import { type Licencia, type ModoFechaInforme, licenciasApi } from "@/lib/api";

const ETIQUETAS_MODO: Record<ModoFechaInforme, string> = {
  inicio: "Fecha de inicio",
  termino: "Fecha de término",
  interseccion: "Intersección de rangos",
};

const MAX_ITEMS_RANKING = 7;

type RangoAplicado = { fechaDesde: string; fechaHasta: string; modo: ModoFechaInforme };

const formatoNumero = new Intl.NumberFormat("es-CL", { maximumFractionDigits: 1 });

const LICENCIAS_VACIO: Licencia[] = [];

export function DashboardPage() {
  const [fechaDesde, setFechaDesde] = useState("");
  const [fechaHasta, setFechaHasta] = useState("");
  const [modo, setModo] = useState<ModoFechaInforme>("inicio");
  const [rangoAplicado, setRangoAplicado] = useState<RangoAplicado | null>(null);

  const dashboardQuery = useQuery({
    queryKey: ["licencias", "dashboard", rangoAplicado],
    queryFn: () => licenciasApi.dashboard(rangoAplicado!.fechaDesde, rangoAplicado!.fechaHasta, rangoAplicado!.modo),
    enabled: rangoAplicado !== null,
  });

  const aplicar = () => {
    if (!fechaDesde || !fechaHasta) {
      toast.error("Debe indicar fecha desde y fecha hasta.");
      return;
    }

    if (fechaDesde > fechaHasta) {
      toast.error("La fecha desde no puede ser posterior a la fecha hasta.");
      return;
    }

    setRangoAplicado({ fechaDesde, fechaHasta, modo });
  };

  const licencias = dashboardQuery.data ?? LICENCIAS_VACIO;

  const totales = useMemo(() => calcularTotalesVolumen(licencias), [licencias]);
  const rankingTipo = useMemo(() => calcularRankingPorTipo(licencias), [licencias]);
  const rankingUnidad = useMemo(
    () => limitarRankingConOtras(calcularRankingPorUnidad(licencias), MAX_ITEMS_RANKING),
    [licencias],
  );
  const proporcion = useMemo(() => calcularProporcionIngreso(licencias), [licencias]);
  const serie = useMemo(
    () =>
      rangoAplicado
        ? calcularSerieTemporal(licencias, rangoAplicado.fechaDesde, rangoAplicado.fechaHasta, rangoAplicado.modo)
        : null,
    [licencias, rangoAplicado],
  );

  const porcentajeManual = totales.totalLicencias === 0 ? 0 : Math.round((proporcion.manual / totales.totalLicencias) * 100);

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Dashboard</h2>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap items-end gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="dFechaDesde">Fecha desde</Label>
              <Input id="dFechaDesde" type="date" value={fechaDesde} onChange={(e) => setFechaDesde(e.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="dFechaHasta">Fecha hasta</Label>
              <Input id="dFechaHasta" type="date" value={fechaHasta} onChange={(e) => setFechaHasta(e.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="dModo">El rango aplica a</Label>
              <Select items={ETIQUETAS_MODO} value={modo} onValueChange={(valor) => setModo(valor as ModoFechaInforme)}>
                <SelectTrigger id="dModo" className="w-56">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {(Object.entries(ETIQUETAS_MODO) as [ModoFechaInforme, string][]).map(([valor, etiqueta]) => (
                    <SelectItem key={valor} value={valor}>
                      {etiqueta}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <Button onClick={aplicar} disabled={dashboardQuery.isFetching}>
              Aplicar
            </Button>
          </div>
        </CardContent>
      </Card>

      {!rangoAplicado && (
        <p className="text-sm text-muted-foreground">Elige un rango de fechas y aplica para ver el resumen de licencias.</p>
      )}

      {rangoAplicado && dashboardQuery.isLoading && <p className="text-sm text-muted-foreground">Calculando…</p>}

      {rangoAplicado && dashboardQuery.isError && (
        <p className="text-sm text-destructive">
          {dashboardQuery.error instanceof Error ? dashboardQuery.error.message : "No se pudo calcular el dashboard."}
        </p>
      )}

      {rangoAplicado && dashboardQuery.isSuccess && totales.totalLicencias === 0 && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">No hay licencias en el rango consultado.</p>
          </CardContent>
        </Card>
      )}

      {rangoAplicado && dashboardQuery.isSuccess && totales.totalLicencias > 0 && (
        <div className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-3">
            <TileVolumen etiqueta="Total de licencias" valor={formatoNumero.format(totales.totalLicencias)} />
            <TileVolumen etiqueta="Total días de reposo" valor={formatoNumero.format(totales.totalDiasReposo)} />
            <TileVolumen etiqueta="Promedio de días por licencia" valor={formatoNumero.format(totales.promedioDias)} />
          </div>

          <div className="grid gap-4 lg:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Por tipo de licencia</CardTitle>
              </CardHeader>
              <CardContent>
                <RankingChart items={rankingTipo} />
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Por unidad</CardTitle>
              </CardHeader>
              <CardContent>
                <RankingChart items={rankingUnidad} />
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Ingreso manual vs. automático</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex h-6 w-full overflow-hidden rounded-md bg-muted">
                {proporcion.automatico > 0 && (
                  <div
                    className="flex items-center justify-center text-xs font-medium text-white"
                    style={{ width: `${100 - porcentajeManual}%`, backgroundColor: "var(--chart-1)" }}
                  >
                    {100 - porcentajeManual >= 12 ? `${100 - porcentajeManual}%` : ""}
                  </div>
                )}
                {proporcion.manual > 0 && (
                  <div
                    className="flex items-center justify-center text-xs font-medium text-white"
                    style={{ width: `${porcentajeManual}%`, backgroundColor: "var(--chart-2)" }}
                  >
                    {porcentajeManual >= 12 ? `${porcentajeManual}%` : ""}
                  </div>
                )}
              </div>
              <div className="flex flex-wrap gap-4 text-sm text-muted-foreground">
                <span className="flex items-center gap-1.5">
                  <span className="size-2.5 rounded-full" style={{ backgroundColor: "var(--chart-1)" }} />
                  Automático: {formatoNumero.format(proporcion.automatico)}
                </span>
                <span className="flex items-center gap-1.5">
                  <span className="size-2.5 rounded-full" style={{ backgroundColor: "var(--chart-2)" }} />
                  Manual: {formatoNumero.format(proporcion.manual)}
                </span>
              </div>
            </CardContent>
          </Card>

          {serie && (
            <Card>
              <CardHeader>
                <CardTitle>Tendencia en el tiempo</CardTitle>
              </CardHeader>
              <CardContent>
                <TendenciaChart serie={serie} />
              </CardContent>
            </Card>
          )}
        </div>
      )}
    </div>
  );
}

function TileVolumen({ etiqueta, valor }: { etiqueta: string; valor: string }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-sm font-normal text-muted-foreground">{etiqueta}</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-3xl font-semibold">{valor}</p>
      </CardContent>
    </Card>
  );
}

function RankingChart({ items }: { items: ItemRanking[] }) {
  // Filas altas para dar espacio a etiquetas largas: recharts oculta un tick entero (en vez de
  // truncarlo) si se superpone verticalmente con el de la fila vecina al hacer word-wrap.
  const alto = Math.max(items.length * 64, 100);
  return (
    <div style={{ width: "100%", height: alto }}>
      <ResponsiveContainer>
        <BarChart data={items} layout="vertical" margin={{ top: 4, right: 24, bottom: 4, left: 4 }}>
          <CartesianGrid horizontal={false} stroke="var(--border)" />
          <XAxis
            type="number"
            allowDecimals={false}
            tick={{ fill: "var(--muted-foreground)", fontSize: 12 }}
            axisLine={{ stroke: "var(--border)" }}
            tickLine={false}
          />
          <YAxis
            type="category"
            dataKey="etiqueta"
            width={170}
            tick={{ fill: "var(--foreground)", fontSize: 12 }}
            axisLine={false}
            tickLine={false}
            interval={0}
          />
          <Tooltip
            cursor={{ fill: "var(--muted)" }}
            contentStyle={{
              backgroundColor: "var(--popover)",
              border: "1px solid var(--border)",
              borderRadius: 8,
              color: "var(--popover-foreground)",
            }}
            formatter={(value) => [String(value), "Licencias"]}
          />
          <Bar dataKey="cantidad" fill="var(--chart-1)" radius={[0, 4, 4, 0]} barSize={20}>
            <LabelList dataKey="cantidad" position="right" fill="var(--foreground)" fontSize={12} />
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}

function TendenciaChart({ serie }: { serie: SerieTemporal }) {
  const datos = serie.puntos.map((punto) => ({
    ...punto,
    etiquetaLegible: formatearEtiquetaSerie(punto.etiqueta, serie.granularidad),
  }));

  return (
    <div style={{ width: "100%", height: 260 }}>
      <ResponsiveContainer>
        <BarChart data={datos} margin={{ top: 8, right: 8, bottom: 4, left: 4 }}>
          <CartesianGrid vertical={false} stroke="var(--border)" />
          <XAxis
            dataKey="etiquetaLegible"
            tick={{ fill: "var(--muted-foreground)", fontSize: 12 }}
            axisLine={{ stroke: "var(--border)" }}
            tickLine={false}
            interval="preserveStartEnd"
          />
          <YAxis
            allowDecimals={false}
            tick={{ fill: "var(--muted-foreground)", fontSize: 12 }}
            axisLine={false}
            tickLine={false}
            width={32}
          />
          <Tooltip
            cursor={{ fill: "var(--muted)" }}
            contentStyle={{
              backgroundColor: "var(--popover)",
              border: "1px solid var(--border)",
              borderRadius: 8,
              color: "var(--popover-foreground)",
            }}
            formatter={(value) => [String(value), "Licencias"]}
          />
          <Bar dataKey="cantidad" fill="var(--chart-1)" radius={[4, 4, 0, 0]} maxBarSize={28} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
