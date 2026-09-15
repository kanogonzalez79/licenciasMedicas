import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Dialog, DialogClose, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { calcularDiasReposo, calcularFechaTermino } from "@/lib/fechas-reposo";
import { type Unidad, licenciasApi } from "@/lib/api";

const SIN_ASIGNAR = "__sin_asignar__";
const SIN_TIPO = "__sin_tipo__";

const TIPOS_LICENCIA: Record<string, string> = {
  "1": "1 - Enfermedad o Accidente Común",
  "2": "2 - Prórroga Medicina Preventiva",
  "3": "3 - Licencia Maternal Pre y Post Natal",
  "4": "4 - Enfermedad Grave Hijo Menor de 1 Año",
  "5": "5 - Accidente del Trabajo o del Trayecto",
  "6": "6 - Enfermedad Profesional",
  "7": "7 - Patología del Embarazo",
};

type Campos = {
  folio: string;
  rutPaciente: string;
  nombreCompletoPaciente: string;
  codigoTipoLicencia: string;
  fechaInicioReposo: string;
  fechaTerminoReposo: string;
  cantidadDias: string;
  rutProfesional: string;
  nombreCompletoProfesional: string;
  correoProfesional: string;
  especialidadProfesional: string;
  observaciones: string;
};

const camposIniciales: Campos = {
  folio: "",
  rutPaciente: "",
  nombreCompletoPaciente: "",
  codigoTipoLicencia: "",
  fechaInicioReposo: "",
  fechaTerminoReposo: "",
  cantidadDias: "",
  rutProfesional: "",
  nombreCompletoProfesional: "",
  correoProfesional: "",
  especialidadProfesional: "",
  observaciones: "",
};

