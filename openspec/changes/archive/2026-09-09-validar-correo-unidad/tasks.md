## 1. Backend: utilidad de validación de correo

- [x] 1.1 Crear `EmailUtils.EsFormatoValido(string? correo)` en `src/LicenciasMedicas.Core/Correos/EmailUtils.cs` (regex compilada, con `Trim()`), siguiendo el estilo de `RutUtils` (`src/LicenciasMedicas.Core/Rut/RutUtils.cs`), y verificar que compila con `dotnet build`.
- [x] 1.2 Agregar `tests/LicenciasMedicas.Core.Tests/EmailUtilsTests.cs` cubriendo: correo válido, sin `@`, dominio sin punto, cadena vacía, `null`, y correo con espacios alrededor que sí es válido tras el trim; verificar que `dotnet test` pasa.

## 2. Backend: endpoints de unidades

- [x] 2.1 En `UnidadesEndpoints.cs`, actualizar `CrearUnidadRequest` y `EditarUnidadRequest` para que `CorreoElectronico` sea `string` no nulo (ya no `string?`).
- [x] 2.2 En el handler `POST /api/unidades`, agregar la validación de correo (ausente o formato inválido vía `EmailUtils.EsFormatoValido`) devolviendo `400` con `{ error: "..." }`, junto al chequeo existente de `Descripcion`; verificar manualmente con una solicitud sin correo y con una solicitud con correo inválido.
- [x] 2.3 Replicar la misma validación en el handler `PUT /api/unidades/{id}`, incluyendo el caso de una unidad existente que no tenía correo guardado; verificar manualmente editando una unidad sin correo previo.
- [x] 2.4 Ajustar `NormalizarVacio`/llamadas relacionadas para que ya no se necesite convertir el correo vacío en `null` en la ruta de escritura (el correo vacío ahora es un error, no un valor válido a normalizar); verificar que `dotnet build` compila sin advertencias de nulabilidad.

## 3. Frontend: formulario de unidades

- [x] 3.1 En `UnidadesPage.tsx`, agregar una función `esCorreoValido(correo: string)` (regex equivalente a `EmailUtils`) usada por `handleAgregar` y `handleGuardarEdicion`.
- [x] 3.2 En `handleAgregar`, validar que `nuevoCorreo` no esté vacío y tenga formato válido antes de llamar a `crear.mutate()`, mostrando `toast.error` con un mensaje claro en caso contrario (mismo patrón que el chequeo de `nuevaDescripcion`).
- [x] 3.3 En `handleGuardarEdicion`, aplicar la misma validación sobre `editCorreo` antes de llamar a `editar.mutate()`, incluyendo el caso de editar una unidad que llegó con `correoElectronico` vacío desde el backend.
- [x] 3.4 Verificar manualmente en el navegador (`npm run dev`): agregar una unidad sin correo (bloqueado con toast), agregar con correo inválido (bloqueado con toast), agregar con correo válido (se crea), y editar una unidad legacy sin correo forzando a completarlo.

## 4. Verificación de extremo a extremo

- [x] 4.1 Ejecutar `dotnet build` sobre la solución completa y `dotnet test` sobre `tests/LicenciasMedicas.Core.Tests` y verificar que todo pasa.
- [x] 4.2 Ejecutar `npm run build` en `src/LicenciasMedicas.Web.Client` y verificar que compila sin errores de tipos.
- [x] 4.3 Probar manualmente los escenarios descritos en `specs/unidades/spec.md` (crear con/sin correo válido, editar con/sin correo válido, edición de unidad legacy sin correo, descripción vacía/duplicada, listado incluyendo unidad legacy sin correo) contra la app corriendo localmente.
