using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace GodisnjiOdmori.Podaci
{
    public class SektorRepozitorijum : Tabela
    {
        public SektorRepozitorijum(string konekcija) : base(konekcija) { }

        private static Sektor Mapiraj(DataRow red)
        {
            return new Sektor { SektorID=Convert.ToInt32(red["SektorID"]),Naziv=Convert.ToString(red["Naziv"]) };
        }

        public List<Sektor> DajSve()
        {
            var rezultat=new List<Sektor>();
            var tabela=Ucitaj("SELECT SektorID,Naziv FROM dbo.Sektor ORDER BY Naziv;");
            foreach(DataRow red in tabela.Rows) rezultat.Add(Mapiraj(red));
            return rezultat;
        }

        public Sektor Daj(int id)
        {
            var tabela=Ucitaj("SELECT SektorID,Naziv FROM dbo.Sektor WHERE SektorID=@SektorID;",
                new SqlParameter("@SektorID",id));
            return tabela.Rows.Count==0?null:Mapiraj(tabela.Rows[0]);
        }

        public int Dodaj(Sektor sektor)
        {
            return Convert.ToInt32(IzvrsiSkalar(
                "INSERT dbo.Sektor(Naziv) VALUES(@Naziv); SELECT SCOPE_IDENTITY();",
                new SqlParameter("@Naziv",sektor.Naziv)));
        }

        public void Izmeni(Sektor sektor)
        {
            Izvrsi("UPDATE dbo.Sektor SET Naziv=@Naziv WHERE SektorID=@SektorID;",
                new SqlParameter("@Naziv",sektor.Naziv),new SqlParameter("@SektorID",sektor.SektorID));
        }

        public void Obrisi(int id)
        {
            Izvrsi("DELETE dbo.Sektor WHERE SektorID=@SektorID;",new SqlParameter("@SektorID",id));
        }
    }
}
