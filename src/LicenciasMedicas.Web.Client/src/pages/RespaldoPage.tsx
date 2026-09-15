import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { respaldoApi } from "@/lib/api";

export function RespaldoPage() {
  const respaldar = useMutation({
    mutationFn: () => respaldoApi.ejecutar(),
    onSuccess: (resultado) => {
      if (resultado.estado === "cancelado") {
        toast.info("Respaldo cancelado: no se eligió una carpeta destino.");
        return;
      }
      toast.success(
        `Respaldo completado: ${resultado.pdfsCopiados} PDF copiados, ${resultado.pdfsEliminados} PDF eliminados en el destino.`,
      );
    },
    onError: (error: Error) => toast.error(error.message),
  });

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Respaldo</h2>

      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium">Respaldar datos grabados</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <p className="text-sm text-muted-foreground">
            Copia la base de datos y los PDF ya grabados hacia una carpeta que elijas (por ejemplo, un disco
            externo). Los PDF se sincronizan como espejo: se copian los que falten y se eliminan en destino los
            que ya no existan en el origen. No incluye licencias pendientes de revisión o de asignación de
            unidad.
          </p>
          <Button onClick={() => respaldar.mutate()} disabled={respaldar.isPending}>
            {respaldar.isPending ? "Respaldando..." : "Respaldar"}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
