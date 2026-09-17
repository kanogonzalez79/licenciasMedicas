CREATE TABLE CorreoCopia (
    CorreoCopiaId     INTEGER PRIMARY KEY AUTOINCREMENT,
    CorreoElectronico TEXT NOT NULL,
    Activo            INTEGER NOT NULL DEFAULT 1
);
CREATE UNIQUE INDEX UX_CorreoCopia_CorreoElectronico ON CorreoCopia(CorreoElectronico COLLATE NOCASE);
