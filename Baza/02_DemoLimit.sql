USE GodisnjiOdmori;
GO
SET DATEFIRST 1;
DECLARE @Od date=DATEADD(day,8-DATEPART(weekday,CAST(GETDATE() AS date)),CAST(GETDATE() AS date));
DECLARE @Do date=DATEADD(day,4,@Od);

IF NOT EXISTS(SELECT 1 FROM dbo.Zahtev WHERE ZaposleniID=2 AND DatumOd=@Od AND DatumDo=@Do AND Status=N'Odobren')
BEGIN
    INSERT dbo.Zahtev(ZaposleniID,DatumOd,DatumDo,Status)
    VALUES(2,@Od,@Do,N'Odobren');
    DECLARE @Marko int=SCOPE_IDENTITY();
    INSERT dbo.DanOdmora(ZahtevID,Datum)
    SELECT @Marko,DATEADD(day,v.broj,@Od) FROM (VALUES(0),(1),(2),(3),(4))v(broj);
END;

IF NOT EXISTS(SELECT 1 FROM dbo.Zahtev WHERE ZaposleniID=3 AND DatumOd=@Od AND DatumDo=@Do AND Status=N'Odobren')
BEGIN
    INSERT dbo.Zahtev(ZaposleniID,DatumOd,DatumDo,Status)
    VALUES(3,@Od,@Do,N'Odobren');
    DECLARE @Jelena int=SCOPE_IDENTITY();
    INSERT dbo.DanOdmora(ZahtevID,Datum)
    SELECT @Jelena,DATEADD(day,v.broj,@Od) FROM (VALUES(0),(1),(2),(3),(4))v(broj);
END;

SELECT @Od AS PocetakDemoPerioda,@Do AS KrajDemoPerioda;
GO
