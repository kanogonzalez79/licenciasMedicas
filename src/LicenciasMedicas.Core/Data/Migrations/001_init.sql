PRAGMA foreign_keys = ON;

CREATE TABLE Unidades (
    UnidadId          INTEGER PRIMARY KEY AUTOINCREMENT,
    Descripcion       TEXT NOT NULL,
    CorreoElectronico TEXT NULL,
    FechaCreacion     TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    FechaModificacion TEXT NULL
);
CREATE UNIQUE INDEX UX_Unidades_Descripcion ON Unidades(Descripcion COLLATE NOCASE);

CREATE TABLE Licencias (
    LicenciaId                  INTEGER PRIMARY KEY AUTOINCREMENT,
    Folio                       TEXT NOT NULL,
    TipoFormulario              INTEGER NOT NULL,
    RutPacienteSinDv            TEXT NOT NULL,
    DvPaciente                  TEXT NOT NULL,
    ApellidoPaternoPaciente     TEXT NULL,
    ApellidoMaternoPaciente     TEXT NULL,
    NombresPaciente             TEXT NULL,
    NombreCompletoPaciente      TEXT NOT NULL,
    EdadPaciente                INTEGER NULL,
    SexoPaciente                TEXT NULL,
    CodigoTipoLicencia          INTEGER NULL,
    DescripcionTipoLicencia     TEXT NULL,
    FechaEmisionOtorgamiento    TEXT NULL,
    FechaInicioReposo           TEXT NOT NULL,
    FechaTerminoReposo          TEXT NOT NULL,
    CantidadDias                INTEGER NOT NULL,
    RutProfesionalSinDv         TEXT NULL,
    DvProfesional               TEXT NULL,
    NombreCompletoProfesional   TEXT NULL,
    CorreoProfesional           TEXT NULL,
    EspecialidadProfesional     TEXT NULL,
    UnidadId                    INTEGER NOT NULL REFERENCES Unidades(UnidadId),
    RutaPdfArchivado            TEXT NOT NULL,
    NombreArchivoOriginal       TEXT NOT NULL,
    HashArchivoSha256           TEXT NOT NULL,
    FechaIngresoSistema         TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    Observaciones                TEXT NULL
);
CREATE UNIQUE INDEX UX_Licencias_Folio        ON Licencias(Folio);
CREATE INDEX        IX_Licencias_RutPaciente  ON Licencias(RutPacienteSinDv);
CREATE INDEX        IX_Licencias_FechaInicio  ON Licencias(FechaInicioReposo);
CREATE INDEX        IX_Licencias_UnidadId     ON Licencias(UnidadId);
CREATE INDEX        IX_Licencias_NombreCompleto ON Licencias(NombreCompletoPaciente COLLATE NOCASE);
CREATE INDEX        IX_Licencias_FechaIngreso ON Licencias(FechaIngresoSistema);

CREATE TABLE LoteRevision (
    LoteId        TEXT PRIMARY KEY,
    FechaCreacion TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
);

CREATE TABLE LicenciaRevision (
    RevisionId                 INTEGER PRIMARY KEY AUTOINCREMENT,
    LoteId                     TEXT NOT NULL REFERENCES LoteRevision(LoteId),
    NombreArchivoOriginal      TEXT NOT NULL,
    RutaStaging                TEXT NOT NULL,
    HashArchivoSha256          TEXT NOT NULL,
    Folio                      TEXT NOT NULL,
    TipoFormulario             INTEGER NOT NULL,
    RutPacienteSinDv           TEXT NOT NULL,
    DvPaciente                 TEXT NOT NULL,
    ApellidoPaternoPaciente    TEXT NULL,
    ApellidoMaternoPaciente    TEXT NULL,
    NombresPaciente            TEXT NULL,
    NombreCompletoPaciente     TEXT NOT NULL,
    EdadPaciente               INTEGER NULL,
    SexoPaciente               TEXT NULL,
    CodigoTipoLicencia         INTEGER NULL,
    DescripcionTipoLicencia    TEXT NULL,
    FechaEmisionOtorgamiento   TEXT NULL,
    FechaInicioReposo          TEXT NOT NULL,
    FechaTerminoReposo         TEXT NOT NULL,
    CantidadDias               INTEGER NOT NULL,
    RutProfesionalSinDv        TEXT NULL,
    DvProfesional              TEXT NULL,
    NombreCompletoProfesional  TEXT NULL,
    CorreoProfesional          TEXT NULL,
    EspecialidadProfesional    TEXT NULL,
    UnidadId                   INTEGER NULL REFERENCES Unidades(UnidadId),
    Observaciones              TEXT NULL,
    FechaCreacion              TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
);
CREATE INDEX        IX_LicenciaRevision_LoteId ON LicenciaRevision(LoteId);
CREATE UNIQUE INDEX UX_LicenciaRevision_Hash   ON LicenciaRevision(HashArchivoSha256);
