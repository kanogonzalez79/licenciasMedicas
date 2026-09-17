## 1. Backend — armado de la tabla alineada

- [x] 1.1 En `CorreoRedaccionService.Redactar`, calcular el ancho de cada columna (RUT, Nombre, Fecha Inicio, Fecha Término, Cantidad de Días) como el máximo entre el largo del encabezado y el largo de cada valor de esa columna en las licencias a incluir.
- [x] 1.2 Generar la fila de encabezado con cada valor alineado a la izquierda (`PadRight`) al ancho de su columna, separadas por 2 espacios.
- [x] 1.3 Generar la línea separadora de guiones bajo el encabezado, con el mismo ancho de columna y separador que el resto de la tabla.
- [x] 1.4 Generar cada fila de licencia con el mismo padding y separador que el encabezado.
- [x] 1.5 Verificar manualmente (`dotnet run --project src/LicenciasMedicas.Web` + llamar `GET /api/correos/...` o la UI) que el texto generado para una unidad con nombres de paciente de largo variable queda alineado en un editor/consola monoespaciada. Verificado con la salida impresa del test `Redactar_ConNombresDeLargoVariable_GeneraTablaAlineada` (`dotnet test ... -v n`): la tabla generada con datos reales (vía `Migrator`/`LicenciasRepository`, nombres de largo variable) queda alineada al inspeccionarla en la consola monoespaciada.

## 2. Frontend — edición con fuente monoespaciada

- [x] 2.1 Agregar la clase `font-mono` al `Textarea` de `RedactarCorreoPage.tsx` y confirmar visualmente en el navegador (`npm run dev`) que la tabla se ve alineada mientras se edita. Verificado en el navegador contra el backend de desarrollo con dos licencias ficticias de nombre corto y largo: encabezado, línea separadora y filas quedan alineados.

## 3. Pruebas

- [x] 3.1 Crear `tests/LicenciasMedicas.Core.Tests/CorreoRedaccionServiceTests.cs` con un caso de licencias de largos de nombre variables, verificando que encabezado, línea separadora y filas tengan exactamente el mismo largo de línea y que cada columna quede alineada en la misma posición de carácter en todas las filas.
- [x] 3.2 Agregar un caso de una sola licencia (columna de ancho mínimo = ancho del encabezado) y otro de unidad sin licencias en la fecha (debe seguir retornando `null`).
- [x] 3.3 Correr `dotnet test tests/LicenciasMedicas.Core.Tests/LicenciasMedicas.Core.Tests.csproj` y verificar que todos los tests pasen. 88/88 tests correctos (3 nuevos de `CorreoRedaccionServiceTests`).

## 4. Verificación final

- [x] 4.1 Ejecutar `openspec validate formato-tabla-correo --strict` y corregir cualquier hallazgo antes de aplicar el change. `Change 'formato-tabla-correo' is valid`.
