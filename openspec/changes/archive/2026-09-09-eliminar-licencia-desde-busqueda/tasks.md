## 1. Backend: borrado de la licencia

- [x] 1.1 Agregar `LicenciasRepository.Eliminar(SqliteConnection connection, int licenciaId)` que borra la fila en `Licencias` por `LicenciaId` y devuelve `true`/`false` según si afectó alguna fila
- [x] 1.2 Agregar `DELETE /api/licencias/{id:int}` en `LicenciasEndpoints`: busca la licencia con `repo.ObtenerPorId`, devuelve 404 si no existe; si existe, intenta borrar el archivo en `Path.Combine(paths.ArchivoDir, licencia.RutaPdfArchivado)` sin fallar si no existe o si el borrado falla, luego llama a `repo.Eliminar` y devuelve `Results.Ok()`
- [x] 1.3 Verificar con una prueba (o prueba manual con la app corriendo) que: eliminar una licencia existente hace que deje de aparecer en `GET /api/licencias`, que su PDF deje de estar disponible en `GET /api/licencias/{id}/pdf`, y que eliminar un id inexistente responde 404

## 2. Frontend: cliente API

- [x] 2.1 Agregar `licenciasApi.eliminar(id: number)` en `lib/api.ts` que llama a `DELETE /api/licencias/{id}` y verificar que compila sin errores de tipos

## 3. Frontend: componente de confirmación

- [x] 3.1 Agregar `components/ui/alert-dialog.tsx` (primitivo shadcn, mismo estilo que `dialog.tsx`) y verificar que se puede importar sin errores

## 4. Frontend: integración en la búsqueda

- [x] 4.1 En `BuscarPage.tsx`, agregar el control "Eliminar" en cada fila de resultados, junto a "Ver PDF" y "Ver ficha"
- [x] 4.2 Al hacer clic en "Eliminar", mostrar el `AlertDialog` de confirmación con los datos de la licencia (folio y/o nombre del paciente) antes de ejecutar el borrado
- [x] 4.3 Al confirmar, ejecutar una mutación (`useMutation`) que llama a `licenciasApi.eliminar`, muestra un toast de éxito o error (`sonner`, igual que en `UnidadesPage`), e invalida el query `["licencias", "buscar"]` para refrescar los resultados
- [x] 4.4 Al cancelar el diálogo, verificar que no se ejecuta ninguna llamada al API y que la fila permanece en los resultados
- [x] 4.5 Verificar manualmente en el navegador: buscar licencias, eliminar una, confirmar que desaparece de los resultados y que el folio queda libre para reingresarla (reprocesando el mismo PDF), y que cancelar el diálogo no elimina nada
