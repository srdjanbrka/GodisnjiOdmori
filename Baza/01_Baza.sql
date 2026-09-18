-- Pokrenuti u SSMS. Nova baza; skripta namerno odbija prepisivanje postojece baze.
USE master;
GO
IF DB_ID(N'GodisnjiOdmori') IS NOT NULL
BEGIN
    PRINT N'Baza GodisnjiOdmori vec postoji. Inicijalizacija je preskocena; nista nije prepisano.';
    SET NOEXEC ON;
END;
GO
IF DB_ID(N'GodisnjiOdmori') IS NULL EXEC(N'CREATE DATABASE GodisnjiOdmori');
GO
USE GodisnjiOdmori;
GO
IF OBJECT_ID('dbo.Sektor') IS NOT NULL
    THROW 50001, N'Tabele vec postoje. Zaustavite izvrsavanje; ne pokrecite inicijalizaciju ponovo.', 1;
ELSE
BEGIN
CREATE TABLE dbo.Sektor (
    SektorID int IDENTITY PRIMARY KEY,
    Naziv nvarchar(100) NOT NULL UNIQUE
);
CREATE TABLE dbo.Zaposleni (
    ZaposleniID int IDENTITY PRIMARY KEY,
    ImePrezime nvarchar(100) NOT NULL,
    SektorID int NOT NULL REFERENCES dbo.Sektor(SektorID),
    RadnoMesto nvarchar(100) NOT NULL
);
CREATE TABLE dbo.Zahtev (
    ZahtevID int IDENTITY PRIMARY KEY,
    ZaposleniID int NOT NULL REFERENCES dbo.Zaposleni(ZaposleniID),
    DatumPodnosenja datetime2 NOT NULL DEFAULT SYSDATETIME(),
    DatumOd date NOT NULL,
    DatumDo date NOT NULL,
    Status nvarchar(20) NOT NULL DEFAULT N'Podnet',
    Napomena nvarchar(500) NULL,
    Verzija rowversion NOT NULL,
    CONSTRAINT CK_Period CHECK(DatumDo >= DatumOd),
    CONSTRAINT CK_Status CHECK(Status IN (N'Podnet', N'Odobren', N'Odbijen', N'Na čekanju'))
);
CREATE TABLE dbo.DanOdmora (
    ZahtevID int NOT NULL REFERENCES dbo.Zahtev(ZahtevID) ON DELETE CASCADE,
    Datum date NOT NULL,
    PRIMARY KEY(ZahtevID, Datum)
);
CREATE INDEX IX_Zahtev_Zaposleni_Status ON dbo.Zahtev(ZaposleniID, Status);
CREATE INDEX IX_DanOdmora_Datum ON dbo.DanOdmora(Datum, ZahtevID);
INSERT dbo.Sektor(Naziv) VALUES(N'Razvoj softvera'),(N'Prodaja'),(N'Administracija');
INSERT dbo.Zaposleni(ImePrezime,SektorID,RadnoMesto) VALUES
 (N'Ana Petrović',1,N'Programer'),(N'Marko Jovanović',1,N'Programer'),
 (N'Jelena Ilić',1,N'Tester'),(N'Nikola Savić',1,N'Programer'),
 (N'Milica Nikolić',2,N'Prodavac'),(N'Petar Stanković',2,N'Prodavac'),
 (N'Tamara Popović',3,N'Administrativni radnik');
END;
GO
CREATE OR ALTER PROCEDURE dbo.DajZaposlene
AS
 SELECT z.ZaposleniID,z.ImePrezime,z.SektorID,z.RadnoMesto,s.Naziv AS Sektor
 FROM dbo.Zaposleni z JOIN dbo.Sektor s ON s.SektorID=z.SektorID ORDER BY z.ImePrezime;
GO
CREATE OR ALTER PROCEDURE dbo.DajZahteve @Filter nvarchar(100)=N''
AS
 SELECT r.*,z.ImePrezime,z.SektorID,z.RadnoMesto,s.Naziv AS Sektor,
 (SELECT COUNT(*) FROM dbo.DanOdmora d WHERE d.ZahtevID=r.ZahtevID) AS BrojDana
 FROM dbo.Zahtev r JOIN dbo.Zaposleni z ON z.ZaposleniID=r.ZaposleniID
 JOIN dbo.Sektor s ON s.SektorID=z.SektorID
 WHERE z.ImePrezime LIKE N'%'+@Filter+N'%' OR s.Naziv LIKE N'%'+@Filter+N'%'
 OR r.Status LIKE N'%'+@Filter+N'%'
 ORDER BY r.ZahtevID DESC;
GO
SET NOEXEC OFF;
GO
