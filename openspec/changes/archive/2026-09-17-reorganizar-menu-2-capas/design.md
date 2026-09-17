## Context

`app-sidebar.tsx` renderiza hoy un array plano `paginas` dentro de un solo `SidebarMenu`. El layout (`App.tsx`) monta `<AppSidebar />` fuera del `<Outlet>`, por lo que el sidebar no se desmonta al navegar entre rutas — cualquier estado de React que viva ahí sobrevive a la navegación dentro de la SPA y solo se pierde con un reload completo. El proyecto usa `@base-ui/react` (no Radix) como base de las primitivas de `ui/`; `ui/sidebar.tsx` ya expone `SidebarMenuSub`, `SidebarMenuSubItem` y `SidebarMenuSubButton` para ítems anidados, pero no existe todavía un componente de acordeón/collapsible en `components/ui/`. `@base-ui/react` sí trae `collapsible/{root,trigger,panel}`, ver proposal.md - Why para la motivación del cambio y specs/navegacion-menu/spec.md para el comportamiento exigido.

## Goals / Non-Goals

**Goals:**
- Definir cómo se construye el componente `ui/collapsible.tsx` siguiendo el mismo patrón shadcn/ui + base-ui que `ui/sidebar.tsx` (uso de `render` en vez de `asChild`).
- Definir la forma de los datos de navegación (grupos + ítems) y cómo se deriva el estado inicial de expansión a partir de la ruta activa.
- Acotar el manejo de estado a memoria de componente (sin localStorage), coherente con specs/navegacion-menu/spec.md.

**Non-Goals:**
- No cambia el comportamiento de ninguna página individual ni las rutas de `routes.tsx`.
- No introduce persistencia entre sesiones del estado de expansión (descartado explícitamente).
- No convierte el logo del header en link ni toca `theme-toggle`.

## Decisions

### 1. Nuevo primitivo `ui/collapsible.tsx` sobre `@base-ui/react/collapsible`
Se agrega un wrapper delgado siguiendo el patrón ya usado en `ui/sidebar.tsx` (componentes de función que envuelven las partes de base-ui y les aplican clases Tailwind vía `data-slot`), exportando algo equivalente a:
- `Collapsible` (envuelve `Collapsible.Root`, props `open`/`onOpenChange`)
- `CollapsibleTrigger` (envuelve `Collapsible.Trigger`, usa el patrón `render` para componer con `SidebarMenuButton` cuando haga falta)
- `CollapsibleContent` (envuelve `Collapsible.Panel`, con la animación de alto que base-ui expone vía sus data-attributes/CSS vars)

Alternativa descartada: usar `<details>/<summary>` nativo. Se descarta porque no da control fino sobre el ícono chevron animado ni sobre la sincronización de estado con la ruta activa, y el proyecto ya estandarizó en primitivas base-ui para todo lo demás.

### 2. Modelo de datos de navegación: grupos con ítems, más un top-level
`app-sidebar.tsx` reemplaza el array plano `paginas` por una estructura de 2 niveles, por ejemplo:
```ts
const navegacionTopLevel = { href: "/", etiqueta: "Inicio", icon: Home };
const gruposNavegacion = [
  { id: "licencias", etiqueta: "Licencias", icon: FileInput, paginas: [ingresar, buscar, redactarCorreo, informe] },
  { id: "sistema", etiqueta: "Sistema", icon: Settings, paginas: [unidades, respaldo, configuracion] },
];
```
Esto mantiene el mismo shape de ítem (`href`, `etiqueta`, `icon`) que hoy, solo anidado bajo un grupo con `id` y `etiqueta` propios.

### 3. Estado de expansión: `useState` derivado de `pathname`, sin persistencia
Al montar `AppSidebar`, el estado inicial de qué grupo(s) están abiertos se calcula una sola vez (lazy `useState` initializer) buscando en qué grupo cae `pathname` actual; si ninguno matchea (p.ej. "Inicio"), ambos arrancan cerrados. Después de montado, el usuario puede abrir/cerrar grupos libremente vía el trigger del acordeón, y ese estado vive en el mismo `useState` mientras el componente permanezca montado — que es siempre, dado que `AppSidebar` no se desmonta al navegar (ver Context). No se usa `useEffect` para re-sincronizar en cada navegación: una vez montado, el acordeón se comporta como control manual puro, y solo la carga/recarga inicial deriva el estado de la ruta. Esto es exactamente lo que pide specs/navegacion-menu/spec.md (estado manual se mantiene durante la sesión, se descarta al recargar).

Alternativa descartada: un `useEffect` que reabra el grupo activo cada vez que cambia `pathname`. Se descarta porque forzaría el grupo activo a abrirse en cada click de navegación aunque el usuario lo haya colapsado a propósito, lo cual contradice el requisito de que el estado manual se respete durante la sesión.

Alternativa descartada: persistir en `localStorage`. Descartada explícitamente por el usuario — cada apertura de la app debe volver a "colapsado salvo el grupo activo".

### 4. Resaltado de ítem activo dentro de un grupo colapsado
`SidebarMenuButton`/`SidebarMenuSubButton` ya soportan `isActive` vía `pathname === href`; esto no cambia. Lo nuevo es que el `CollapsibleTrigger` del grupo también puede reflejar visualmente si contiene la ruta activa (útil si en el futuro se decide mostrar un indicador en el grupo colapsado), pero esto es opcional y no bloquea la implementación mínima.

## Risks / Trade-offs

- [Riesgo] Un usuario podría esperar que el grupo se reabra automáticamente al navegar a una página de ese grupo (ej. viene de "Inicio" y hace click en un ítem del grupo "Sistema" a través de una URL externa/bookmark) → Mitigación: no aplica en la práctica porque cualquier navegación dentro de la SPA pasa por el propio menú (el usuario ya tuvo que abrir el grupo para hacer click), y la navegación por URL directa sí se cubre por el cálculo de estado inicial al montar.
- [Riesgo] Duplicar lógica de "está activo" entre el trigger del grupo y los ítems hijos podría desincronizarse → Mitigación: derivar ambos del mismo array de datos (`pagina.href === pathname`) en un único lugar, sin estado duplicado.

## Open Questions

(ninguna — todas las decisiones de comportamiento fueron confirmadas con el usuario antes de este proposal)
