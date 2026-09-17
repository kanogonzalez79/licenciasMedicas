import { useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { type ModoFechaInforme, licenciasApi } from "@/lib/api";

const ETIQUETAS_MODO: Record<ModoFechaInforme, string> = {
  inicio: "Fecha de inicio",
  termino: "Fecha de término",
  interseccion: "Intersección de rangos",
};

export function InformePage() {
  const [fechaDesde, setFechaDesde] = useState("");
  const [fechaHasta, setFechaHasta] = useState("");
  const [modo, setModo] = useState<ModoFechaInforme>("inicio");

  const generar = () => {
    if (!fechaDesde || !fechaHasta) {
      toast.error("Debe indicar fecha desde y fecha hasta.");
      return;
    }

    if (fechaDesde > fechaHasta) {
      toast.error("La fecha desde no puede ser posterior a la fecha hasta.");
      return;
    }

    window.open(licenciasApi.informeUrl(fechaDesde, fechaHasta, modo), "_blank");
  };

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Informe</h2>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap items-end gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="iFechaDesde">Fecha desde</Label>
              <Input
                id="iFechaDesde"
                type="date"
                value={fechaDesde}
                onChange={(e) => setFechaDesde(e.target.value)}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="iFechaHasta">Fecha hasta</Label>
              <Input
                id="iFechaHasta"
                type="date"
                value={fechaHasta}
                onChange={(e) => setFechaHasta(e.target.value)}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="iModo">El rango aplica a</Label>
              <Select
                items={ETIQUETAS_MODO}
                value={modo}
                onValueChange={(valor) => setModo(valor as ModoFechaInforme)}
              >
                <SelectTrigger id="iModo" className="w-56">
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
            <Button onClick={generar}>Generar Excel</Button>
          </div>
        </CardContent>
      </Card>

      <p className="text-sm text-muted-foreground">
        Genera un archivo Excel con las licencias del rango consultado. En modo "Intersección de rangos" se incluyen
        también las licencias cuyo período de reposo se solapa con el rango, aunque ni su fecha de inicio ni su
        fecha de término estén dentro de él.
      </p>
    </div>
  );
}
