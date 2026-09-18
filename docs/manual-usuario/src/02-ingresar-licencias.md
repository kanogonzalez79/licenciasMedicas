---
grupo: Licencias
capabilities_openspec:
  - archivo-documento-licencia
  - ingresar-licencia-manual
  - licencia-correo-enviado
---

# Ingresar licencias

Esta es la pantalla principal para incorporar licencias nuevas al sistema. Combina dos caminos de ingreso:

- **Lectura automática de PDF**: para los documentos que llegan como PDF en un formato que el sistema sabe reconocer.
- **Ingreso manual**: para escribir a mano los datos de una licencia, adjuntando su documento de respaldo (PDF o imagen), cuando el PDF no se puede leer automáticamente o cuando el documento no es un PDF.

![Pantalla de Ingresar licencias, con los botones Procesar, Grabar e Ingresar manual](01-vista-general.png)

## El flujo de la lectura automática: Procesar → revisar → Grabar

Ningún PDF queda archivado de forma permanente en el sistema de un solo paso. Siempre pasa por tres etapas:

1. **Incoming** — Los PDF nuevos se depositan en una carpeta de entrada del sistema. Mientras están ahí, todavía no son licencias del sistema: solo son archivos esperando ser leídos.
2. **Revisión (staging)** — Al presionar **Procesar**, el sistema lee automáticamente cada PDF de la carpeta de entrada y arma una lista de licencias "leídas, pendientes de grabar". El PDF se mueve a una carpeta de trabajo intermedia mientras se revisa.
3. **Archivo definitivo** — Al presionar **Grabar**, cada licencia de la lista queda guardada de forma permanente en el sistema y su documento queda archivado en la carpeta de archivo (organizada por año y mes).

