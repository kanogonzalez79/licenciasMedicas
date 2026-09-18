## Why

Hoy no hay ninguna vista que resuma de un vistazo el volumen y la distribución de las licencias médicas ingresadas; para saber cuántas licencias hay en un período, cómo se reparten por tipo o por unidad, o cómo evoluciona el volumen en el tiempo, hay que abrir el Excel de informe y armar tablas dinámicas a mano. Un dashboard con esa información ya agregada agiliza la revisión periódica sin salir de la aplicación.

## What Changes

- Se agrega una nueva página "Dashboard" en el grupo "Licencias" del menú, junto a "Informe".
- La página pide primero un rango de fechas (desde/hasta) y un selector de "a qué fecha aplica el rango" (inicio, término o intersección de rangos), igual que el flujo de Informe. El contenido del dashboard solo se calcula/muestra después de aplicar el rango, y se puede reajustar sin salir de la página.
- Una vez aplicado el rango, se muestran:
  - **Volumen**: total de licencias, total de días de reposo, promedio de días por licencia.
  - **Distribución**: ranking por tipo de licencia, ranking por unidad, proporción de ingresos manuales vs. leídos por el parser automático.
  - **Tendencia temporal**: cantidad de licencias agrupadas en el tiempo dentro del rango, con granularidad automática (día, semana o mes) según el largo del rango consultado.
- Se agrega un nuevo endpoint `GET /api/licencias/dashboard` que reutiliza la misma consulta que hoy arma el Excel de informe (mismos parámetros: fechaDesde, fechaHasta, modo) pero devuelve el listado de licencias en JSON en vez de un archivo `.xlsx`; toda la agregación (por tipo, por unidad, por período) se calcula en el cliente.
- Se agrega Recharts como nueva dependencia de `LicenciasMedicas.Web.Client` para los gráficos de barras/ranking y de tendencia temporal.

## Capabilities

### New Capabilities
- `dashboard-licencias`: pantalla de dashboard con gate de rango de fechas (y modo de aplicación), endpoint JSON de datos agregables, y los bloques de Volumen, Distribución y Tendencia temporal.

### Modified Capabilities
- `navegacion-menu`: el grupo "Licencias" del menú lateral pasa a incluir también la página "Dashboard".

## Impact

- Backend: nuevo endpoint en `LicenciasEndpoints.cs` (`GET /api/licencias/dashboard`), reutilizando `LicenciasRepository.ObtenerParaInforme`. No hay cambios de esquema de base de datos.
- Frontend: nueva página `DashboardPage.tsx`, entrada nueva en `app-sidebar.tsx` y `routes.tsx`, nueva dependencia `recharts` en `package.json`.
- No afecta el flujo Procesar → staging → Grabar ni el archivado de documentos.
