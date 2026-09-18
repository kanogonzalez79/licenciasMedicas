---
grupo: Licencias
capabilities_openspec:
  - detalle-licencia
  - eliminar-licencia
  - licencia-correo-enviado
---

# Buscar licencia

Permite encontrar licencias ya grabadas en el sistema, revisar su detalle completo, abrir su documento original y, si corresponde, eliminarlas.

## Filtros de búsqueda

Todos los filtros son opcionales y se pueden combinar: Fecha inicio desde, Fecha inicio hasta, RUT paciente (sin puntos ni dígito verificador), Nombre paciente y Unidad. Los cambios en los filtros no buscan de inmediato — hay que presionar **Buscar** para aplicarlos.

![Filtros de búsqueda y tabla de resultados de licencias](01-filtros-resultados.png)

## Resultados

La tabla muestra, por cada licencia: Folio, RUT, Nombre completo, Tipo de licencia, Fecha inicio, Fecha término, Días, Unidad, Fecha de ingreso al sistema, **Origen** (una marca "Manual" o "Automático" según cómo se ingresó) y **Correo** (una marca "Correo enviado" cuando corresponde).

Por cada fila hay tres acciones disponibles:

- **Ver PDF** — abre en una pestaña nueva el documento original archivado de esa licencia.
- **Ver ficha** — abre la ficha de detalle completa (ver más abajo).
- **Eliminar** — pide confirmación antes de actuar; una vez confirmada, borra de forma **permanente e irreversible** el registro de la licencia y su documento archivado. El folio queda libre para volver a usarse si la licencia se reingresa.

## Ficha de detalle de una licencia

Muestra todos los datos de la licencia tal como están almacenados, organizados en cinco secciones: **Identificación** (folio, origen, tipo de formulario, fecha de ingreso al sistema, y el estado de correo enviado), **Paciente**, **Licencia médica**, **Profesional** y **Unidad**. Un campo sin valor guardado se muestra como *"No informado"*, en vez de dejarse en blanco.

![Ficha de detalle de una licencia, con sus secciones y el estado de correo enviado](02-ficha-detalle.png)

::: consejo
Desde la sección "Identificación" de la ficha se puede marcar o desmarcar en cualquier momento el estado de **correo enviado** de esa licencia puntual, sin necesidad de haberlo hecho al momento de ingresarla. Es útil para corregir una marca puesta por error, o para marcar después una licencia antigua cuyo aviso se mandó por fuera del sistema.
:::

Los campos codificados se muestran junto con su descripción, no solo el código: "Tipo de formulario" indica "Formulario tipo 1", "Formulario tipo 2" o "Manual"; "Sexo" indica "Masculino" o "Femenino".