export function IngresoManualModal({ unidades }: { unidades: Unidad[] }) {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [campos, setCampos] = useState<Campos>(camposIniciales);
  const [unidadId, setUnidadId] = useState(SIN_ASIGNAR);
  const [archivo, setArchivo] = useState<File | null>(null);

  function limpiar() {
    setCampos(camposIniciales);
    setUnidadId(SIN_ASIGNAR);
    setArchivo(null);
  }

  function actualizarCampo<K extends keyof Campos>(campo: K, valor: string) {
    setCampos((actual) => ({ ...actual, [campo]: valor }));
  }

  function actualizarFechaInicio(valor: string) {
    setCampos((actual) => {
      if (valor && actual.fechaTerminoReposo) {
        return { ...actual, fechaInicioReposo: valor, cantidadDias: String(calcularDiasReposo(valor, actual.fechaTerminoReposo)) };
      }
      if (valor && actual.cantidadDias) {
        return { ...actual, fechaInicioReposo: valor, fechaTerminoReposo: calcularFechaTermino(valor, Number(actual.cantidadDias)) };
      }
      return { ...actual, fechaInicioReposo: valor };
    });
  }

  function actualizarFechaTermino(valor: string) {
    setCampos((actual) => {
      if (actual.fechaInicioReposo && valor) {
        return { ...actual, fechaTerminoReposo: valor, cantidadDias: String(calcularDiasReposo(actual.fechaInicioReposo, valor)) };
      }
      return { ...actual, fechaTerminoReposo: valor };
    });
  }

  function actualizarCantidadDias(valor: string) {
    setCampos((actual) => {
      if (actual.fechaInicioReposo && valor) {
        return { ...actual, cantidadDias: valor, fechaTerminoReposo: calcularFechaTermino(actual.fechaInicioReposo, Number(valor)) };
      }
      return { ...actual, cantidadDias: valor };
    });
  }

  const ingresar = useMutation({
    mutationFn: () =>
      licenciasApi.ingresarManual({
        folio: campos.folio.trim(),
        rutPaciente: campos.rutPaciente.trim(),
        nombreCompletoPaciente: campos.nombreCompletoPaciente.trim(),
        codigoTipoLicencia: Number(campos.codigoTipoLicencia),
        fechaInicioReposo: campos.fechaInicioReposo,
        fechaTerminoReposo: campos.fechaTerminoReposo,
        cantidadDias: Number(campos.cantidadDias),
        rutProfesional: campos.rutProfesional.trim() || undefined,
        nombreCompletoProfesional: campos.nombreCompletoProfesional.trim() || undefined,
        correoProfesional: campos.correoProfesional.trim() || undefined,
        especialidadProfesional: campos.especialidadProfesional.trim() || undefined,
        unidadId: Number(unidadId),
        observaciones: campos.observaciones.trim() || undefined,
        archivo: archivo!,
      }),
    onSuccess: () => {
      toast.success("Licencia ingresada.");
      queryClient.invalidateQueries({ queryKey: ["licencias", "buscar"] });
      limpiar();
      setOpen(false);
    },
  });

  function handleGuardar() {
    if (!campos.folio.trim()) return toast.error("El folio es obligatorio.");
    if (!campos.rutPaciente.trim()) return toast.error("El RUT del paciente es obligatorio.");
    if (!campos.nombreCompletoPaciente.trim()) return toast.error("El nombre del paciente es obligatorio.");
    if (!campos.codigoTipoLicencia) return toast.error("Debe seleccionar el tipo de licencia.");
    if (!campos.fechaInicioReposo || !campos.fechaTerminoReposo || !campos.cantidadDias) {
      return toast.error("Debe completar las fechas de reposo (o fecha de inicio y cantidad de días).");
    }
    if (unidadId === SIN_ASIGNAR) return toast.error("Debe seleccionar una unidad.");
    if (!archivo) return toast.error("Debe adjuntar el documento de respaldo (PDF o imagen).");

    ingresar.mutate();
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(siguiente) => {
        setOpen(siguiente);
        if (!siguiente) limpiar();
      }}
    >
      <DialogTrigger render={<Button variant="outline" />}>Ingresar manual</DialogTrigger>
      <DialogContent className="w-[90vw] max-w-3xl">
        <DialogHeader>
          <DialogTitle>Ingresar licencia manual</DialogTitle>
        </DialogHeader>

        <div className="grid grid-cols-2 gap-x-4 gap-y-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mFolio">Folio</Label>
            <Input id="mFolio" value={campos.folio} onChange={(e) => actualizarCampo("folio", e.target.value)} />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mRutPaciente">RUT paciente</Label>
            <Input
              id="mRutPaciente"
              placeholder="12345678-9"
              value={campos.rutPaciente}
              onChange={(e) => actualizarCampo("rutPaciente", e.target.value)}
            />
          </div>

          <div className="col-span-2 flex flex-col gap-1.5">
            <Label htmlFor="mNombrePaciente">Nombre completo paciente</Label>
            <Input
              id="mNombrePaciente"
              value={campos.nombreCompletoPaciente}
              onChange={(e) => actualizarCampo("nombreCompletoPaciente", e.target.value)}
            />
          </div>

          <div className="col-span-2 flex flex-col gap-1.5">
            <Label htmlFor="mArchivo">Documento de respaldo (PDF o imagen)</Label>
            <Input
              id="mArchivo"
              type="file"
              accept="application/pdf,image/jpeg,image/png,.pdf,.jpg,.jpeg,.png"
              onChange={(e) => setArchivo(e.target.files?.[0] ?? null)}
            />
          </div>

          <div className="col-span-2 flex flex-col gap-1.5">
            <Label htmlFor="mUnidad">Unidad</Label>
            <Select
              items={{
                [SIN_ASIGNAR]: "-- Seleccione --",
                ...Object.fromEntries(unidades.map((u) => [String(u.unidadId), u.descripcion])),
              }}
              value={unidadId}
              onValueChange={(valor) => setUnidadId(valor ?? SIN_ASIGNAR)}
            >
              <SelectTrigger id="mUnidad" className="w-full">
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
          </div>

          <div className="col-span-2 flex flex-col gap-1.5">
            <Label htmlFor="mTipoLicencia">Tipo de licencia</Label>
            <Select
              items={{ [SIN_TIPO]: "-- Seleccione --", ...TIPOS_LICENCIA }}
              value={campos.codigoTipoLicencia || SIN_TIPO}
              onValueChange={(valor) => actualizarCampo("codigoTipoLicencia", !valor || valor === SIN_TIPO ? "" : valor)}
            >
              <SelectTrigger id="mTipoLicencia" className="w-full">
                <SelectValue placeholder="-- Seleccione --" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={SIN_TIPO}>-- Seleccione --</SelectItem>
                {Object.entries(TIPOS_LICENCIA).map(([codigo, descripcion]) => (
                  <SelectItem key={codigo} value={codigo}>
                    {descripcion}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mFechaInicio">Fecha inicio reposo</Label>
            <Input
              id="mFechaInicio"
              type="date"
              value={campos.fechaInicioReposo}
              onChange={(e) => actualizarFechaInicio(e.target.value)}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mFechaTermino">Fecha término reposo</Label>
            <Input
              id="mFechaTermino"
              type="date"
              value={campos.fechaTerminoReposo}
              onChange={(e) => actualizarFechaTermino(e.target.value)}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mCantidadDias">Cantidad de días</Label>
            <Input
              id="mCantidadDias"
              type="number"
              min={1}
              value={campos.cantidadDias}
              onChange={(e) => actualizarCantidadDias(e.target.value)}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mRutProfesional">RUT profesional (opcional)</Label>
            <Input
              id="mRutProfesional"
              placeholder="12345678-9"
              value={campos.rutProfesional}
              onChange={(e) => actualizarCampo("rutProfesional", e.target.value)}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mNombreProfesional">Nombre completo profesional (opcional)</Label>
            <Input
              id="mNombreProfesional"
              value={campos.nombreCompletoProfesional}
              onChange={(e) => actualizarCampo("nombreCompletoProfesional", e.target.value)}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mCorreoProfesional">Correo profesional (opcional)</Label>
            <Input
              id="mCorreoProfesional"
              type="email"
              value={campos.correoProfesional}
              onChange={(e) => actualizarCampo("correoProfesional", e.target.value)}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="mEspecialidadProfesional">Especialidad profesional (opcional)</Label>
            <Input
              id="mEspecialidadProfesional"
              value={campos.especialidadProfesional}
              onChange={(e) => actualizarCampo("especialidadProfesional", e.target.value)}
            />
          </div>

          <div className="col-span-2 flex flex-col gap-1.5">
            <Label htmlFor="mObservaciones">Observaciones (opcional)</Label>
            <Textarea
              id="mObservaciones"
              value={campos.observaciones}
              onChange={(e) => actualizarCampo("observaciones", e.target.value)}
            />
          </div>
        </div>

        <DialogFooter>
          <Button onClick={handleGuardar} disabled={ingresar.isPending}>
            Guardar
          </Button>
          <DialogClose render={<Button variant="outline" />}>Cancelar</DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
