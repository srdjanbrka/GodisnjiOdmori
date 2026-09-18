using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace GodisnjiOdmori.Podaci
{
    public sealed class Repozitorijum : IDisposable
    {
        private readonly SqlConnection veza;
        private SqlTransaction transakcija;
        public Repozitorijum(string konekcija) { veza=new SqlConnection(konekcija); veza.Open(); }
        private SqlCommand Komanda(string sql, params object[] parovi)
        {
            var c=new SqlCommand(sql,veza,transakcija);
            for(int i=0;i<parovi.Length;i+=2) c.Parameters.AddWithValue((string)parovi[i],parovi[i+1]??DBNull.Value);
            return c;
        }
        public void PocniIzmenu()
        {
            transakcija=veza.BeginTransaction(IsolationLevel.Serializable);
            // Svi upisi se serijalizuju. Dovoljno za seminarski obim; sprečava dva odobrenja preko limita.
            using(var c=Komanda("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'GodisnjiOdmoriUpis', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; SELECT @r;"))
                if(Convert.ToInt32(c.ExecuteScalar())<0) throw new InvalidOperationException("Drugi zahtev se obrađuje. Pokušajte ponovo.");
        }
        public void Potvrdi() { transakcija.Commit(); transakcija.Dispose(); transakcija=null; }
        public List<Zaposleni> Zaposleni()
        {
            var lista=new List<Zaposleni>();
            using(var c=Komanda("dbo.DajZaposlene"))
            { c.CommandType=CommandType.StoredProcedure;
              using(var r=c.ExecuteReader()) while(r.Read()) lista.Add(new Zaposleni {
                ZaposleniID=(int)r["ZaposleniID"], SektorID=(int)r["SektorID"],
                ImePrezime=(string)r["ImePrezime"], Sektor=(string)r["Sektor"], RadnoMesto=(string)r["RadnoMesto"] }); }
            return lista;
        }
        private Zahtev Mapiraj(SqlDataReader r)
        {
            return new Zahtev { ZahtevID=(int)r["ZahtevID"],ZaposleniID=(int)r["ZaposleniID"],
                SektorID=(int)r["SektorID"],ImePrezime=(string)r["ImePrezime"],Sektor=(string)r["Sektor"],
                RadnoMesto=(string)r["RadnoMesto"],DatumOd=(DateTime)r["DatumOd"],DatumDo=(DateTime)r["DatumDo"],
                DatumPodnosenja=(DateTime)r["DatumPodnosenja"],Status=(string)r["Status"],
                Napomena=r["Napomena"]==DBNull.Value?null:(string)r["Napomena"],
                Verzija=Convert.ToBase64String((byte[])r["Verzija"]),BrojDana=(int)r["BrojDana"] };
        }
        public List<Zahtev> Zahtevi(string filter="",int? zaposleniId=null)
        {
            var lista=new List<Zahtev>();
            using(var c=Komanda(@"SELECT r.*,z.ImePrezime,z.SektorID,z.RadnoMesto,s.Naziv AS Sektor,
                (SELECT COUNT(*) FROM dbo.DanOdmora d WHERE d.ZahtevID=r.ZahtevID) AS BrojDana
                FROM dbo.Zahtev r JOIN dbo.Zaposleni z ON z.ZaposleniID=r.ZaposleniID
                JOIN dbo.Sektor s ON s.SektorID=z.SektorID
                WHERE (@z=0 OR r.ZaposleniID=@z) AND
                (z.ImePrezime LIKE N'%'+@f+N'%' OR s.Naziv LIKE N'%'+@f+N'%' OR r.Status LIKE N'%'+@f+N'%')
                ORDER BY r.ZahtevID DESC","@f",filter??"","@z",zaposleniId??0))
              using(var r=c.ExecuteReader()) while(r.Read()) lista.Add(Mapiraj(r));
            return lista;
        }
        public Zahtev Daj(int id)
        {
            Zahtev z=null;
            using(var c=Komanda(@"SELECT r.*,z.ImePrezime,z.SektorID,z.RadnoMesto,s.Naziv AS Sektor,
                (SELECT COUNT(*) FROM dbo.DanOdmora d WHERE d.ZahtevID=r.ZahtevID) AS BrojDana
                FROM dbo.Zahtev r JOIN dbo.Zaposleni z ON z.ZaposleniID=r.ZaposleniID
                JOIN dbo.Sektor s ON s.SektorID=z.SektorID WHERE r.ZahtevID=@id","@id",id))
            using(var r=c.ExecuteReader()) if(r.Read()) z=Mapiraj(r);
            if(z==null) return null;
            using(var c=Komanda("SELECT Datum FROM dbo.DanOdmora WHERE ZahtevID=@id ORDER BY Datum","@id",id))
            using(var r=c.ExecuteReader()) while(r.Read()) z.Dani.Add(r.GetDateTime(0));
            return z;
        }
        public List<Odsustvo> OdobrenaOdsustva(int sektor, DateTime od,DateTime doDatuma,int izuzetiId)
        {
            var lista=new List<Odsustvo>();
            using(var c=Komanda(@"SELECT DISTINCT z.ZaposleniID,d.Datum FROM dbo.DanOdmora d
                JOIN dbo.Zahtev r ON r.ZahtevID=d.ZahtevID JOIN dbo.Zaposleni z ON z.ZaposleniID=r.ZaposleniID
                WHERE r.Status=N'Odobren' AND z.SektorID=@s AND d.Datum BETWEEN @od AND @do AND r.ZahtevID<>@id",
                "@s",sektor,"@od",od,"@do",doDatuma,"@id",izuzetiId))
            using(var r=c.ExecuteReader()) while(r.Read()) lista.Add(new Odsustvo{ZaposleniID=r.GetInt32(0),Datum=r.GetDateTime(1)});
            return lista;
        }
        public bool ImaPreklapanje(Zahtev z)
        {
            using(var c=Komanda(@"SELECT COUNT(*) FROM dbo.Zahtev WHERE ZaposleniID=@z AND ZahtevID<>@id
                AND Status<>N'Odbijen' AND DatumOd<=@do AND DatumDo>=@od",
                "@z",z.ZaposleniID,"@id",z.ZahtevID,"@od",z.DatumOd,"@do",z.DatumDo))
                return Convert.ToInt32(c.ExecuteScalar())>0;
        }
        public int Sacuvaj(Zahtev z)
        {
            if(z.ZahtevID==0)
            {
                using(var c=Komanda(@"INSERT dbo.Zahtev(ZaposleniID,DatumOd,DatumDo,Napomena,Status)
                    OUTPUT INSERTED.ZahtevID VALUES(@z,@od,@do,@n,@s)","@z",z.ZaposleniID,"@od",z.DatumOd,"@do",z.DatumDo,"@n",z.Napomena,"@s",z.Status))
                    z.ZahtevID=Convert.ToInt32(c.ExecuteScalar());
            }
            else
            {
                using(var c=Komanda(@"UPDATE dbo.Zahtev SET ZaposleniID=@z,DatumOd=@od,DatumDo=@do,Napomena=@n,Status=@s WHERE ZahtevID=@id",
                    "@z",z.ZaposleniID,"@od",z.DatumOd,"@do",z.DatumDo,"@n",z.Napomena,"@s",z.Status,"@id",z.ZahtevID)) c.ExecuteNonQuery();
                using(var c=Komanda("DELETE dbo.DanOdmora WHERE ZahtevID=@id","@id",z.ZahtevID)) c.ExecuteNonQuery();
            }
            foreach(var dan in z.Dani)
                using(var c=Komanda("INSERT dbo.DanOdmora(ZahtevID,Datum) VALUES(@id,@d)","@id",z.ZahtevID,"@d",dan)) c.ExecuteNonQuery();
            return z.ZahtevID;
        }
        public void Status(int id,string status)
        { using(var c=Komanda("UPDATE dbo.Zahtev SET Status=@s WHERE ZahtevID=@id","@s",status,"@id",id)) c.ExecuteNonQuery(); }
        public void Obrisi(int id)
        { using(var c=Komanda("DELETE dbo.Zahtev WHERE ZahtevID=@id","@id",id)) c.ExecuteNonQuery(); }
        public void Dispose() { if(transakcija!=null) transakcija.Dispose(); veza.Dispose(); }
    }
}
