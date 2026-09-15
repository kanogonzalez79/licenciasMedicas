import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Separator } from "@/components/ui/separator";
import type { Licencia } from "@/lib/api";

const SIN_INFORMAR = "No informado";

const DESCRIPCIONES_TIPO_FORMULARIO: Record<number, string> = {
  1: "Formulario tipo 1",
  2: "Formulario tipo 2",
  3: "Manual",
};

const DESCRIPCIONES_SEXO: Record<string, string> = {
  M: "Masculino",
  F: "Femenino",
};

function formatearValor(valor: string | number | null | undefined): string {
  if (valor === null || valor === undefined || valor === "") return SIN_INFORMAR;
  return String(valor);
}

function formatearConDescripcion(
  valor: string | number | null | undefined,
  descripciones: Record<string | number, string>,
): string {
  if (valor === null || valor === undefined || valor === "") return SIN_INFORMAR;
  const descripcion = descripciones[valor];
  return descripcion ? `${valor} - ${descripcion}` : String(valor);
}

function Campo({
  etiqueta,
  valor,
  formatear = formatearValor,
}: {
  etiqueta: string;
  valor: string | number | null | undefined;
  formatear?: (valor: string | number | null | undefined) => string;
}) {
  const texto = formatear(valor);
  return (
    <div className="flex flex-col gap-0.5">
      <span className="text-xs text-muted-foreground">{etiqueta}</span>
      <span className={texto === SIN_INFORMAR ? "italic text-muted-foreground" : ""}>{texto}</span>
    </div>
  );
}

function Seccion({ titulo, children }: { titulo: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-2">
      <h4 className="text-sm font-medium text-foreground">{titulo}</h4>
      <div className="grid grid-cols-2 gap-x-4 gap-y-2 text-sm">{children}</div>
    </div>
  );
}

export function LicenciaDetalleModal({
  licencia,
  onOpenChange,
}: {
  licencia: Licencia | null;
  onOpenChange: (open: boolean) => void;
}) {
  return (
    <Dialog open={licencia !== null} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Ficha de licencia</DialogTitle>
        </DialogHeader>

        {licencia && (
          <div className="flex flex-col gap-4">
            <Seccion titulo="Identificación">
              <Campo etiqueta="Folio" valor={licencia.folio} />
              <Campo etiqueta="Origen" valor={licencia.esIngresoManual ? "Manual" : "Automático"} />
              <Campo
                etiqueta="Tipo de formulario"
                valor={licencia.tipoFormulario}
                formatear={(valor) => formatearConDescripcion(valor, DESCRIPCIONES_TIPO_FORMULARIO)}
              />
              <Campo etiqueta="Fecha de ingreso al sistema" valor={licencia.fechaIngresoSistema} />
            </Seccion>

            <Separator />

            <Seccion titulo="Paciente">
              <Campo etiqueta="RUT" valor={`${licencia.rutPacienteSinDv}-${licencia.dvPaciente}`} />
              <Campo etiqueta="Nombre completo" valor={licencia.nombreCompletoPaciente} />
              <Campo etiqueta="Apellido paterno" valor={licencia.apellidoPaternoPaciente} />
              <Campo etiqueta="Apellido materno" valor={licencia.apellidoMaternoPaciente} />
              <Campo etiqueta="Nombres" valor={licencia.nombresPaciente} />
              <Campo etiqueta="Edad" valor={licencia.edadPaciente} />
              <Campo
                etiqueta="Sexo"
                valor={licencia.sexoPaciente}
                formatear={(valor) => formatearConDescripcion(valor, DESCRIPCIONES_SEXO)}
              />
            </Seccion>

            <Separator />

            <Seccion titulo="Licencia médica">
              <Campo etiqueta="Tipo de licencia" valor={licencia.descripcionTipoLicencia} />
              <Campo etiqueta="Código de tipo de licencia" valor={licencia.codigoTipoLicencia} />
              <Campo etiqueta="Fecha de emisión/otorgamiento" valor={licencia.fechaEmisionOtorgamiento} />
              <Campo etiqueta="Fecha inicio reposo" valor={licencia.fechaInicioReposo} />
              <Campo etiqueta="Fecha término reposo" valor={licencia.fechaTerminoReposo} />
              <Campo etiqueta="Cantidad de días" valor={licencia.cantidadDias} />
              <div className="col-span-2">
                <Campo etiqueta="Observaciones" valor={licencia.observaciones} />
              </div>
            </Seccion>

            <Separator />

            <Seccion titulo="Profesional">
              <Campo
                etiqueta="RUT"
                valor={
                  licencia.rutProfesionalSinDv
                    ? `${licencia.rutProfesionalSinDv}-${licencia.dvProfesional ?? ""}`
                    : null
                }
              />
              <Campo etiqueta="Nombre completo" valor={licencia.nombreCompletoProfesional} />
              <Campo etiqueta="Correo" valor={licencia.correoProfesional} />
              <Campo etiqueta="Especialidad" valor={licencia.especialidadProfesional} />
            </Seccion>

            <Separator />

            <Seccion titulo="Unidad">
              <Campo etiqueta="Unidad" valor={licencia.unidadDescripcion} />
            </Seccion>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
