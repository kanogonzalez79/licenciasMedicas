# Introducción

## Qué es este sistema

El Sistema de Licencias Médicas es una herramienta interna para digitalizar, revisar, buscar y archivar las licencias médicas del personal, y para avisar por correo electrónico a cada unidad organizacional cuando le llega una licencia nueva.

Es una aplicación de escritorio de un solo usuario: se ejecuta en un computador de la organización y se usa desde un navegador web apuntando a esa misma máquina. No requiere instalación de una base de datos aparte ni conexión permanente a internet — solo se necesita conexión saliente para enviar los correos de aviso.

## Cómo abrir el sistema

El sistema se abre ejecutando su programa (`.exe`) en el computador donde está instalado. Al iniciar, abre automáticamente una ventana del navegador con la aplicación. Si se cierra esa ventana por error, se puede volver a abrir escribiendo la misma dirección en un navegador (generalmente algo como `http://127.0.0.1:<puerto>`).

::: importante
El sistema está pensado para un solo computador y un solo uso a la vez. No está diseñado para exponerse en una red compartida ni para que varias personas lo usen simultáneamente desde distintos equipos.
:::

## Conceptos básicos que conviene conocer antes de empezar

Estos términos se usan a lo largo de todo el manual:

- **Licencia médica**: el documento que certifica el reposo médico de una persona. Se puede ingresar al sistema de dos formas: leyendo automáticamente un PDF, o escribiendo sus datos a mano (ingreso manual).
- **Folio**: el número que identifica de forma única a cada licencia en todo el sistema. Nunca puede haber dos licencias grabadas con el mismo folio.
- **Unidad**: el área o departamento de la organización al que pertenece la persona con la licencia. Cada licencia se asigna a una unidad, y es a esa unidad a la que se le avisa por correo.
- **Tipo de licencia**: el motivo médico de la licencia (por ejemplo, enfermedad común, licencia maternal, accidente del trabajo). Es un código de 1 a 7 definido por la normativa vigente — se detalla en el [Glosario](#sec-10-glosario).
- **Tipo de formulario**: indica *cómo* se ingresó la licencia al sistema, no el motivo médico: formulario tipo 1 o tipo 2 (leídos automáticamente desde un PDF) o "Manual" (tipeada a mano). No debe confundirse con el tipo de licencia.
- **Incoming, staging y archivo**: las tres etapas por las que pasa un PDF de licencia antes de quedar grabado de forma permanente. Se explican en detalle en [Ingresar licencias](#sec-02-ingresar-licencias).

## Cómo está organizado este manual

El manual sigue el mismo orden que el menú lateral de la aplicación: primero **Inicio**, luego el grupo **Licencias** (todo lo relacionado con ingresar, buscar y avisar licencias) y finalmente el grupo **Sistema** (configuración y mantenimiento del sistema mismo). Al final hay un glosario de términos y un anexo de soporte para cuando algo no sale como se espera.
