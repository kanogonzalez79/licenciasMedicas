import { createBrowserRouter } from "react-router";

import App from "@/App";
import { BuscarPage } from "@/pages/BuscarPage";
import { ConfiguracionPage } from "@/pages/ConfiguracionPage";
import { DashboardPage } from "@/pages/DashboardPage";
import { IngresarPage } from "@/pages/IngresarPage";
import { InicioPage } from "@/pages/InicioPage";
import { InformePage } from "@/pages/InformePage";
import { RedactarCorreoPage } from "@/pages/RedactarCorreoPage";
import { RespaldoPage } from "@/pages/RespaldoPage";
import { UnidadesPage } from "@/pages/UnidadesPage";

export const router = createBrowserRouter([
  {
    path: "/",
    Component: App,
    children: [
      { index: true, Component: InicioPage },
      { path: "unidades", Component: UnidadesPage },
      { path: "buscar", Component: BuscarPage },
      { path: "ingresar", Component: IngresarPage },
      { path: "redactar-correo", Component: RedactarCorreoPage },
      { path: "informe", Component: InformePage },
      { path: "dashboard", Component: DashboardPage },
      { path: "respaldo", Component: RespaldoPage },
      { path: "configuracion", Component: ConfiguracionPage },
    ],
  },
]);
