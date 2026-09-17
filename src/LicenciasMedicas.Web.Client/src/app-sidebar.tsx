import { Building2, FileInput, HardDrive, Mail, Moon, Search, Settings, Sun } from "lucide-react";
import { useTheme } from "next-themes";
import { Link, useLocation } from "react-router";

import { Button } from "@/components/ui/button";
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
} from "@/components/ui/sidebar";

const paginas = [
  { href: "/unidades", etiqueta: "Unidades", icon: Building2 },
  { href: "/buscar", etiqueta: "Buscar licencia", icon: Search },
  { href: "/ingresar", etiqueta: "Ingresar licencias", icon: FileInput },
  { href: "/redactar-correo", etiqueta: "Redactar correo", icon: Mail },
  { href: "/respaldo", etiqueta: "Respaldo", icon: HardDrive },
  { href: "/configuracion", etiqueta: "Configuración", icon: Settings },
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
              {paginas.map((pagina) => (
                <SidebarMenuItem key={pagina.href}>
                  <SidebarMenuButton render={<Link to={pagina.href} />} isActive={pathname === pagina.href}>
                    <pagina.icon />
                    <span>{pagina.etiqueta}</span>
                  </SidebarMenuButton>
                </SidebarMenuItem>
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
