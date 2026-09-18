---
grupo: Licencias
capabilities_openspec:
  - redactar-correo
  - enviar-correo
  - correos-copia
  - licencia-correo-enviado
---

# Redactar correo

Genera el texto de aviso de licencias médicas para una unidad, y permite copiarlo o enviarlo directamente por correo electrónico sin salir del sistema.

## Buscar unidades con licencias pendientes de aviso

Se elige una **Fecha de grabado** (el día en que las licencias fueron grabadas en el sistema, no la fecha de la licencia) y se presiona **Buscar**. El sistema muestra qué unidades tienen, en esa fecha, licencias grabadas que todavía **no** están marcadas como "correo ya enviado" — con la cantidad de licencias de cada una.

![Tabla de unidades con licencias pendientes de aviso en la fecha buscada](01-unidades-en-fecha.png)

Si una unidad no aparece en la lista es porque no tiene licencias pendientes de aviso esa fecha: o no le llegó ninguna licencia ese día, o todas las que le llegaron ya están marcadas como correo enviado.

## Redactar el texto del aviso

Al presionar **Redactar correo** en la fila de una unidad, se genera un texto compuesto por un saludo y una tabla en columnas alineadas con las licencias pendientes de esa unidad y fecha (RUT, Nombre, Fecha Inicio, Fecha Término y Cantidad de Días). El texto queda en un cuadro editable — se puede corregir libremente antes de copiarlo o enviarlo.

![Texto de aviso generado para una unidad, editable, con los botones Copiar y Enviar correo](02-texto-generado.png)

- **Copiar** — copia el texto al portapapeles, para pegarlo en cualquier cliente de correo externo.
- **Enviar correo** — envía el texto directamente por correo electrónico a la dirección registrada de la unidad, usando el servidor SMTP configurado en [Configuración](#sec-09-configuracion). Antes de enviar, pide confirmación mostrando el destinatario y la cantidad de licencias incluidas; si ya se había enviado un aviso antes para esa misma unidad y fecha, también muestra cuándo fue ese envío anterior y permite reenviar igualmente.

::: importante
El botón **Enviar correo** queda deshabilitado si la unidad no tiene un correo electrónico con formato válido registrado. Hay que completarlo primero en [Unidades](#sec-07-unidades).
:::

Cualquier correo activo configurado en "Correos en copia (CC)" (ver [Configuración](#sec-09-configuracion)) recibe copia automáticamente de todos los avisos enviados, sin importar la unidad destinataria.
