## Context

`CorreoRedaccionService.Redactar` arma el texto del correo uniendo columnas con `\t` (`string.Join('\t', ...)`). Ese texto se edita en un `<Textarea>` de fuente proporcional y se envía como `TextPart("plain")` por `CorreoEnvioService` — ver proposal.md. No hay spec ni tests previos para este servicio.

## Goals / Non-Goals

**Goals:**
- Alinear la tabla usando padding de ancho fijo por columna, calculado en tiempo de generación.
- Que la alineación se vea igual en el `Textarea` de edición y en el correo/copiado final.

**Non-Goals:**
- No se cambia a correo HTML (se descartó en la exploración: rompe el modelo de texto editable de punta a punta y requiere tocar `CorreoEnvioService` y el flujo de envío/copiado).
- No se cambia el formato de fechas ni el orden/nombre de las columnas.
- No se trunca ni se envuelve el contenido de columnas muy largas.

## Decisions

- **Ancho de columna = máximo entre encabezado y valores de esa columna, en ese envío puntual** (no un ancho fijo global). Evita desperdiciar espacio cuando los nombres son cortos, y sigue alineando cuando son largos. Se recalcula cada vez que se genera el texto (no se persiste).
- **Separador entre columnas: 2 espacios fijos** después del padding de cada columna (gutter), igual para todas las columnas.
- **Alineación a la izquierda** para todas las columnas, incluida "Cantidad de Días" — consistente con el resto de la tabla y más simple que alinear números a la derecha; con una sola cifra de 1-2 dígitos no aporta legibilidad extra.
- **Línea separadora de guiones bajo el encabezado**: mismo ancho de columna y mismo separador que las filas, generada con el mismo cálculo de anchos.
- **Fuente monoespaciada en el `Textarea`** (clase `font-mono` de Tailwind, ya disponible en el proyecto vía shadcn/ui): es el único cambio de frontend; no se toca el modelo de edición de texto libre.
- El padding se basa en `string.Length` (conteo de caracteres, no de ancho visual). Aceptable porque los valores son RUT, nombres en español (con tildes/ñ) y fechas `yyyy-MM-dd`: todos caracteres de ancho fijo en fuente monoespaciada, sin CJK ni emoji.

## Risks / Trade-offs

- [Un cliente de correo que fuerce fuente proporcional en `text/plain`] → la tabla puede perder alineación en ese cliente puntual. Mitigación: es el trade-off aceptado al explorar (ver proposal.md); la alternativa (HTML) tiene un costo de rediseño mayor que no se justifica para esta herramienta de un solo usuario.
- [Nombres de paciente muy largos ensanchan toda la columna "Nombre" y por lo tanto la línea completa] → aceptado; no se trunca porque cortar un nombre sería peor que una línea larga.

## Migration Plan

No aplica — no hay datos persistidos ni versión de esquema involucrados; es un cambio de formato de texto generado en el momento.
