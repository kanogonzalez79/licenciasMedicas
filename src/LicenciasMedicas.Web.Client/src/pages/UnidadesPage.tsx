import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { type Unidad, unidadesApi } from "@/lib/api";

const EMAIL_PATTERN = /^\S+@\S+\.\S+$/;

function esCorreoValido(correo: string): boolean {
  return EMAIL_PATTERN.test(correo.trim());
}

export function UnidadesPage() {
  const queryClient = useQueryClient();
  const unidadesQuery = useQuery({ queryKey: ["unidades"], queryFn: unidadesApi.listar });

  const [nuevaDescripcion, setNuevaDescripcion] = useState("");
  const [nuevoCorreo, setNuevoCorreo] = useState("");

  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [editDescripcion, setEditDescripcion] = useState("");
  const [editCorreo, setEditCorreo] = useState("");

  const crear = useMutation({
    mutationFn: () => unidadesApi.crear(nuevaDescripcion.trim(), nuevoCorreo.trim()),
    onSuccess: () => {
      setNuevaDescripcion("");
      setNuevoCorreo("");
      toast.success("Unidad agregada.");
      queryClient.invalidateQueries({ queryKey: ["unidades"] });
    },
  });

  const editar = useMutation({
    mutationFn: () => unidadesApi.editar(editandoId!, editDescripcion.trim(), editCorreo.trim()),
    onSuccess: () => {
      setEditandoId(null);
      toast.success("Unidad actualizada.");
      queryClient.invalidateQueries({ queryKey: ["unidades"] });
    },
  });

  function iniciarEdicion(unidad: Unidad) {
    setEditandoId(unidad.unidadId);
    setEditDescripcion(unidad.descripcion);
    setEditCorreo(unidad.correoElectronico ?? "");
  }

  function handleAgregar() {
    if (!nuevaDescripcion.trim()) {
      toast.error("La descripción es obligatoria.");
      return;
    }
    if (!esCorreoValido(nuevoCorreo)) {
      toast.error("El correo electrónico es obligatorio y debe tener un formato válido.");
      return;
    }
    crear.mutate();
  }

  function handleGuardarEdicion() {
    if (!editDescripcion.trim()) {
      toast.error("La descripción es obligatoria.");
      return;
    }
    if (!esCorreoValido(editCorreo)) {
      toast.error("El correo electrónico es obligatorio y debe tener un formato válido.");
      return;
    }
    editar.mutate();
  }

  const unidades = unidadesQuery.data ?? [];

  return (
    <div className="space-y-4">
      <h2 className="text-lg font-semibold">Unidades</h2>

      <Card>
        <CardContent className="pt-6">
          <h3 className="mb-3 text-sm font-medium">Agregar unidad</h3>
          <div className="flex flex-wrap items-end gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="nuevaDescripcion">Descripción</Label>
              <Input id="nuevaDescripcion" value={nuevaDescripcion} onChange={(e) => setNuevaDescripcion(e.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="nuevoCorreo">Correo electrónico</Label>
              <Input id="nuevoCorreo" type="email" value={nuevoCorreo} onChange={(e) => setNuevoCorreo(e.target.value)} />
            </div>
            <Button onClick={handleAgregar} disabled={crear.isPending}>
              Agregar
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Descripción</TableHead>
                <TableHead>Correo electrónico</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              {unidades.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3} className="italic text-muted-foreground">
                    No hay unidades registradas.
                  </TableCell>
                </TableRow>
              )}
              {unidades.map((unidad) =>
                unidad.unidadId === editandoId ? (
                  <TableRow key={unidad.unidadId}>
                    <TableCell>
                      <Input value={editDescripcion} onChange={(e) => setEditDescripcion(e.target.value)} />
                    </TableCell>
                    <TableCell>
                      <Input type="email" value={editCorreo} onChange={(e) => setEditCorreo(e.target.value)} />
                    </TableCell>
                    <TableCell className="space-x-2">
                      <Button size="sm" onClick={handleGuardarEdicion} disabled={editar.isPending}>
                        Guardar
                      </Button>
                      <Button size="sm" variant="outline" onClick={() => setEditandoId(null)}>
                        Cancelar
                      </Button>
                    </TableCell>
                  </TableRow>
                ) : (
                  <TableRow key={unidad.unidadId}>
                    <TableCell>{unidad.descripcion}</TableCell>
                    <TableCell>{unidad.correoElectronico}</TableCell>
                    <TableCell>
                      <Button size="sm" variant="outline" onClick={() => iniciarEdicion(unidad)}>
                        Editar
                      </Button>
                    </TableCell>
                  </TableRow>
                ),
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
