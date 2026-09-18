import {
  Building2,
  ChevronRight,
  ClipboardList,
  FileInput,
  FileSpreadsheet,
  HardDrive,
  Home,
  LayoutDashboard,
  Mail,
  Moon,
  Search,
  Settings,
  Settings2,
  Sun,
} from "lucide-react";
import { useTheme } from "next-themes";
import { useState } from "react";
import { Link, useLocation } from "react-router";

import { Button } from "@/components/ui/button";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
} from "@/components/ui/sidebar";

const inicio = { href: "/", etiqueta: "Inicio", icon: Home };

const gruposNavegacion = [
  {
    id: "licencias",
    etiqueta: "Licencias",
    icon: ClipboardList,
    paginas: [
      { href: "/ingresar", etiqueta: "Ingresar licencias", icon: FileInput },
      { href: "/buscar", etiqueta: "Buscar licencia", icon: Search },
      { href: "/redactar-correo", etiqueta: "Redactar correo", icon: Mail },
      { href: "/informe", etiqueta: "Informe", icon: FileSpreadsheet },
      { href: "/dashboard", etiqueta: "Dashboard", icon: LayoutDashboard },
    ],
  },
  {
    id: "sistema",
    etiqueta: "Sistema",
    icon: Settings2,
    paginas: [
      { href: "/unidades", etiqueta: "Unidades", icon: Building2 },
      { href: "/respaldo", etiqueta: "Respaldo", icon: HardDrive },
      { href: "/configuracion", etiqueta: "Configuración", icon: Settings },
    ],
  },
];

function ThemeToggle() {
  const { resolvedTheme, setTheme } = useTheme();

  return (
    <Button
      variant="ghost"
      size="sm"
      className="w-full justify-start gap-2"
      onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
    >
      {resolvedTheme === "dark" ? <Sun className="size-4" /> : <Moon className="size-4" />}
      {resolvedTheme === "dark" ? "Modo claro" : "Modo oscuro"}
    </Button>
  );
}

export function AppSidebar() {
  const { pathname } = useLocation();

  // Estado inicial: colapsado, salvo el grupo que contiene la ruta activa. Solo se
  // recalcula al montar (recarga/apertura de la app), no en cada navegación, para que
  // los ajustes manuales del usuario se mantengan mientras navega sin recargar.
  const [gruposAbiertos, setGruposAbiertos] = useState<Record<string, boolean>>(() =>
    Object.fromEntries(
      gruposNavegacion.map((grupo) => [grupo.id, grupo.paginas.some((pagina) => pagina.href === pathname)]),
    ),
  );

  return (
    <Sidebar>
      <SidebarHeader className="flex flex-row items-center gap-2 px-4 py-3">
        <img src="/favicon.svg" alt="" className="size-5" />
        <span className="text-sm font-semibold">Licencias Médicas</span>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton render={<Link to={inicio.href} />} isActive={pathname === inicio.href}>
                  <inicio.icon />
                  <span>{inicio.etiqueta}</span>
                </SidebarMenuButton>
              </SidebarMenuItem>

              {gruposNavegacion.map((grupo) => (
                <Collapsible
                  key={grupo.id}
                  open={gruposAbiertos[grupo.id]}
                  onOpenChange={(open) => setGruposAbiertos((prev) => ({ ...prev, [grupo.id]: open }))}
                >
                  <SidebarMenuItem>
                    <CollapsibleTrigger render={<SidebarMenuButton className="group/collapsible" />}>
                      <grupo.icon />
                      <span>{grupo.etiqueta}</span>
                      <ChevronRight className="ml-auto transition-transform group-data-open/collapsible:rotate-90" />
                    </CollapsibleTrigger>
                    <CollapsibleContent>
                      <SidebarMenuSub>
                        {grupo.paginas.map((pagina) => (
                          <SidebarMenuSubItem key={pagina.href}>
                            <SidebarMenuSubButton render={<Link to={pagina.href} />} isActive={pathname === pagina.href}>
                              <pagina.icon />
                              <span>{pagina.etiqueta}</span>
                            </SidebarMenuSubButton>
                          </SidebarMenuSubItem>
                        ))}
                      </SidebarMenuSub>
                    </CollapsibleContent>
                  </SidebarMenuItem>
                </Collapsible>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter>
        <ThemeToggle />
      </SidebarFooter>
    </Sidebar>
  );
}
