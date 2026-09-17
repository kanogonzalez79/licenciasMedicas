## 1. Datos

- [ ] 1.1 Crear `src/LicenciasMedicas.Core/Data/Migrations/004_licencia_correo_enviado.sql` con `ALTER TABLE Licencias ADD COLUMN CorreoEnviado INTEGER NOT NULL DEFAULT 0` y verificar que `dotnet test tests/LicenciasMedicas.Core.Tests/LicenciasMedicas.Core.Tests.csproj --filter MigratorTests` sigue pasando (la migración se aplica sin error sobre una base nueva y sobre una existente).
- [ ] 1.2 Agregar `CorreoEnviado` (bool) a la entidad `Licencia` y verificar que compila y que `LicenciasRepositoryTests` existentes siguen pasando.

## 2. Backend: grabado con el flag

- [ ] 2.1 Incluir `CorreoEnviado` en el `INSERT` de `LicenciasRepository.Insertar` y agregar un test en `LicenciasRepositoryTests.cs` que graba una licencia con `CorreoEnviado = true` y otra sin marcar, y verifica que cada una se recupera con el valor correcto.
- [ ] 2.2 Agregar `CorreoEnviado` a `DatosLicenciaManual` (default `false`) y asignarlo en `IngresoManualService.Ingresar` antes de insertar; agregar un test en `IngresoManualServiceTests.cs` que verifica que una licencia ingresada con el flag marcado queda grabada con `CorreoEnviado = true`.
- [ ] 2.3 Extender `AsignacionUnidad` con `CorreoEnviado` (bool) y usarlo en `ProcesamientoService.Grabar` al construir cada `Licencia`; agregar un test en `ProcesamientoServiceTests.cs` que graba un lote con algunas filas marcadas y verifica el valor grabado de cada una.

## 3. Backend: exclusión en redactar/enviar correo

- [ ] 3.1 Agregar `AND l.CorreoEnviado = 0` al `WHERE` de `LicenciasRepository.UnidadesConLicenciasGrabadasEnFecha` y `LicenciasDeUnidadGrabadasEnFecha`.
- [ ] 3.2 Agregar tests en `CorreoRedaccionServiceTests.cs` (o `LicenciasRepositoryTests.cs` si aplica más directo) que verifiquen: (a) una licencia marcada como `CorreoEnviado = true` no aparece en el texto de aviso ni en el conteo de su unidad y fecha; (b) si todas las licencias de una unidad y fecha están marcadas, la unidad no aparece en `UnidadesConLicenciasGrabadasEnFecha` para esa fecha.

## 4. Backend: edición posterior del flag

- [ ] 4.1 Agregar `LicenciasRepository.ActualizarCorreoEnviado(connection, licenciaId, correoEnviado)` (UPDATE puntual) y un test que verifica que cambia el valor de una licencia ya grabada, en ambos sentidos (marcar y desmarcar).
- [ ] 4.2 Agregar `PATCH /api/licencias/{id}/correo-enviado` en `LicenciasEndpoints.cs`, siguiendo el patrón de `CorreosCopiaEndpoints.MapPatch`, con un request `{ correoEnviado: bool }`; devolver 404 si la licencia no existe.
- [ ] 4.3 Agregar un test de integración (o unitario del endpoint si el proyecto ya tiene ese patrón) que verifica que el `PATCH` cambia el valor persistido y que responde 404 para un id inexistente.

## 5. Frontend: ingreso manual

- [ ] 5.1 Agregar el checkbox "Correo ya enviado" a `IngresoManualModal` y su tipo correspondiente en `lib/api.ts`, y verificar manualmente en el navegador (`npm run dev`) que una licencia grabada con el checkbox marcado aparece luego en Búsqueda con el nuevo indicador.

## 6. Frontend: grilla de revisión del ingreso automático

- [ ] 6.1 Agregar una columna de checkbox "Correo enviado" por fila en `IngresarPage.tsx` (`FilaPendiente`), incluir el valor en el payload de `procesamientoApi.grabar`, y verificar manualmente que al grabar, el valor de cada fila queda reflejado en la licencia grabada.
- [ ] 6.2 Agregar el botón/acción "Marcar lote como correo enviado" que setea el estado local de todas las filas pendientes en `true`, permitiendo seguir editando filas individuales después; verificar manualmente el escenario "marcar todo el lote y luego desmarcar una fila".

## 7. Frontend: detalle de licencia y búsqueda

- [ ] 7.1 Agregar en `licencia-detalle-modal.tsx` un control para ver y cambiar el estado de "correo enviado", conectado al endpoint `PATCH` nuevo, y verificar manualmente que el cambio se refleja de inmediato en el modal y persiste al reabrirlo.
- [ ] 7.2 Agregar el badge "Correo enviado" en `BuscarPage.tsx` cuando el flag esté en `true` (sin badge para el caso pendiente), y verificar manualmente en el navegador que se muestra solo para licencias marcadas.

## 8. Verificación final

- [ ] 8.1 Ejecutar `dotnet test tests/LicenciasMedicas.Core.Tests/LicenciasMedicas.Core.Tests.csproj` completo y confirmar que todo pasa.
- [ ] 8.2 Ejecutar `npm run lint` en `src/LicenciasMedicas.Web.Client` y confirmar que no hay errores nuevos.
- [ ] 8.3 Probar manualmente el flujo end-to-end descrito en proposal.md: ingresar una licencia atrasada (manual y vía parser) marcada como "correo enviado", confirmar que no aparece en Redactar/Enviar Correo del día, y luego cambiar su estado desde el detalle.
