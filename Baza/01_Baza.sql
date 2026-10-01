USE master;
GO
IF DB_ID(N'GodisnjiOdmori') IS NOT NULL
BEGIN
    PRINT N'Baza GodisnjiOdmori vec postoji. Pokrenite Baza/03_KorekcijeProfesor.sql.';
    SET NOEXEC ON;
END;
GO
IF DB_ID(N'GodisnjiOdmori') IS NULL EXEC(N'CREATE DATABASE GodisnjiOdmori');
GO
USE GodisnjiOdmori;
GO

CREATE TABLE dbo.Sektor (
    SektorID int IDENTITY PRIMARY KEY,
    Naziv nvarchar(100) NOT NULL UNIQUE,
    CONSTRAINT CK_Sektor_Naziv CHECK(LEN(LTRIM(RTRIM(Naziv)))>=2)
);

CREATE TABLE dbo.Zaposleni (
    ZaposleniID int IDENTITY PRIMARY KEY,
    ImePrezime nvarchar(100) NOT NULL,
    SektorID int NOT NULL REFERENCES dbo.Sektor(SektorID),
    RadnoMesto nvarchar(100) NOT NULL,
    CONSTRAINT CK_Zaposleni_Ime CHECK(LEN(LTRIM(RTRIM(ImePrezime)))>=3),
    CONSTRAINT CK_Zaposleni_RadnoMesto CHECK(LEN(LTRIM(RTRIM(RadnoMesto)))>=2)
);

CREATE TABLE dbo.Zahtev (
    ZahtevID int IDENTITY PRIMARY KEY,
    ZaposleniID int NOT NULL REFERENCES dbo.Zaposleni(ZaposleniID),
    DatumPodnosenja datetime2 NOT NULL DEFAULT SYSDATETIME(),
    DatumOd date NOT NULL,
    DatumDo date NOT NULL,
    Status nvarchar(20) NOT NULL DEFAULT N'Podnet',
    Verzija rowversion NOT NULL,
    CONSTRAINT CK_Zahtev_Period CHECK(DatumDo>=DatumOd),
    CONSTRAINT CK_Zahtev_Status CHECK(Status IN (N'Podnet',N'Odobren',N'Odbijen',N'Na čekanju'))
);

CREATE TABLE dbo.DanOdmora (
    ZahtevID int NOT NULL REFERENCES dbo.Zahtev(ZahtevID) ON DELETE CASCADE,
    Datum date NOT NULL,
    CONSTRAINT PK_DanOdmora PRIMARY KEY(ZahtevID,Datum)
);

CREATE TABLE dbo.Korisnik (
    KorisnikID int IDENTITY PRIMARY KEY,
    KorisnickoIme nvarchar(50) NOT NULL UNIQUE,
    LozinkaSalt nvarchar(200) NOT NULL,
    LozinkaHash nvarchar(200) NOT NULL,
    Uloga nvarchar(20) NOT NULL,
    ZaposleniID int NULL,
    Aktivan bit NOT NULL DEFAULT 1,
    CONSTRAINT CK_Korisnik_Ime CHECK(LEN(LTRIM(RTRIM(KorisnickoIme)))>=3),
    CONSTRAINT CK_Korisnik_Uloga CHECK(Uloga IN (N'Zaposleni',N'Kadrovska')),
    CONSTRAINT CK_Korisnik_Veza CHECK((Uloga=N'Zaposleni' AND ZaposleniID IS NOT NULL) OR Uloga=N'Kadrovska')
);

CREATE INDEX IX_Zahtev_Zaposleni_Status ON dbo.Zahtev(ZaposleniID,Status);
CREATE INDEX IX_DanOdmora_Datum ON dbo.DanOdmora(Datum,ZahtevID);
CREATE INDEX IX_Korisnik_Zaposleni ON dbo.Korisnik(ZaposleniID);

INSERT dbo.Sektor(Naziv) VALUES(N'Razvoj softvera'),(N'Prodaja'),(N'Administracija');
INSERT dbo.Zaposleni(ImePrezime,SektorID,RadnoMesto) VALUES
 (N'Ana Petrović',1,N'Programer'),(N'Marko Jovanović',1,N'Programer'),
 (N'Jelena Ilić',1,N'Tester'),(N'Nikola Savić',1,N'Programer'),
 (N'Milica Nikolić',2,N'Prodavac'),(N'Petar Stanković',2,N'Prodavac'),
 (N'Tamara Popović',3,N'Administrativni radnik');

INSERT dbo.Korisnik(KorisnickoIme,LozinkaSalt,LozinkaHash,Uloga,ZaposleniID) VALUES
 (N'zaposleni',N'WmFwb3NsZW5pU2FsdDIwMjY=',N'eOiTbsKr4iVkAgBKKrRoim4YuG1WoZXNGkek5lXEsm4=',N'Zaposleni',1),
 (N'kadrovska',N'S2Fkcm92c2thU2FsdDIwMjY=',N'LGRyX8mr8zNzZ1Qh1Bylt7IGTHxYdPmurt+vbHVQLgw=',N'Kadrovska',NULL);
GO

