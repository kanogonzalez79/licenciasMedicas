## Why

En la ficha de detalle de una licencia, los campos "Tipo de formulario" y "Sexo" muestran únicamente su código crudo (`1`/`2`/`3` y `M`/`F`), obligando a quien revisa la ficha a conocer de memoria qué significa cada código. El resto de campos codificados de la ficha (tipo de licencia) ya muestra su descripción junto al código; estos dos quedaron sin ese mismo tratamiento.

## What Changes

- La ficha de detalle muestra, junto al código de "Tipo de formulario", su descripción textual (`1` → "Formulario tipo 1", `2` → "Formulario tipo 2", `3` → "Manual").
- La ficha de detalle muestra, junto al código de "Sexo", su descripción textual (`M` → "Masculino", `F` → "Femenino").
- Si el valor almacenado no corresponde a ninguno de los códigos conocidos, la ficha muestra el valor tal cual viene, sin inventar una descripción.
- El mapeo de código a descripción se resuelve en el frontend, en el propio componente de la ficha; no se agregan columnas ni cambios de API o de base de datos.

## Capabilities

### Modified Capabilities
- `detalle-licencia`: la ficha de detalle ahora exige mostrar la descripción de los campos codificados "Tipo de formulario" y "Sexo", además de su código.

## Impact

- `src/LicenciasMedicas.Web.Client/src/components/licencia-detalle-modal.tsx`: agrega el mapeo código→descripción y ajusta el renderizado de "Tipo de formulario" y "Sexo".
- Sin cambios de API, backend ni base de datos.
