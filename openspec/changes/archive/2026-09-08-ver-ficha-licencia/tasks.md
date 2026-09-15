## 1. Componente Dialog reutilizable

- [x] 1.1 Crear `src/LicenciasMedicas.Web.Client/src/components/ui/dialog.tsx` envolviendo `@base-ui/react/dialog` (overlay + contenido centrado), exportando `Dialog`, `DialogTrigger`, `DialogContent`, `DialogHeader`, `DialogFooter`, `DialogTitle`, `DialogDescription`, siguiendo la estructura de `sheet.tsx`; verificar que compila sin errores de tipos (`npm run build` o `tsc --noEmit` en el cliente).
- [x] 1.2 Verificar manualmente que el `Dialog` abre centrado con overlay, cierra con el botón de cierre, con click fuera y con `Esc`.

## 2. Ficha de detalle de licencia

- [x] 2.1 Crear componente `LicenciaDetalleModal` (o similar) en `src/pages/` o `src/components/` que reciba una `Licencia` y renderice sus campos agrupados en las secciones Identificación, Paciente, Licencia médica, Profesional y Unidad, excluyendo `rutaPdfArchivado`, `nombreArchivoOriginal` y `hashArchivoSha256`.
- [x] 2.2 Manejar campos opcionales (`apellidoMaternoPaciente`, `edadPaciente`, `sexoPaciente`, `codigoTipoLicencia`, `descripcionTipoLicencia`, `fechaEmisionOtorgamiento`, `correoProfesional`, `especialidadProfesional`, `observaciones`) mostrando un indicador claro (por ejemplo "—" o "No informado") cuando el valor es `null`.
- [x] 2.3 Verificar visualmente con al menos una licencia que tenga campos opcionales vacíos y otra que los tenga todos completos.

## 3. Integración en BuscarPage

- [x] 3.1 Agregar estado `licenciaSeleccionada: Licencia | null` en `BuscarPage.tsx` y renderizar `LicenciaDetalleModal` controlado por ese estado.
- [x] 3.2 Agregar botón "Ver ficha" en la columna de acciones de la tabla de resultados, junto a "Ver PDF", que setea `licenciaSeleccionada` a la licencia de esa fila; verificar que cada fila abre la ficha correcta (comparando folio/RUT mostrados contra la fila de origen).
- [x] 3.3 Verificar que cerrar el modal no altera los filtros de búsqueda activos ni la página/resultados actuales.
- [x] 3.4 Verificar que "Ver ficha" y "Ver PDF" funcionan de forma independiente (abrir ficha sin haber abierto el PDF, y viceversa).

## 4. Verificación final

- [x] 4.1 Ejecutar la app en modo desarrollo, buscar licencias con distintos filtros, y confirmar para varias filas que los datos mostrados en la ficha coinciden exactamente con los de la fila y con el registro en la base de datos SQLite.
- [x] 4.2 Ejecutar el build de producción del cliente y confirmar que no hay errores de tipos ni de lint relacionados a los archivos nuevos/modificados.
