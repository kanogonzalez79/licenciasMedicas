# Cómo (re)tomar las capturas de pantalla del manual

Todas las capturas se toman en **tema oscuro**, sobre el entorno de desarrollo normal, con **datos ficticios** (nunca datos reales de pacientes, según la regla de `CLAUDE.md`). Al terminar, se eliminan los datos sembrados.

## 1. Levantar el entorno

```bash
# Terminal 1 — backend
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/LicenciasMedicas.Web

# Terminal 2 — frontend
cd src/LicenciasMedicas.Web.Client
npm run dev
```

Abrir `http://localhost:5173` (o el puerto que informe Vite si el 5173 está ocupado) y activar el **modo oscuro** con el interruptor del pie del menú lateral.

## 2. Sembrar datos ficticios

No existe (ni conviene crear) un script permanente para esto: se arma una vez, se usa y se descarta. La forma más simple es un script Node de un solo uso que llame directamente a la API (evita además el problema de mojibake de Git Bash con tildes/ñ como argumento de `curl`, documentado en `CLAUDE.md`).

**Unidades** (5, vía `POST /api/unidades`): usar nombres de áreas genéricas y un dominio de correo claramente ficticio, por ejemplo `@empresa-ficticia.cl`. Las usadas la última vez:

| Descripción | Correo |
|---|---|
| Recursos Humanos | rrhh@empresa-ficticia.cl |
| Contabilidad y Finanzas | contabilidad@empresa-ficticia.cl |
| Bodega y Logística | bodega@empresa-ficticia.cl |
| Tecnología y Sistemas | sistemas@empresa-ficticia.cl |
| Atención al Cliente | atencioncliente@empresa-ficticia.cl |

**Licencias** (10-12, vía `POST /api/licencias/manual`, formulario multipart): nombres y RUT inventados, con dígito verificador válido (algoritmo módulo 11 — se puede calcular con un one-liner de Node, ver historial de esta sesión o cualquier calculadora de RUT). Repartir las fechas en 3-4 meses hacia atrás desde la fecha de captura, variar el tipo de licencia (usar al menos 4 de los 7 códigos) y la unidad asignada, para que el Dashboard y el ranking por unidad/tipo se vean variados. Adjuntar como documento de respaldo un archivo pequeño (PNG/PDF) **distinto en cada licencia** — el sistema rechaza adjuntos duplicados por hash, así que reutilizar el mismo archivo para todas falla a partir de la segunda.

Marcar `correoEnviado: true` en 1-2 licencias (para que se vea la marca "Correo enviado" en Buscar licencia) y dejar el resto pendiente (para que aparezcan varias unidades con contador en Redactar correo).

## 3. Una licencia de origen "Automático" (opcional, para Ingresar licencias)

Los PDF de ejemplo del repo (`_ejemplos/`, `tests/LicenciasMedicas.Core.Tests/Fixtures/`) **contienen datos que parecen reales** (nombre, RUT, correo personal y dirección de una persona) — no están confirmados como ficticios, así que **no se deben usar** para las capturas del manual.

En su lugar, se genera un PDF sintético con datos 100% inventados que el parser de "Tipo 1" (`src/LicenciasMedicas.Core/Parsing/Tipo1Extractor.cs`) pueda leer igual: es una extracción basada en texto plano (PdfPig), así que basta con un HTML simple impreso a PDF (por ejemplo con Puppeteer) que contenga, en este orden, los anclas y patrones que el extractor busca:

```
SECCION 0:
N3 Folio: 55123478-9

A.1 IDENTIFICACION DEL TRABAJADOR
GONZALEZ PEREZ MARIA FERNANDA 76543210-3 29 F
FECHA EMISION LICENCIA 15 Dia 08 Mes 26 Ano
FECHA INICIO DE REPOSO 15 Dia 08 Mes 26 Ano
N DE DIAS 5 N DE DIAS EN PALABRAS CINCO

A.3 TIPO DE LICENCIA
1
1=Enfermedad o Accidente Comun

A.4 CARACTERISTICAS DEL REPOSO
Reposo total en domicilio.

A.5 IDENTIFICACION DEL PROFESIONAL
SOTO RIVAS CARLOS ALBERTO 15987654-3
carlos.soto@ejemplo-clinico.cl

A.6 DIAGNOSTICO
Informacion confidencial.
```

(RUT con dígito verificador válido, igual que las licencias manuales.) Ese PDF se deja en la carpeta `incoming` del backend en ejecución (`.../bin/Debug/net8.0-windows/data/incoming/`) y se procesa normalmente desde la pantalla "Ingresar licencias". Para la captura de "PDF que no se pudieron procesar", basta con dejar además cualquier archivo `.pdf` corrupto (por ejemplo un `.txt` renombrado) en esa misma carpeta.

## 4. Tomar las capturas

Ventana del navegador a un ancho generoso (se usó 1900px; el ancho real capturado depende de la herramienta de captura usada). Para cada pantalla, navegar a su ruta y capturar según la lista de abajo — el nombre de archivo debe coincidir exactamente con el que referencia el `.md` de esa sección.

| Sección | Ruta | Archivo(s) esperado(s) |
|---|---|---|
| Inicio | `/` | `imagenes/01-inicio/01-vista-general.png` |
| Ingresar licencias | `/ingresar` | `01-vista-general.png`, `02-grilla-pendientes.png`, `03-pdf-no-procesados.png`, `04-modal-ingreso-manual.png` (abrir "Ingresar manual" y completar campos de ejemplo sin adjuntar archivo, para no guardarla) |
| Buscar licencia | `/buscar` | `01-filtros-resultados.png`, `02-ficha-detalle.png` (abrir "Ver ficha" de cualquier fila) |
| Redactar correo | `/redactar-correo` | `01-unidades-en-fecha.png` (buscar con la fecha de hoy), `02-texto-generado.png` (presionar "Redactar correo" en una unidad con varias licencias) |
| Informe | `/informe` | `01-formulario.png` |
| Dashboard | `/dashboard` | `01-resumen-completo.png` (aplicar un rango que cubra todas las licencias sembradas) |
| Unidades | `/unidades` | `01-listado-y-alta.png` |
| Respaldo | `/respaldo` | `01-vista-general.png` |
| Configuración | `/configuracion` | `01-smtp.png`, `02-correos-copia.png` (agregar 1-2 correos de ejemplo antes de capturar) |

Preferir formato **PNG** (más nítido para texto de UI que JPEG) cuando la herramienta de captura lo permita.

## 5. Limpiar los datos sembrados

- **Licencias**: eliminarlas todas desde "Buscar licencia" (botón Eliminar), o vía `DELETE /api/licencias/{id}` — esto también borra su documento archivado.
- **Correos en copia**: eliminarlos desde "Configuración" (botón Eliminar), o vía `DELETE /api/configuracion/correos-copia/{id}`.
- **Unidades**: el sistema **no ofrece una forma de eliminar unidades** (decisión de producto, ver `openspec/specs/unidades/spec.md`). Las unidades ficticias creadas para las capturas quedan en la base de datos de desarrollo. Opciones: dejarlas (son inofensivas, quedan solo en la base local de desarrollo) o reutilizarlas la próxima vez que se recapture en vez de crear unas nuevas.
- **`incoming`/`staging`**: si quedó algún PDF de prueba sin procesar o a medio camino, basta con borrarlo directamente de la carpeta `data/incoming` o `data/staging` del build del backend — no son datos de la base, son solo archivos en disco.
- Confirmar que `SMTP` en Configuración no haya quedado con una configuración guardada de prueba (no hace falta si nunca se presionó "Guardar" en esa pantalla).
