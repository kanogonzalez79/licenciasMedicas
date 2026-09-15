import { Card, CardContent, CardHeader } from "@/components/ui/card";

export function InicioPage() {
  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Sistema de Licencias Médicas</h2>
      <Card>
        <CardHeader>
          <p className="text-sm text-muted-foreground">Use el menú de la izquierda para:</p>
        </CardHeader>
        <CardContent>
          <ul className="list-disc space-y-2 pl-5 text-sm">
            <li>
              <strong>Unidades</strong>: mantener el listado de unidades a las que se asignan las licencias.
            </li>
            <li>
              <strong>Buscar licencia</strong>: buscar licencias ya grabadas y ver su PDF original.
            </li>
            <li>
              <strong>Ingresar licencias</strong>: procesar los PDF de la carpeta{" "}
              <code className="rounded bg-muted px-1 py-0.5 font-mono text-xs">incoming</code> y grabarlos.
            </li>
            <li>
              <strong>Redactar correo</strong>: generar el texto de aviso para las unidades con licencias grabadas en un
              día.
            </li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
