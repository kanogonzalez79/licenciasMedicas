---
grupo: Sistema
capabilities_openspec:
  - respaldo-datos
---

# Respaldo

Permite copiar manualmente los datos ya grabados de forma permanente — la base de datos y los documentos archivados — hacia una carpeta elegida por la persona usuaria, típicamente un disco externo, para protegerlos ante una falla del disco donde corre el sistema.

![Pantalla de Respaldo, con el botón Respaldar y su explicación](01-vista-general.png)

## Cómo respaldar

Al presionar **Respaldar**, el sistema abre un selector de carpeta del sistema operativo para elegir el destino. Si se cierra el selector sin elegir una carpeta, no se ejecuta ningún respaldo ni se modifica nada.

Elegida la carpeta, el sistema:

1. Copia hacia el destino los documentos archivados que falten (los que ya están copiados de un respaldo anterior no se vuelven a copiar).
2. Copia una versión consistente y actualizada de la base de datos, reemplazando la copia del respaldo anterior.
3. Elimina en el destino los documentos que ya no correspondan a ninguna licencia vigente (por ejemplo, de licencias que fueron eliminadas desde el último respaldo), para que la carpeta destino quede siempre como un espejo del estado actual.

Al terminar, se muestra un resumen con la cantidad de documentos copiados y eliminados en el destino.

::: importante
El respaldo **no incluye** las licencias todavía pendientes de revisión ni los PDF que siguen sin procesar en la carpeta de entrada — solo lo que ya quedó archivado de forma permanente con **Grabar**.
:::

::: nota
Si la carpeta destino deja de estar disponible durante el proceso (por ejemplo, se desconecta el disco externo), el sistema informa el error y no continúa, sin dejar la carpeta destino con menos datos de los que ya tenía antes de empezar.
:::
