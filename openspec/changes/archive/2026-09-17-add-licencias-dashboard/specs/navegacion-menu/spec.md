## MODIFIED Requirements

### Requirement: Estructura de 2 capas del menú
El sistema SHALL organizar el menú lateral en un ítem de nivel superior "Inicio" y dos grupos colapsables — "Licencias" (Ingresar licencias, Buscar licencia, Redactar correo, Informe, Dashboard) y "Sistema" (Unidades, Respaldo, Configuración) — en lugar de una lista plana de páginas.

#### Scenario: Acceder a Inicio desde el menú
- **WHEN** la persona usuaria abre la aplicación
- **THEN** el menú lateral muestra "Inicio" como ítem de nivel superior, fuera de cualquier grupo, y al seleccionarlo navega a la página de inicio

#### Scenario: Páginas agrupadas por función
- **WHEN** la persona usuaria despliega el grupo "Licencias"
- **THEN** ve únicamente Ingresar licencias, Buscar licencia, Redactar correo, Informe y Dashboard
- **WHEN** la persona usuaria despliega el grupo "Sistema"
- **THEN** ve únicamente Unidades, Respaldo y Configuración
