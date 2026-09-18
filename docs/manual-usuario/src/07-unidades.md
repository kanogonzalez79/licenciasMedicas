---
grupo: Sistema
capabilities_openspec:
  - unidades
---

# Unidades

Administra el listado de unidades organizacionales a las que se asignan las licencias médicas. Cada licencia grabada queda asociada a una unidad, y es esa asociación la que después se usa para armar y enviar los avisos por correo (ver [Redactar correo](#sec-04-redactar-correo)).

![Formulario para agregar una unidad y tabla con las unidades existentes](01-listado-y-alta.png)

## Agregar una unidad

Se completan dos campos, ambos obligatorios: **Descripción** (el nombre de la unidad) y **Correo electrónico** (la dirección a la que se le enviarán los avisos de licencias). La descripción debe ser única — el sistema rechaza crear o editar una unidad con un nombre que ya usa otra — y el correo debe tener un formato válido.

## Editar una unidad

El botón **Editar** en cada fila convierte esa fila en campos editables, con botones **Guardar** y **Cancelar**. Se aplican las mismas reglas que al crear: descripción obligatoria y única, correo obligatorio y con formato válido — incluso para unidades antiguas que todavía no tuvieran un correo registrado.

::: nota
No existe una acción para eliminar una unidad desde esta pantalla. Si una unidad ya no se usa, lo más simple es dejarla en el listado (no afecta el funcionamiento del resto del sistema) o renombrarla si corresponde reutilizarla para otra área.
:::
