## 1. Validación técnica del selector de carpeta

- [x] 1.1 Prototipar mostrar un diálogo nativo "Buscar carpeta" de Windows desde un endpoint del backend ASP.NET Core (en un hilo STA dedicado) y verificar manualmente que se abre visible y en foco, y que devuelve de forma confiable la ruta elegida o el estado "cancelado".

## 2. Snapshot consistente de la base de datos

- [x] 2.1 Implementar una función que ejecute `VACUUM INTO` hacia un archivo temporal en la carpeta destino y luego lo reemplace atómicamente como `licencias.db`, y verificar con una prueba que el archivo resultante es una base SQLite válida y legible.
- [x] 2.2 Verificar con una prueba que ejecutar el snapshot mientras hay otra conexión abierta a `licencias.db` (simulando la app en uso) no falla ni deja un archivo corrupto en destino.

## 3. Sincronización espejo de `archivo/`

- [x] 3.1 Implementar una función que liste las rutas relativas de PDFs en origen y en destino, y verificar con una prueba que identifica correctamente cuáles faltan en destino y cuáles ya no existen en origen.
- [x] 3.2 Implementar la copia hacia destino de los PDFs que faltan, y verificar con una prueba que los archivos ya presentes en destino no se vuelven a copiar.
- [x] 3.3 Implementar el borrado en destino de los PDFs que ya no existen en origen, ejecutado después de completar las copias y el snapshot de la base de datos (ver Decisión de orden en design.md), y verificar con una prueba que ese orden se respeta.

## 4. Endpoint de respaldo

- [x] 4.1 Crear el endpoint de respaldo que abre el selector de carpeta y, si la persona usuaria cancela sin elegir una, responde sin modificar nada en ninguna carpeta; verificar con una prueba/manualmente que no se escribe ni borra nada en ese caso.
- [x] 4.2 Orquestar en el endpoint: validar que la carpeta destino elegida existe y es escribible, ejecutar el snapshot de la base de datos, sincronizar los PDFs, y devolver un resumen con la cantidad de PDFs copiados y eliminados; verificar con una prueba de integración que el resumen coincide con los cambios reales hechos en destino.
- [x] 4.3 Manejar el caso en que la carpeta destino deja de estar disponible durante el respaldo (por ejemplo, el disco externo se desconecta), devolviendo un error claro; verificar simulando la falla (por ejemplo, apuntando a una ruta que se vuelve inválida a mitad de la ejecución) y confirmando que el orden de operaciones (copiar/sobrescribir antes de borrar) limita el daño a "datos de más", nunca "datos de menos".

## 5. Interfaz de usuario

- [x] 5.1 Agregar en la interfaz un punto de entrada "Respaldar" que invoque el endpoint y muestre un estado "en progreso" mientras se ejecuta, deshabilitando el control durante ese tiempo; verificar manualmente.
- [x] 5.2 Mostrar el resumen del resultado (PDFs copiados, PDFs eliminados) al finalizar exitosamente, y verificar manualmente que los números coinciden con lo ocurrido en la carpeta destino.
- [x] 5.3 Mostrar un mensaje de error claro para cada caso de falla (carpeta cancelada, carpeta no disponible, error inesperado durante el proceso), y verificar manualmente cada uno.

## 6. Verificación end-to-end

- [x] 6.1 Con un disco externo real: ejecutar un respaldo, agregar una licencia nueva y eliminar otra existente, y ejecutar un segundo respaldo hacia la misma carpeta; verificar manualmente que el segundo respaldo solo copia el PDF nuevo, elimina el PDF de la licencia borrada, y que `licencias.db` en destino refleja el estado actual.