CREATE OR ALTER PROCEDURE dbo.DajZaposlene
AS
BEGIN
    SET NOCOUNT ON;
    SELECT z.ZaposleniID,z.ImePrezime,z.SektorID,z.RadnoMesto,s.Naziv AS Sektor
    FROM dbo.Zaposleni z JOIN dbo.Sektor s ON s.SektorID=z.SektorID
    ORDER BY z.ImePrezime;
END;
GO

CREATE OR ALTER PROCEDURE dbo.DajZahteve
    @Filter nvarchar(100)=N'',@ZaposleniID int=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.*,z.ImePrezime,z.SektorID,z.RadnoMesto,s.Naziv AS Sektor,
      (SELECT COUNT(*) FROM dbo.DanOdmora d WHERE d.ZahtevID=r.ZahtevID) AS BrojDana
    FROM dbo.Zahtev r
    JOIN dbo.Zaposleni z ON z.ZaposleniID=r.ZaposleniID
    JOIN dbo.Sektor s ON s.SektorID=z.SektorID
    WHERE (@ZaposleniID IS NULL OR r.ZaposleniID=@ZaposleniID)
      AND (z.ImePrezime LIKE N'%'+@Filter+N'%' OR s.Naziv LIKE N'%'+@Filter+N'%' OR r.Status LIKE N'%'+@Filter+N'%')
    ORDER BY r.ZahtevID DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Korisnik_DajPoKorisnickomImenu @KorisnickoIme nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT KorisnikID,KorisnickoIme,LozinkaSalt,LozinkaHash,Uloga,ZaposleniID,Aktivan
    FROM dbo.Korisnik WHERE KorisnickoIme=@KorisnickoIme;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Korisnik_DajSve
AS
BEGIN
    SET NOCOUNT ON;
    SELECT KorisnikID,KorisnickoIme,LozinkaSalt,LozinkaHash,Uloga,ZaposleniID,Aktivan
    FROM dbo.Korisnik ORDER BY KorisnickoIme;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Korisnik_DajPoID @KorisnikID int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT KorisnikID,KorisnickoIme,LozinkaSalt,LozinkaHash,Uloga,ZaposleniID,Aktivan
    FROM dbo.Korisnik WHERE KorisnikID=@KorisnikID;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Korisnik_Dodaj
    @KorisnickoIme nvarchar(50),@LozinkaSalt nvarchar(200),@LozinkaHash nvarchar(200),
    @Uloga nvarchar(20),@ZaposleniID int=NULL,@Aktivan bit=1
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Korisnik(KorisnickoIme,LozinkaSalt,LozinkaHash,Uloga,ZaposleniID,Aktivan)
    VALUES(@KorisnickoIme,@LozinkaSalt,@LozinkaHash,@Uloga,@ZaposleniID,@Aktivan);
    SELECT CONVERT(int,SCOPE_IDENTITY());
END;
GO

CREATE OR ALTER PROCEDURE dbo.Korisnik_Izmeni
    @KorisnikID int,@KorisnickoIme nvarchar(50),@LozinkaSalt nvarchar(200),
    @LozinkaHash nvarchar(200),@Uloga nvarchar(20),@ZaposleniID int=NULL,@Aktivan bit
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Korisnik SET KorisnickoIme=@KorisnickoIme,LozinkaSalt=@LozinkaSalt,
      LozinkaHash=@LozinkaHash,Uloga=@Uloga,ZaposleniID=@ZaposleniID,Aktivan=@Aktivan
    WHERE KorisnikID=@KorisnikID;
    SELECT @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Korisnik_Obrisi @KorisnikID int
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.Korisnik WHERE KorisnikID=@KorisnikID;
    SELECT @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Zahtev_ImaPreklapanje
    @ZaposleniID int,@ZahtevID int,@DatumOd date,@DatumDo date
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CONVERT(int,COUNT(*)) FROM dbo.Zahtev
    WHERE ZaposleniID=@ZaposleniID AND ZahtevID<>@ZahtevID
      AND Status<>N'Odbijen' AND DatumOd<=@DatumDo AND DatumDo>=@DatumOd;
END;
GO

CREATE OR ALTER PROCEDURE dbo.Zahtev_DajOdobrenaOdsustva
    @SektorID int,@DatumOd date,@DatumDo date,@IzuzetiZahtevID int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT z.ZaposleniID,d.Datum
    FROM dbo.DanOdmora d
    JOIN dbo.Zahtev r ON r.ZahtevID=d.ZahtevID
    JOIN dbo.Zaposleni z ON z.ZaposleniID=r.ZaposleniID
    WHERE r.Status=N'Odobren' AND z.SektorID=@SektorID
      AND d.Datum BETWEEN @DatumOd AND @DatumDo AND r.ZahtevID<>@IzuzetiZahtevID;
END;
GO

IF NOT EXISTS(SELECT 1 FROM sys.database_principals WHERE name=N'GodisnjiOdmoriAplikacija')
    CREATE USER GodisnjiOdmoriAplikacija WITHOUT LOGIN;
GRANT SELECT,INSERT,UPDATE,DELETE TO GodisnjiOdmoriAplikacija;
GRANT EXECUTE TO GodisnjiOdmoriAplikacija;
GO
SET NOEXEC OFF;
GO
