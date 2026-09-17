CREATE TABLE ConfiguracionSmtp (
    Id                INTEGER PRIMARY KEY CHECK (Id = 1),
    Host              TEXT NOT NULL,
    Puerto            INTEGER NOT NULL,
    Usuario           TEXT NOT NULL,
    ContrasenaCifrada TEXT NOT NULL,
    Remitente         TEXT NOT NULL,
    UsaSsl            INTEGER NOT NULL
);

CREATE TABLE CorreosEnviados (
    CorreoEnviadoId INTEGER PRIMARY KEY AUTOINCREMENT,
    UnidadId        INTEGER NOT NULL REFERENCES Unidades(UnidadId),
    Fecha           TEXT NOT NULL,
    FechaHoraEnvio  TEXT NOT NULL
);
CREATE UNIQUE INDEX UX_CorreosEnviados_UnidadId_Fecha ON CorreosEnviados(UnidadId, Fecha);
