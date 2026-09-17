## Why

El texto del correo de aviso de licencias separa las columnas de la tabla con tabulaciones (`\t`), pero tanto el `<Textarea>` donde se edita como la mayoría de clientes de correo muestran texto plano en fuente proporcional. El resultado es una tabla desalineada y difícil de leer, tanto al editar el texto como al recibirlo.

## What Changes

- La tabla de licencias del texto de correo pasa a usar columnas de ancho fijo (padding con espacios calculado según el valor más largo de cada columna, incluyendo el encabezado), con un separador fijo entre columnas.
- Se agrega una línea de guiones bajo el encabezado, del mismo ancho que cada columna, para reforzar el aspecto de tabla.
- El `<Textarea>` de edición en la pantalla "Redactar correo" pasa a usar fuente monoespaciada, para que la alineación se vea igual mientras se edita y al enviar/copiar.
- No cambia el formato de las fechas (siguen en `yyyy-MM-dd`, igual que en el resto de la app) ni el modelo de texto plano editable de punta a punta.

## Capabilities

### New Capabilities
- `redactar-correo`: genera el texto del correo de aviso (saludo + tabla de licencias) para una unidad y fecha, con la tabla alineada en columnas de ancho fijo.

### Modified Capabilities
(ninguna — `enviar-correo` no cambia: sigue recibiendo el texto ya editable tal como lo genera `redactar-correo`)

## Impact

- `src/LicenciasMedicas.Core/Correos/CorreoRedaccionService.cs`: lógica de armado de la tabla (cálculo de anchos, padding, línea separadora).
- `src/LicenciasMedicas.Web.Client/src/pages/RedactarCorreoPage.tsx`: clase de fuente monoespaciada en el `Textarea`.
- Pruebas nuevas para `CorreoRedaccionService` (hoy sin cobertura).
