## Why

Hoy el campo "Correo electrónico" al agregar o editar una unidad no se valida en ningún lugar: el `<Input type="email">` del formulario no dispara la validación nativa del navegador (no está dentro de un `<form>`), y el endpoint del backend solo valida que la `Descripcion` no esté vacía. Como resultado, se puede guardar cualquier texto como correo de una unidad, lo que compromete la calidad del dato justo en el campo que el módulo de "Correos" necesitará para identificar a quién dirigir las licencias médicas de cada unidad.

## What Changes

- El correo electrónico pasa a ser **obligatorio** al crear o editar una unidad (antes era opcional/nullable en la escritura).
- Se valida el formato del correo con una regex pragmática (texto@dominio.tld), tanto en el formulario de React (`UnidadesPage.tsx`) como en los endpoints del backend (`UnidadesEndpoints.cs`), replicando el estilo de validación manual ya usado para `Descripcion`.
- El backend rechaza con `400 Bad Request` (mismo formato `{ error: "..." }` que el chequeo de `Descripcion`) cuando el correo falta o no tiene formato válido, tanto en `POST /api/unidades` como en `PUT /api/unidades/{id}`.
- El frontend muestra un `toast.error` y no dispara la mutación si el correo falta o es inválido, tanto en el formulario de "Agregar unidad" como en la fila en modo edición.
- **BREAKING** (a nivel de contrato de escritura): `CrearUnidadRequest.CorreoElectronico` y `EditarUnidadRequest.CorreoElectronico` dejan de ser opcionales — cualquier consumidor de la API que hoy envía `null` o lo omite empezará a recibir `400`.
- No se modifica el esquema de la base de datos: la columna `Unidades.CorreoElectronico` sigue siendo `NULL`-able para no romper la lectura de unidades existentes que aún no tengan correo cargado. Esas unidades legacy solo son forzadas a completar un correo válido la próxima vez que se guarden (crear siempre lo exige; editar lo exige al guardar, incluso si la unidad ya existía sin correo).

## Capabilities

### New Capabilities
- `unidades`: gestión de unidades (crear, editar, listar), incluyendo la regla de que el correo electrónico es obligatorio y debe tener formato válido.

### Modified Capabilities
(ninguna — no existe spec previo de `unidades`)

## Impact

- **Frontend**: `src/LicenciasMedicas.Web.Client/src/pages/UnidadesPage.tsx` (validación en `handleAgregar` y `handleGuardarEdicion`).
- **Backend**: `src/LicenciasMedicas.Web/Endpoints/UnidadesEndpoints.cs` (`CrearUnidadRequest`, `EditarUnidadRequest`, validación en `POST` y `PUT`).
- **Sin cambios de esquema**: `src/LicenciasMedicas.Core/Data/Migrations/001_init.sql` no se toca; `Unidad.CorreoElectronico` (modelo de lectura) sigue siendo `string?`.
- **Sin cambios** en `UnidadesRepository.cs` más allá de seguir recibiendo un correo ya validado y no nulo desde el endpoint.
