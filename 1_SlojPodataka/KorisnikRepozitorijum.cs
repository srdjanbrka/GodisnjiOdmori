using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace GodisnjiOdmori.Podaci
{
    public class KorisnikRepozitorijum
    {
        private readonly string konekcija;
        public KorisnikRepozitorijum(string konekcija) { this.konekcija=konekcija; }

        private static Korisnik Mapiraj(SqlDataReader citac)
        {
            return new Korisnik {
                KorisnikID=Convert.ToInt32(citac["KorisnikID"]),
                KorisnickoIme=Convert.ToString(citac["KorisnickoIme"]),
                LozinkaSalt=Convert.ToString(citac["LozinkaSalt"]),
                LozinkaHash=Convert.ToString(citac["LozinkaHash"]),
                Uloga=Convert.ToString(citac["Uloga"]),
                ZaposleniID=citac["ZaposleniID"]==DBNull.Value?(int?)null:Convert.ToInt32(citac["ZaposleniID"]),
                Aktivan=Convert.ToBoolean(citac["Aktivan"])
            };
        }

        private List<Korisnik> Ucitaj(string procedura,params SqlParameter[] parametri)
        {
            var rezultat=new List<Korisnik>();
            using(var veza=new SqlConnection(konekcija))
            using(var komanda=new SqlCommand(procedura,veza))
            {
                komanda.CommandType=CommandType.StoredProcedure;
                if(parametri!=null && parametri.Length>0) komanda.Parameters.AddRange(parametri);
                veza.Open();
                using(var citac=komanda.ExecuteReader())
                    while(citac.Read()) rezultat.Add(Mapiraj(citac));
            }
            return rezultat;
        }

        private int Izvrsi(string procedura,params SqlParameter[] parametri)
        {
            using(var veza=new SqlConnection(konekcija))
            using(var komanda=new SqlCommand(procedura,veza))
            {
                komanda.CommandType=CommandType.StoredProcedure;
                if(parametri!=null && parametri.Length>0) komanda.Parameters.AddRange(parametri);
                veza.Open();
                return Convert.ToInt32(komanda.ExecuteScalar());
            }
        }

        public List<Korisnik> DajSve() { return Ucitaj("dbo.Korisnik_DajSve"); }

        public Korisnik Daj(int id)
        {
            var lista=Ucitaj("dbo.Korisnik_DajPoID",new SqlParameter("@KorisnikID",id));
            return lista.Count==0?null:lista[0];
        }

        public Korisnik DajPoKorisnickomImenu(string korisnickoIme)
        {
            var lista=Ucitaj("dbo.Korisnik_DajPoKorisnickomImenu",new SqlParameter("@KorisnickoIme",korisnickoIme));
            return lista.Count==0?null:lista[0];
        }

        public int Dodaj(Korisnik korisnik) { return Izvrsi("dbo.Korisnik_Dodaj",Parametri(korisnik)); }

        public void Izmeni(Korisnik korisnik)
        {
            var parametri=new List<SqlParameter>(Parametri(korisnik));
            parametri.Insert(0,new SqlParameter("@KorisnikID",korisnik.KorisnikID));
            Izvrsi("dbo.Korisnik_Izmeni",parametri.ToArray());
        }

        public void Obrisi(int id)
        {
            Izvrsi("dbo.Korisnik_Obrisi",new SqlParameter("@KorisnikID",id));
        }

        private static SqlParameter[] Parametri(Korisnik korisnik)
        {
            return new[] {
                new SqlParameter("@KorisnickoIme",korisnik.KorisnickoIme),
                new SqlParameter("@LozinkaSalt",korisnik.LozinkaSalt),
                new SqlParameter("@LozinkaHash",korisnik.LozinkaHash),
                new SqlParameter("@Uloga",korisnik.Uloga),
                new SqlParameter("@ZaposleniID",(object)korisnik.ZaposleniID??DBNull.Value),
                new SqlParameter("@Aktivan",korisnik.Aktivan)
            };
        }
    }
}
