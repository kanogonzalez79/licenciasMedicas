## Why

El menú lateral (`app-sidebar.tsx`) hoy es una lista plana de 7 ítems (Unidades, Buscar licencia, Ingresar licencias, Redactar correo, Informe, Respaldo, Configuración) que mezcla trabajo operativo diario con administración del sistema, sin ninguna jerarquía visual. Además, la página "Inicio" (`/`) no aparece en el menú en absoluto — hoy es inalcanzable salvo escribiendo la URL a mano. A medida que se agregan más pantallas, una lista plana deja de escalar y de comunicar qué es "uso diario" versus "configuración".

## What Changes

- Reorganizar el menú en 2 capas: dos grupos colapsables (acordeón) dentro del mismo sidebar — "Licencias" (Ingresar licencias, Buscar licencia, Redactar correo, Informe) y "Sistema" (Unidades, Respaldo, Configuración).
- Agregar "Inicio" como ítem de nivel superior, fuera de ambos grupos, haciéndolo alcanzable desde el menú por primera vez.
- Cada grupo arranca colapsado al abrir o recargar la aplicación, excepto que se expande automáticamente si contiene la ruta activa al montar.
- El estado de expandido/colapsado que la persona usuaria toque manualmente se mantiene mientras navega entre páginas sin recargar (no se persiste entre recargas ni entre sesiones).
- Introducir un componente `ui/collapsible.tsx` (basado en `@base-ui/react`, patrón `render` como el resto del proyecto) para soportar el acordeón, siguiendo el estilo shadcn/ui ya usado en `ui/sidebar.tsx`.

## Capabilities

### New Capabilities
- `navegacion-menu`: estructura de 2 capas del menú lateral (ítem top-level "Inicio" + grupos colapsables "Licencias" y "Sistema"), comportamiento de expansión/colapso y su estado inicial según la ruta activa.

### Modified Capabilities
(ninguna — las capabilities existentes de cada página no cambian su comportamiento, solo cómo se accede a ellas desde el menú)

## Impact

- `src/LicenciasMedicas.Web.Client/src/app-sidebar.tsx`: reestructurar el array de navegación en grupos y renderizar con acordeón.
- `src/LicenciasMedicas.Web.Client/src/components/ui/collapsible.tsx`: nuevo componente (no existe hoy).
- No afecta backend, rutas de `routes.tsx`, ni el comportamiento de ninguna página individual.
