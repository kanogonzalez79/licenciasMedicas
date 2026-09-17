## Context

Ver proposal.md - Why. Referencia de código existente:
- `ConfiguracionSmtpService` guarda la configuración SMTP en una fila única (`ConfiguracionSmtp`, `Id = 1`) — no aplica acá porque esto es una lista de N correos, no un registro único.
- `CorreoEnvioService.Enviar` (`src/LicenciasMedicas.Core/Correos/CorreoEnvioService.cs`) es el único punto que construye el `MimeMessage` y hace el envío real; es el único lugar que necesita conocer los correos en copia.
- `EmailUtils.EsFormatoValido` ya valida formato de correo y se reutiliza tal cual.
- `ConfiguracionEndpoints.cs` (`/api/configuracion/smtp`) es el patrón de referencia para agrupar endpoints de configuración con `MapGroup`.
- `ConfiguracionPage.tsx` es la pantalla donde vive hoy la card de SMTP; la nueva sección se agrega ahí como una card independiente, sin tocar la existente.

## Goals / Non-Goals

**Goals:**
- Persistir una lista de correos en copia, cada uno con estado activo/inactivo.
- Aplicar esos correos como CC en todo envío de `CorreoEnvioService.Enviar`, sin distinguir por unidad.
- Rechazar duplicados (activos o inactivos) al agregar, sin reactivar automáticamente.

**Non-Goals:**
- No se agrega CC por unidad ni reglas de CC condicionales — es una lista plana, global, todo-o-nada por correo.
- No se toca `ConfiguracionSmtp` ni el modelo de `Unidades`.
- No se envían correos desde ningún otro flujo (el informe Excel no envía correo hoy y sigue sin hacerlo).

## Decisions

### Tabla dedicada `CorreoCopia`, no una columna en `ConfiguracionSmtp`
Es una lista de 0..N elementos con estado propio por elemento; una fila única (como `ConfiguracionSmtp`) obligaría a serializar la lista en un campo de texto (CSV/JSON), perdiendo la capacidad de indexar/validar duplicados en SQL y de togglear un elemento sin reescribir todo el campo.

```sql
CREATE TABLE CorreoCopia (
    CorreoCopiaId     INTEGER PRIMARY KEY AUTOINCREMENT,
    CorreoElectronico TEXT NOT NULL,
    Activo            INTEGER NOT NULL DEFAULT 1
);
CREATE UNIQUE INDEX UX_CorreoCopia_CorreoElectronico ON CorreoCopia (LOWER(CorreoElectronico));
```
Nueva migración `003_correo_copia.sql`, siguiendo la numeración secuencial ya usada por `001_init.sql` / `002_correo_smtp.sql`.

El servicio recorta (`Trim()`) la dirección antes de guardar, así que el índice único sobre `LOWER(CorreoElectronico)` alcanza para detectar duplicados por may/min sin duplicar la normalización en dos lugares.

### Validar duplicados en el servicio, no solo con la constraint de la base
La constraint UNIQUE es la garantía de integridad final, pero el servicio hace la búsqueda explícita antes del INSERT para poder distinguir el caso "ya existe" (mensaje de negocio claro) de cualquier otro error inesperado de base de datos, y para poder decidir explícitamente que un duplicado inactivo se rechaza en vez de reactivarse (ver proposal.md).

### Eliminar es un DELETE físico; desactivar es un UPDATE de `Activo`
Son dos operaciones distintas a propósito: "eliminar" libera la dirección para poder volver a agregarla como registro nuevo; "desactivar" la deja fuera de los envíos pero conservada en la lista (incluyendo su historial de haber existido), sin liberar la dirección para un alta nueva — un intento de re-agregarla mientras está inactiva se rechaza como duplicado, tal como decide la capability.

### Nuevo servicio `CorreoCopiaService`, separado de `ConfiguracionSmtpService`
Aunque ambos viven en `LicenciasMedicas.Core.Correos` y se muestran en la misma pantalla, son conceptos independientes (transporte SMTP vs. lista de destinatarios en copia) con ciclos de vida y validaciones distintas — separarlos evita que un servicio crezca mezclando dos responsabilidades.

### Endpoints bajo `/api/configuracion/correos-copia`, agrupados con `MapGroup`
Mismo patrón que `ConfiguracionEndpoints.cs`: `GET /` (listar), `POST /` (agregar), `PATCH /{id}` (activar/desactivar), `DELETE /{id}` (eliminar). Vive en un archivo nuevo `CorreosCopiaEndpoints.cs` en vez de agregarse al archivo de SMTP, por la misma separación de responsabilidades.

### `CorreoEnvioService.Enviar` agrega los CC al armar el `MimeMessage`
Se inyecta `CorreoCopiaService` en `CorreoEnvioService` (mismo patrón de constructor que ya usa para `UnidadesRepository` y `ConfiguracionSmtpService`) y, después de `mensaje.To.Add(...)`, se agrega un `mensaje.Cc.Add(...)` por cada correo activo. Si la lista de activos está vacía, simplemente no se agrega ningún CC — no es un caso de error.

### Frontend: card independiente en `ConfiguracionPage.tsx`, reutilizando el patrón de lista de `UnidadesPage.tsx`
Input + botón "Agregar" para el alta, y una tabla con una fila por correo (dirección, switch o checkbox de activo/inactivo, botón eliminar) — mismo patrón visual que la lista de unidades, sin introducir un componente nuevo de UI salvo que no exista ya un control de toggle (se evalúa en `tasks.md`/implementación si se usa un checkbox como el de `usaSsl` o se agrega el componente `Switch` de shadcn/ui).

## Risks / Trade-offs

- **Correo en copia mal escrito pero con formato válido** (ej. typo en el dominio) → No hay mitigación automática posible (no se puede validar existencia real de un correo); queda a criterio de quien lo configura, igual que ya ocurre con el remitente y el correo de unidad.
- **Alguien agrega el mismo correo que ya es el destinatario ("To") de alguna unidad** → No se deduplica contra los destinatarios de unidades; el correo llegaría duplicado (To + CC) en ese caso puntual. No se mitiga porque son dos conceptos independientes (destinatario de unidad vs. lista global de copia) y detectarlo en tiempo de envío agregaría complejidad para un caso borde poco probable y sin impacto real (el destinatario simplemente recibe el aviso dos veces, una en To y otra en CC).

## Migration Plan

- Nueva migración `003_correo_copia.sql`, aplicada automáticamente al iniciar la app (`Migrator.Migrar`), igual que las anteriores — no requiere pasos manuales ni afecta datos existentes.
- Sin rollback especial: es una tabla nueva sin relación con datos existentes; en caso de reversión, basta con no usar la nueva capability (la tabla queda vacía y sin efecto sobre `CorreoEnvioService` si no se agregó ningún correo en copia).
