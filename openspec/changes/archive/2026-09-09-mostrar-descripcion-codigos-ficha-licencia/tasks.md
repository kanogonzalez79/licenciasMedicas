## 1. Mapeo de descripciones en la ficha de detalle

- [x] 1.1 En `licencia-detalle-modal.tsx`, agregar un mapeo de `tipoFormulario` (1/2/3) a su descripción ("Formulario tipo 1" / "Formulario tipo 2" / "Manual") y usarlo para renderizar el campo "Tipo de formulario", dejando el código crudo como fallback si el valor no es 1, 2 ni 3.
- [x] 1.2 En `licencia-detalle-modal.tsx`, agregar un mapeo de `sexoPaciente` ("M"/"F") a su descripción ("Masculino" / "Femenino") y usarlo para renderizar el campo "Sexo", dejando el valor crudo como fallback si no es "M" ni "F".
- [x] 1.3 Verificar manualmente en la app (búsqueda → "Ver ficha") que una licencia procesada automáticamente muestra "Formulario tipo 1" o "Formulario tipo 2" y "Masculino"/"Femenino" según corresponda, y que una licencia de ingreso manual muestra "Manual" y el campo "Sexo" en estado "No informado".
