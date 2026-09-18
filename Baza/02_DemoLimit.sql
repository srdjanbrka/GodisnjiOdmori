-- OPCIONO: pokrenuti samo ako želite brzu demonstraciju poslovnog pravila X=2.
-- Dodaje dva odobrena zahteva iz sektora Razvoj softvera za sledeću radnu nedelju.
-- Skripta je idempotentna: zahteve sa oznakom DEMO_LIMIT_X neće dodati ponovo.
USE GodisnjiOdmori;
GO
SET DATEFIRST 1;
DECLARE @Od date=DATEADD(day,8-DATEPART(weekday,CAST(GETDATE() AS date)),CAST(GETDATE() AS date));
DECLARE @Do date=DATEADD(day,4,@Od);

IF NOT EXISTS(SELECT 1 FROM dbo.Zahtev WHERE Napomena=N'DEMO_LIMIT_X_MARKO')
BEGIN
    INSERT dbo.Zahtev(ZaposleniID,DatumOd,DatumDo,Napomena,Status)
    VALUES(2,@Od,@Do,N'DEMO_LIMIT_X_MARKO',N'Odobren');
    DECLARE @Marko int=SCOPE_IDENTITY();
    INSERT dbo.DanOdmora(ZahtevID,Datum)
    SELECT @Marko,DATEADD(day,v.broj,@Od) FROM (VALUES(0),(1),(2),(3),(4))v(broj);
END;

IF NOT EXISTS(SELECT 1 FROM dbo.Zahtev WHERE Napomena=N'DEMO_LIMIT_X_JELENA')
BEGIN
    INSERT dbo.Zahtev(ZaposleniID,DatumOd,DatumDo,Napomena,Status)
    VALUES(3,@Od,@Do,N'DEMO_LIMIT_X_JELENA',N'Odobren');
    DECLARE @Jelena int=SCOPE_IDENTITY();
    INSERT dbo.DanOdmora(ZahtevID,Datum)
    SELECT @Jelena,DATEADD(day,v.broj,@Od) FROM (VALUES(0),(1),(2),(3),(4))v(broj);
END;

SELECT @Od AS PocetakDemoPerioda,@Do AS KrajDemoPerioda;
GO
