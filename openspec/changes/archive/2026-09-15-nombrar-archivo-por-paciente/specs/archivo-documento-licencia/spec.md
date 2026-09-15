## Purpose

Define cómo se nombra el documento (PDF o imagen) de una licencia al archivarlo de forma definitiva en la carpeta por año/mes, para que sea identificable por una persona que navega la carpeta sin necesidad de abrir el sistema.

## ADDED Requirements

### Requirement: Nombre de archivo con nombre del paciente y folio
El sistema SHALL nombrar el documento archivado definitivamente de una licencia (PDF o imagen, tanto del flujo automático como del ingreso manual) como `{Nombre completo del paciente en Título Case} - {Folio}{extensión}`, dentro de la carpeta por año/mes correspondiente.

#### Scenario: Documento procesado automáticamente
- **WHEN** se graba una licencia cuyo documento fue leído por el parser automático, con paciente "JUAN CARLOS PEREZ SOTO" y folio "LM-00123"
- **THEN** el documento queda archivado como "Juan Carlos Perez Soto - LM-00123.pdf" dentro de la carpeta del año/mes correspondiente

#### Scenario: Documento ingresado manualmente
- **WHEN** se graba una licencia ingresada manualmente junto con su documento de respaldo, con paciente "María José Núñez" y folio "999-2026"
- **THEN** el documento queda archivado como "María José Núñez - 999-2026" seguido de la extensión del documento adjunto, dentro de la carpeta del año/mes correspondiente

### Requirement: Normalización del nombre a Título Case
El sistema SHALL normalizar el nombre del paciente a Título Case al construir el nombre de archivo (primera letra de cada palabra en mayúscula, resto en minúscula), preservando tildes y la letra "ñ", sin importar si el nombre original venía en mayúsculas, minúsculas o una mezcla de ambas.

#### Scenario: Nombre extraído en mayúsculas
- **WHEN** el nombre completo del paciente es "JOSÉ ANTONIO NÚÑEZ"
- **THEN** el nombre de archivo usa "José Antonio Núñez"

#### Scenario: Nombre con mezcla de mayúsculas y minúsculas
- **WHEN** el nombre completo del paciente es "pEdRo GONZALEZ"
- **THEN** el nombre de archivo usa "Pedro Gonzalez"

### Requirement: Unicidad de nombre de archivo apoyada en el folio
El sistema SHALL garantizar que dos documentos de la misma persona archivados en el mismo año/mes nunca produzcan el mismo nombre de archivo, usando el folio (que ya es único en todo el sistema) como parte del nombre, sin requerir un correlativo adicional.

#### Scenario: Dos documentos de la misma persona en el mismo mes
- **WHEN** se archivan dos licencias distintas de la misma persona (mismo nombre, folios distintos) dentro del mismo año/mes
- **THEN** cada documento queda archivado con su propio folio en el nombre, sin colisión de nombres de archivo

### Requirement: Caracteres inválidos en el nombre del paciente
El sistema SHALL reemplazar cualquier carácter del nombre del paciente que no sea válido para nombres de archivo del sistema operativo, de forma que el archivo siempre pueda crearse en disco.

#### Scenario: Nombre con caracteres no válidos para nombre de archivo
- **WHEN** el nombre completo del paciente contiene un carácter no permitido en nombres de archivo (por ejemplo "/" o ":")
- **THEN** ese carácter se reemplaza y el documento se archiva sin error
