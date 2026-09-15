## Context

Ver `proposal.md` - Why. Puntos relevantes para el diseño:
- No existe capa de validación compartida en el proyecto (sin FluentValidation en backend, sin zod/yup en frontend); toda validación hoy es manual e inline en el endpoint (`UnidadesEndpoints.cs`) y en los handlers del formulario (`UnidadesPage.tsx`).
- Frontend (TypeScript/React) y backend (C#) son proyectos separados sin código compartido, por lo que cualquier regla de validación se duplica necesariamente en ambos lenguajes.
- La columna `Unidades.CorreoElectronico` es `TEXT NULL` y puede haber unidades ya creadas sin correo.

## Goals / Non-Goals

**Goals:**
- Rechazar correos con formato inválido o ausentes al crear/editar una unidad, en cliente y servidor.
- Mantener el estilo de validación ya existente en el proyecto (chequeos manuales inline, sin introducir librerías nuevas).
- No romper la lectura de unidades legacy sin correo guardado.

**Non-Goals:**
- No se valida deliverability real del correo (no se envía un correo de verificación ni se consulta MX/DNS).
- No se migra ni se hace backfill de datos existentes en la base de datos.
- No se agrega envío de correos (el módulo "Correos" sigue sin usar `CorreoElectronico` para enviar nada; solo mejora la calidad del dato para ese uso futuro).

## Decisions

### Regex pragmática, no RFC 5322 estricto
Se valida con un patrón simple del tipo `^\S+@\S+\.\S+$` (sin espacios, con `@` y al menos un punto en el dominio), en vez de una implementación estricta de RFC 5322.
- **Por qué**: atrapa errores de tipeo obvios (falta `@`, falta dominio) sin arriesgar falsos negativos sobre direcciones reales pero con formatos poco comunes. Consistente con el nivel de rigor ya usado en el proyecto (validaciones simples, sin librerías de terceros).
- **Alternativa descartada**: regex RFC 5322 completa — más compleja de mantener y de explicar, sin beneficio real dado que no hay envío de correos que dependa de esa exactitud.

### Backend: utilidad estática `EmailUtils.EsFormatoValido`, siguiendo el patrón de `RutUtils`
En vez de dejar la regex como una expresión inline dentro del lambda del endpoint, se extrae a una clase estática en `LicenciasMedicas.Core` (p. ej. `src/LicenciasMedicas.Core/Correos/EmailUtils.cs`) con un único método `EsFormatoValido(string? correo)`, que hace `Trim()` y aplica la regex.
- **Por qué**: el proyecto ya tiene exactamente este patrón para otra regla de formato — `RutUtils` (`src/LicenciasMedicas.Core/Rut/RutUtils.cs`), una clase estática con `Regex` compilada, probada con tests unitarios dedicados (`RutUtilsTests.cs`) sin necesidad de levantar un host HTTP. Seguirlo mantiene la validación testable de forma aislada y consistente con el estilo ya establecido, sin introducir una librería de validación nueva.
- **Alternativa descartada**: regex inline dentro del lambda del endpoint — no hay forma de testearla sin un test de integración con `WebApplicationFactory`, patrón que este proyecto no usa hoy (solo tiene `LicenciasMedicas.Core.Tests`, sin proyecto de tests para `LicenciasMedicas.Web`).

### Frontend: regla duplicada en TypeScript, sin capa compartida
La misma regla de formato se reimplementa en `UnidadesPage.tsx` (regex equivalente en TS, usada en `handleAgregar`/`handleGuardarEdicion` antes de disparar la mutación, mostrando `toast.error`).
- **Por qué**: no existe hoy ningún mecanismo de código compartido entre `LicenciasMedicas.Web.Client` y los proyectos .NET (son stacks distintos), y ya existe este mismo patrón de duplicación para la validación de `Descripcion` vacía (chequeo en el cliente + chequeo en el servidor). Introducir un mecanismo de código compartido solo para esta regla sería una abstracción prematura.
- **Alternativa descartada**: generar la regex desde una única fuente (p. ej. un JSON compartido) — over-engineering para una sola regla de validación.

### Sin cambio de esquema; regla solo a nivel de aplicación
`Unidades.CorreoElectronico` permanece `TEXT NULL` en la base de datos. La obligatoriedad se aplica únicamente en los endpoints de escritura (`POST`/`PUT`), no como `NOT NULL` en SQL.
- **Por qué**: decisión ya tomada en la exploración previa - evita una migración que tendría que decidir qué hacer con filas `NULL` existentes, y logra el mismo efecto práctico (toda unidad nueva o editada queda con correo válido) sin ese costo.
- **Consecuencia de diseño**: el modelo de lectura `Unidad.CorreoElectronico` (`Unidad.cs`) sigue siendo `string?`, mientras que los contratos de escritura `CrearUnidadRequest.CorreoElectronico` y `EditarUnidadRequest.CorreoElectronico` pasan a ser `string` no nulo. Lectura y escritura divergen intencionalmente.

### Validación de formato antes que unicidad/persistencia
En los endpoints, el chequeo de formato de correo se hace junto al chequeo existente de `Descripcion` vacía (antes de tocar la base de datos), devolviendo `400` con el mismo formato `{ error: "..." }`. El chequeo de descripción duplicada (`DescripcionDuplicadaException` → `409`) sigue ocurriendo después, a nivel de repositorio, sin cambios.
- **Por qué**: mantiene la capa de validación de entrada (formato) separada de la de integridad de datos (unicidad), igual que el código actual ya separa `IsNullOrWhiteSpace` (400, antes de la BD) de `DescripcionDuplicadaException` (409, detectado por la BD).

## Risks / Trade-offs

- **[Riesgo] Unidades legacy sin correo quedan "atascadas" si nadie las edita** → Mitigación: es el comportamiento deseado (ver exploración previa); no requiere acción adicional, pero vale documentarlo para quien administre unidades.
- **[Riesgo] La regex pragmática podría rechazar direcciones válidas pero atípicas** (p. ej. dominios muy cortos, TLD nuevos raros) → Mitigación: aceptable para este caso de uso interno; si aparece un caso real de falso rechazo, se ajusta la regex puntualmente.
- **[Trade-off] Duplicación de la regla en dos lenguajes** → Mitigación: mantener el patrón simple (una sola línea de regex en cada lado) minimiza el costo de mantenerlas sincronizadas; ambas se cubren con tests.
