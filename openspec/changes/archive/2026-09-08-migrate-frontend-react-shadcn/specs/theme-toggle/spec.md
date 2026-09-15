## Purpose

Permite a la persona usuaria alternar la apariencia de la interfaz entre clara y oscura, y recordar esa elección entre sesiones de la aplicación.

## ADDED Requirements

### Requirement: Alternar entre apariencia clara y oscura
El sistema SHALL permitir a la persona usuaria cambiar entre apariencia clara y oscura mediante un control visible en la interfaz.

#### Scenario: Cambiar a modo oscuro
- **WHEN** la persona usuaria activa el control de apariencia estando en modo claro
- **THEN** toda la interfaz (sidebar, paneles, tablas, formularios) pasa a mostrarse en su variante oscura

#### Scenario: Volver a modo claro
- **WHEN** la persona usuaria activa el control de apariencia estando en modo oscuro
- **THEN** toda la interfaz vuelve a mostrarse en su variante clara

### Requirement: Persistencia de la preferencia de apariencia
El sistema SHALL recordar la última apariencia elegida y aplicarla automáticamente en sesiones futuras de la aplicación, sin requerir que la persona usuaria la vuelva a seleccionar.

#### Scenario: Reabrir la aplicación tras elegir modo oscuro
- **WHEN** la persona usuaria eligió modo oscuro y luego cierra y vuelve a abrir la aplicación
- **THEN** la interfaz se muestra en modo oscuro sin necesidad de volver a activarlo

### Requirement: Apariencia por defecto
El sistema SHALL usar la preferencia de apariencia del sistema operativo (claro u oscuro) como valor inicial cuando la persona usuaria no eligió una apariencia previamente.

#### Scenario: Primer uso sin preferencia guardada
- **WHEN** la persona usuaria abre la aplicación por primera vez y su sistema operativo está configurado en modo oscuro
- **THEN** la interfaz se muestra en modo oscuro sin que la persona usuaria haya interactuado con el control de apariencia
