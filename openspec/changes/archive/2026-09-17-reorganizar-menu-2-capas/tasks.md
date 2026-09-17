## 1. Componente `ui/collapsible.tsx`

- [x] 1.1 Crear `src/LicenciasMedicas.Web.Client/src/components/ui/collapsible.tsx` envolviendo `@base-ui/react/collapsible` (`Root`, `Trigger`, `Panel`) con el mismo patrón de `data-slot` + clases Tailwind usado en `ui/sidebar.tsx`, exportando `Collapsible`, `CollapsibleTrigger` y `CollapsibleContent`; verificar que compila con `npm run build` sin errores de tipos.
- [x] 1.2 Agregar la transición de altura/animación de apertura-cierre del panel (usando las CSS vars/data-attributes que expone `Collapsible.Panel`) y verificar visualmente en el navegador que abrir/cerrar no salta abruptamente.

## 2. Datos de navegación con grupos

- [x] 2.1 En `app-sidebar.tsx`, reemplazar el array plano `paginas` por un ítem top-level "Inicio" (`href: "/"`) y dos grupos (`licencias`, `sistema`) con sus ítems según proposal.md - What Changes; verificar que `npm run lint` (oxlint) pasa sin errores.
- [x] 2.2 Agregar el ícono de "Inicio" (ej. `Home` de lucide-react) y ajustar los imports de íconos existentes según corresponda.

## 3. Renderizado del menú de 2 capas

- [x] 3.1 Renderizar "Inicio" como `SidebarMenuItem` suelto antes de los grupos, igual que los ítems actuales.
- [x] 3.2 Renderizar cada grupo como un `Collapsible` con `CollapsibleTrigger` (ícono del grupo + etiqueta + chevron) y `CollapsibleContent` conteniendo un `SidebarMenu`/`SidebarMenuSub` con los ítems del grupo, usando `SidebarMenuSubButton` + `Link` para cada ítem; verificar manualmente en el navegador que ambos grupos aparecen y despliegan sus ítems correctos.
- [x] 3.3 Mantener el resaltado de ítem activo (`isActive={pathname === pagina.href}`) en los ítems dentro de los grupos; verificar navegando a cada página que su ítem correspondiente queda resaltado.

## 4. Estado de expansión inicial y manual

- [x] 4.1 Calcular el estado inicial de cada grupo con un `useState` de inicialización perezosa que revise si `pathname` (de `useLocation`) cae dentro de los `href` del grupo; si no cae en ninguno (ej. "Inicio"), ambos grupos arrancan colapsados; verificar recargando la app en "/" y en una página de cada grupo (ej. `/respaldo`, `/informe`).
- [x] 4.2 Conectar `CollapsibleTrigger`/`Collapsible` de cada grupo al estado anterior vía `open`/`onOpenChange`, sin re-derivar el estado en un `useEffect` en cada cambio de ruta (ver design.md - Decisión 3); verificar que colapsar manualmente un grupo y navegar a otra página del mismo grupo sin recargar mantiene el grupo expandido.
- [x] 4.3 Verificar que recargar la aplicación después de haber tocado manualmente el estado de los grupos siempre vuelve a "colapsado salvo el grupo de la ruta activa" (sin usar `localStorage` ni ningún otro storage persistente).

## 5. Verificación final

- [x] 5.1 Probar manualmente el flujo completo en el navegador (`dotnet run` + `npm run dev`): abrir en cada una de las 8 rutas existentes y confirmar que el menú resalta el ítem correcto con el grupo (si aplica) expandido, y que el acordeón responde a clicks.
- [x] 5.2 Ejecutar `npm run lint` y `dotnet build` para confirmar que no se rompió nada fuera del frontend (el cambio no debería tocar el backend, pero se valida igual).
