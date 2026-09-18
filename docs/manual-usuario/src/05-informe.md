---
grupo: Licencias
capabilities_openspec:
  - informe-excel-licencias
---

# Informe

Genera y descarga un archivo Excel con el listado de licencias de un rango de fechas.

![Formulario de generación del informe Excel, con fecha desde, fecha hasta y modo](01-formulario.png)

## Parámetros

- **Fecha desde** y **Fecha hasta**: ambas obligatorias.
- **El rango aplica a**: define qué fecha de la licencia se compara contra el rango elegido. Hay tres modos, compartidos también con el [Dashboard](#sec-06-dashboard):
  - **Fecha de inicio** — incluye las licencias cuya fecha de inicio de reposo cae dentro del rango.
  - **Fecha de término** — incluye las licencias cuya fecha de término de reposo cae dentro del rango.
  - **Intersección de rangos** — incluye toda licencia cuyo período de reposo se solape con el rango en al menos un día, aunque ni su inicio ni su término estén dentro de él (por ejemplo, una licencia larga que empezó antes del rango consultado y todavía no termina).

Al presionar **Generar Excel**, el archivo se descarga con una fila por cada licencia que califica, en este orden de columnas: RUT del paciente (sin dígito verificador), dígito verificador, nombre completo del paciente, folio, descripción del tipo de licencia, fecha de inicio de reposo, fecha de término de reposo, fecha de otorgamiento y cantidad de días. Si ninguna licencia califica para el rango elegido, igualmente se genera el archivo, solo con la fila de encabezado.