::: importante
Mientras una licencia está en la etapa 2 (revisión, sin grabar), todavía se puede corregir, descartar o dejar pendiente. Una vez que se presiona **Grabar**, la licencia queda archivada de forma permanente y solo se puede eliminar desde [Buscar licencia](#sec-03-buscar-licencia).
:::

::: nota
Si el sistema se cierra o se reinicia mientras hay licencias leídas y todavía sin grabar, esas licencias **no se conservan**: al volver a abrir el sistema, sus PDF vuelven automáticamente a la carpeta de entrada y hay que presionar **Procesar** de nuevo. Por eso conviene grabar (o descartar) un lote antes de cerrar el sistema.
:::

### Botón Procesar

Lee todos los PDF nuevos de la carpeta de entrada y los clasifica en dos grupos, que aparecen como dos tablas separadas:

**Licencias leídas, pendientes de grabar** — con columnas RUT sin DV, DV, Nombre completo, Tipo de licencia, Fecha inicio, Fecha término, Días, "¿Se leyó bien?", Unidad y Correo enviado. Antes de grabar, hay que revisar y completar dos cosas por cada fila:

- **Unidad**: es obligatorio elegirla de la lista desplegable para poder grabar esa licencia.
- **Correo enviado**: casilla opcional, para marcar que el aviso de esa licencia ya se envió por fuera del sistema (por ejemplo, una licencia atrasada que ya se avisó a la unidad antes de digitalizarla). El botón **Marcar lote como correo enviado** marca esta casilla en todas las filas del lote de una sola vez; después se puede desmarcar alguna fila puntual sin afectar a las demás.

![Grilla de licencias leídas, pendientes de grabar, con la columna Unidad para asignar cada licencia](02-grilla-pendientes.png)

**PDF que no se pudieron procesar (siguen en incoming)** — solo aparece si algún archivo falló al leerse. Muestra el nombre del archivo y el motivo, y ese PDF queda en la carpeta de entrada sin bloquear al resto del lote, para poder corregirlo o tratarlo manualmente. Los motivos posibles son:

| Motivo mostrado | Qué significa |
|---|---|
| No se reconoció el formato del PDF | El documento no corresponde a ninguna plantilla que el sistema sepa leer |
| El RUT del paciente no es válido | El RUT leído del PDF no tiene un dígito verificador correcto |
| No se encontraron los datos del trabajador | Falta la sección con los datos de la persona en el PDF |
| Las fechas de reposo no son válidas | Las fechas leídas son inconsistentes entre sí |
| La cantidad de días no es válida | El número de días leído no calza con las fechas |
| El código de tipo de licencia no es válido | El código leído no corresponde a ninguno de los 7 tipos reconocidos |
| No se encontró el folio del documento | El PDF no trae un número de folio legible |
| El PDF no trae la página de Comprobante | Falta una página que el formulario tipo 2 requiere |
| No se pudo leer el archivo PDF | El archivo está dañado o no se pudo abrir |
| Este PDF ya fue grabado anteriormente | El mismo archivo (por su contenido) ya había sido procesado antes |
| Ya existe una licencia grabada con este folio | El folio leído coincide con una licencia que ya está en el sistema o en revisión |

![Tabla de PDF que no se pudieron procesar, con el motivo del error](03-pdf-no-procesados.png)

### Botón Grabar

Aparece solo cuando hay licencias pendientes de grabar. Al presionarlo, cada fila de la lista se archiva definitivamente: el documento pasa a la carpeta de archivo (por año y mes) y se crea el registro de la licencia. Si todas las filas se graban bien, se muestra un mensaje de éxito con la cantidad grabada. Si alguna fila no se pudo grabar (por ejemplo, porque otra persona grabó el mismo folio segundos antes), el sistema informa cuáles filas quedaron sin grabar y por qué, sin afectar a las demás — esas filas quedan disponibles para revisarlas de nuevo.

## Ingreso manual de una licencia

El botón **Ingresar manual** abre un formulario para tipear los datos de una licencia y adjuntar su documento de respaldo, sin pasar por la carpeta de entrada ni por la revisión de arriba. Se usa para licencias que llegan en papel, en un formato de PDF que el sistema no reconoce, o que se quieren regularizar directamente.

![Formulario de ingreso manual de licencia, con sus campos y el selector de archivo adjunto](04-modal-ingreso-manual.png)

**Campos obligatorios**: Folio, RUT paciente, Nombre completo paciente, Tipo de licencia, fechas de reposo (o fecha de inicio + cantidad de días), Unidad, y el documento de respaldo adjunto (PDF, JPG o PNG, máximo 20 MB).

**Campos opcionales**: datos del profesional que emitió la licencia (RUT, nombre, correo, especialidad) y Observaciones.

**Tipo de licencia** — son los 7 códigos médicos definidos por la normativa:

1. Enfermedad o Accidente Común
2. Prórroga Medicina Preventiva
3. Licencia Maternal Pre y Post Natal
4. Enfermedad Grave Hijo Menor de 1 Año
5. Accidente del Trabajo o del Trayecto
6. Enfermedad Profesional
7. Patología del Embarazo

**Cálculo automático de fechas y días**: basta con completar dos de estos tres datos y el sistema calcula el tercero automáticamente:

- Fecha de inicio + Fecha de término → calcula la cantidad de días (contando ambos extremos).
- Fecha de inicio + Cantidad de días → calcula la fecha de término.

::: nota
El período de reposo no puede superar los **365 días**. Si se intenta grabar un rango mayor, el sistema lo rechaza indicando que supera el máximo permitido.
:::

Al igual que en la lectura automática, el sistema rechaza el ingreso si el folio ya existe, o si el documento adjunto (por su contenido) ya fue usado en otra licencia grabada o pendiente de revisión. También se puede marcar la casilla **"Correo ya enviado"**, para licencias atrasadas cuyo aviso ya se mandó antes de digitalizarlas — el comportamiento es idéntico al de la casilla de correo enviado del flujo automático.

Al guardar exitosamente, la licencia queda disponible de inmediato en [Buscar licencia](#sec-03-buscar-licencia), marcada con origen "Manual".
