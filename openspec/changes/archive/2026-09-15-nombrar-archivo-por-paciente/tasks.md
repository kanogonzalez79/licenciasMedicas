## 1. Normalización de nombre en AppPaths

- [x] 1.1 En `AppPaths.cs`, agregar un método privado (o utilidad interna) que normalice un nombre completo a Título Case: pasa primero a minúsculas y luego aplica capitalización por palabra, preservando tildes y "ñ". Verificar con casos manuales: "JUAN CARLOS PEREZ SOTO" → "Juan Carlos Perez Soto", "pEdRo GONZALEZ" → "Pedro Gonzalez", "JOSÉ NÚÑEZ" → "José Núñez".
- [x] 1.2 Cambiar la firma de `ResolverRutaArchivoPermanente` para recibir también `nombreCompletoPaciente`, y construir el nombre de archivo como `{nombreNormalizado} - {folioSeguro}{extension}`, reutilizando `Path.GetInvalidFileNameChars()` para sanitizar el nombre igual que ya se hace con el folio. Verificar que la carpeta por año/mes se mantiene igual que antes.
- [x] 1.3 Escribir tests unitarios directos sobre `ResolverRutaArchivoPermanente` (nuevo archivo de test o el que corresponda en `LicenciasMedicas.Core.Tests`) cubriendo: nombre en mayúsculas, nombre con tildes/ñ, nombre con caracteres inválidos para archivo, y dos folios distintos de la misma persona produciendo nombres de archivo distintos sin colisión.

## 2. Actualizar los llamadores

- [x] 2.1 En `ProcesamientoService.Grabar`, pasar `revision.NombreCompletoPaciente` al llamar a `ResolverRutaArchivoPermanente`. Verificar que `dotnet test` sobre `ProcesamientoServiceTests.cs` sigue pasando y que un test nuevo confirma que el archivo grabado queda con el nombre `{Paciente} - {Folio}.pdf` dentro de `data/archivo/{año}/{mes}/`.
- [x] 2.2 En `IngresoManualService.Ingresar`, pasar `datos.NombreCompletoPaciente` al llamar a `ResolverRutaArchivoPermanente`. Verificar que `dotnet test` sobre `IngresoManualServiceTests.cs` sigue pasando y que un test nuevo confirma el mismo esquema de nombre para el adjunto (PDF o imagen).

## 3. Verificación end-to-end

- [x] 3.1 Correr la suite completa (`dotnet test`) y confirmar que no quedan referencias rotas a la firma anterior de `ResolverRutaArchivoPermanente` (ej. en `LicenciasMedicas.Web` si la llama directamente).
- [x] 3.2 Probar manualmente en la app: procesar y grabar un PDF real de prueba, e ingresar una licencia manual con un adjunto, y confirmar visualmente en `data/archivo/{año}/{mes}/` que ambos archivos quedan nombrados como `{Nombre Paciente} - {Folio}{extensión}`.
