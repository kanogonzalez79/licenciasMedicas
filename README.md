# Licencias Médicas

Sistema local (un solo usuario, Windows) para procesar, archivar y buscar licencias médicas: lee PDFs automáticamente (parser), permite ingreso manual con adjunto, y organiza los documentos archivados por año/mes.

- **Backend:** C# / .NET 8 (ASP.NET Core, Minimal APIs) + Dapper + SQLite.
- **Frontend:** React 19 + TypeScript + Vite + Tailwind CSS 4 + shadcn/ui.
- Corre 100% local, escuchando solo en `127.0.0.1` (loopback) — no requiere red ni servidor externo.

Ver [`CLAUDE.md`](./CLAUDE.md) para contexto completo del proyecto (módulos, glosario de dominio, reglas de desarrollo).

## Requisitos previos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) o superior.
- [Node.js](https://nodejs.org/) `24.14.1` (ver `src/LicenciasMedicas.Web.Client/.nvmrc`; si usas `nvm`, corre `nvm use` dentro de esa carpeta).
- Windows (la app usa WinForms para el diálogo nativo "Buscar carpeta" del respaldo y se publica como `.exe` self-contained `win-x64`).

## Instalación

Clona el repositorio e instala las dependencias del frontend:

```bash
git clone https://github.com/kanogonzalez79/licenciasMedicas.git
cd licenciasMedicas

# Dependencias del cliente React
cd src/LicenciasMedicas.Web.Client
npm install
cd ../..

# Restaurar paquetes .NET (Dapper, Microsoft.Data.Sqlite, PdfPig, etc.)
dotnet restore
```

No hay variables de entorno ni secretos que configurar: la base de datos SQLite y las carpetas de datos (`data/incoming`, `data/staging`, `data/archivo`, `logs/`) se crean automáticamente junto al ejecutable la primera vez que corre la app.

## Ejecutar en modo desarrollo

Se necesitan **dos procesos en paralelo** (backend + frontend con hot reload):

**1. Backend** (desde la raíz del repo):

```bash
dotnet run --project src/LicenciasMedicas.Web
```

Escucha en `http://127.0.0.1:5244` (puerto fijo solo en desarrollo).

**2. Frontend** (en otra terminal):

```bash
cd src/LicenciasMedicas.Web.Client
npm run dev
```

Levanta Vite en `http://localhost:5173` (o el siguiente puerto libre) con proxy de `/api/*` hacia el backend.

Abre `http://localhost:5173` en el navegador para usar la app.

## Pruebas

```bash
dotnet test tests/LicenciasMedicas.Core.Tests/LicenciasMedicas.Core.Tests.csproj
```

Lint del frontend:

```bash
cd src/LicenciasMedicas.Web.Client
npm run lint
```

## Compilar para distribución (.exe)

La forma más simple es correr el script ya preparado desde la raíz del repo:

```bat
compilar-exe.bat
```

Esto compila el backend, embebe el build de producción del cliente React en `wwwroot`, y deja `LicenciasMedicas.Web.exe` (self-contained, sin dependencias externas) en `publish/`, además de un `.zip` listo para distribuir.

Equivalente manual:

```bash
dotnet publish src/LicenciasMedicas.Web/LicenciasMedicas.Web.csproj -c Release -o publish
```

## Estructura del repositorio

```
src/
  LicenciasMedicas.Core/        Lógica de dominio (parsing de PDF, licencias, unidades, respaldo, etc.)
  LicenciasMedicas.Web/         Backend ASP.NET Core (Minimal APIs) + host de la SPA
  LicenciasMedicas.Web.Client/  Frontend React + Vite
tests/
  LicenciasMedicas.Core.Tests/  Tests unitarios/integración (xUnit)
openspec/                       Historial y specs de cada capability (ver /opsx:explore)
```

Más detalle de cada módulo en [`CLAUDE.md`](./CLAUDE.md).
