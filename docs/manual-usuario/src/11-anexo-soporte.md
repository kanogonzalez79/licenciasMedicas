# Anexo: soporte y solución de problemas

## "Procesé un lote y al reabrir el sistema desapareció"

Es el comportamiento esperado, no una falla: las licencias leídas y **todavía sin grabar** no sobreviven a un cierre o reinicio del sistema. Sus PDF vuelven automáticamente a la carpeta de entrada. Basta con presionar **Procesar** de nuevo en [Ingresar licencias](#sec-02-ingresar-licencias) para volver a leerlos.

## "No me deja grabar una licencia por el folio"

El sistema nunca permite dos licencias con el mismo folio, ni siquiera entre una ya grabada y otra pendiente de revisión. Si el folio corresponde efectivamente a una licencia distinta, hay que verificar el folio leído o tipeado; si corresponde al mismo documento que ya se ingresó antes, no hace falta volver a ingresarlo.

## "El envío de correo falla o quiero revisar qué pasó"

Cada intento de conexión al servidor de correo (tanto al usar "Probar conexión" en Configuración como al enviar un aviso real) queda registrado en un archivo de texto dentro de la carpeta de datos del sistema, con fecha, hora, servidor, usuario, si fue exitoso o no, y el motivo del error en caso de falla. Ese registro **nunca incluye la contraseña**, y se acumula entre reinicios del sistema — sirve para diagnosticar fallas intermitentes revisando qué pasó exactamente en cada intento. Para revisarlo, hay que pedir la ruta de la carpeta de datos a la persona encargada del soporte técnico del sistema.

## Motivos de error al procesar o ingresar un PDF

Ver la tabla completa de motivos en [Ingresar licencias](#sec-02-ingresar-licencias).

## "Eliminé una licencia por error"

La eliminación desde [Buscar licencia](#sec-03-buscar-licencia) es permanente e irreversible: borra tanto el registro como el documento archivado. No hay una papelera ni una forma de deshacerla desde el sistema. Si se cuenta con un respaldo reciente (ver [Respaldo](#sec-08-respaldo)), la persona encargada del soporte técnico puede evaluar una recuperación manual desde esa copia; si no, la licencia debe volver a ingresarse (el folio queda disponible de nuevo apenas se elimina).
