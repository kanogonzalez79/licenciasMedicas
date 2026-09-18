# Navegacion Menu Specification

## Purpose

Define la estructura de 2 capas del menú lateral de navegación (un ítem de inicio de nivel superior y grupos colapsables de páginas relacionadas) y cómo se comporta su expansión/colapso a medida que la persona usuaria navega por la aplicación.

## Requirements

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

### Requirement: Estado inicial de los grupos según la ruta activa
El sistema SHALL mostrar ambos grupos del menú colapsados al abrir o recargar la aplicación, excepto que el grupo que contiene la página actualmente activa SHALL mostrarse expandido automáticamente.

#### Scenario: Abrir la aplicación en la página de inicio
- **WHEN** la persona usuaria abre la aplicación y la ruta activa es "Inicio"
- **THEN** los grupos "Licencias" y "Sistema" aparecen ambos colapsados

#### Scenario: Recargar estando en una página de un grupo
- **WHEN** la persona usuaria recarga la aplicación estando en la página "Respaldo"
- **THEN** el grupo "Sistema" aparece expandido automáticamente y el grupo "Licencias" aparece colapsado

#### Scenario: Navegar directo a una URL de un grupo colapsado
- **WHEN** la persona usuaria escribe en el navegador la URL de una página del grupo "Licencias" (por ejemplo, la de Redactar correo) sin haber interactuado antes con el menú
- **THEN** al cargar la aplicación el grupo "Licencias" aparece expandido y muestra esa página como activa

### Requirement: Persistencia del estado de expansión solo durante la sesión de navegación
El sistema SHALL mantener el estado de expandido/colapsado que la persona usuaria ajuste manualmente mientras navega entre páginas sin recargar la aplicación, y SHALL descartar ese estado al recargar o reabrir la aplicación.

#### Scenario: Colapsar un grupo y navegar a otra página del mismo grupo
- **WHEN** la persona usuaria expande manualmente el grupo "Sistema", navega a "Unidades" y luego a "Configuración" sin recargar la página
- **THEN** el grupo "Sistema" permanece expandido durante toda esa navegación

#### Scenario: Recargar después de haber ajustado manualmente los grupos
- **WHEN** la persona usuaria expandió manualmente el grupo "Sistema" estando en "Inicio", y luego recarga la aplicación
- **THEN** el grupo "Sistema" vuelve a mostrarse colapsado, ya que la ruta activa ("Inicio") no pertenece a ningún grupo
