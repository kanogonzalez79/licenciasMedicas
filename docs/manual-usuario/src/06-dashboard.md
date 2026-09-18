---
grupo: Licencias
capabilities_openspec:
  - dashboard-licencias
---

# Dashboard

Ofrece una vista de resumen del volumen, la distribución y la tendencia de las licencias médicas en un rango de fechas, sin tener que armar esos totales a mano en el Excel de [Informe](#sec-05-informe).

Se eligen los mismos tres controles que en Informe — **Fecha desde**, **Fecha hasta** y **El rango aplica a** (fecha de inicio, fecha de término o intersección de rangos) — y se presiona **Aplicar**. Mientras no se aplique un rango, el dashboard no muestra ningún contenido.

![Dashboard con las tarjetas de totales y los gráficos de distribución](01-resumen-completo.png)

## Qué muestra

- **Totales**: cantidad total de licencias del rango, total de días de reposo (sumando los días completos de cada licencia) y promedio de días por licencia. Si el rango no tiene licencias, se muestra un mensaje indicándolo en vez de un promedio sin sentido.
- **Por tipo de licencia** y **Por unidad**: dos gráficos de barras con la cantidad de licencias agrupadas por cada categoría, ordenadas de mayor a menor. El ranking por unidad agrupa las menos frecuentes bajo "Otras" cuando hay muchas unidades distintas.
- **Ingreso manual vs. automático**: una barra de proporción que muestra cuántas licencias del rango se ingresaron a mano y cuántas se leyeron automáticamente desde PDF.
- **Tendencia en el tiempo**: un gráfico de barras con la cantidad de licencias distribuida en el tiempo. La agrupación se elige automáticamente según el largo del rango consultado: por día si son 60 días o menos, por semana si es de 61 a 365 días, y por mes si supera un año — así el gráfico nunca queda ni demasiado apretado ni demasiado disperso.

::: consejo
Para comparar dos períodos (por ejemplo, este mes contra el anterior), basta con cambiar las fechas y volver a presionar Aplicar: todo el contenido se recalcula al instante, sin salir de la pantalla.
:::
