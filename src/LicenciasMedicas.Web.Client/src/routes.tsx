import { createBrowserRouter } from "react-router";

import App from "@/App";
import { BuscarPage } from "@/pages/BuscarPage";
import { IngresarPage } from "@/pages/IngresarPage";
import { InicioPage } from "@/pages/InicioPage";
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
      { path: "respaldo", Component: RespaldoPage },
    ],
  },
]);
