using System;
using System.Data;
using System.Data.SqlClient;

namespace GodisnjiOdmori.Podaci
{
    public abstract class Tabela
    {
        private readonly string konekcija;

        protected Tabela(string konekcija)
        {
            if(String.IsNullOrWhiteSpace(konekcija))
                throw new ArgumentException("Konekcija sa bazom je obavezna.","konekcija");
            this.konekcija=konekcija;
        }

        protected DataTable Ucitaj(string sql,params SqlParameter[] parametri)
        {
            var tabela=new DataTable();
            using(var veza=new SqlConnection(konekcija))
            using(var komanda=new SqlCommand(sql,veza))
            using(var adapter=new SqlDataAdapter(komanda))
            {
                if(parametri!=null && parametri.Length>0) komanda.Parameters.AddRange(parametri);
                adapter.Fill(tabela);
            }
            return tabela;
        }

        protected int Izvrsi(string sql,params SqlParameter[] parametri)
        {
            using(var veza=new SqlConnection(konekcija))
            using(var komanda=new SqlCommand(sql,veza))
            {
                if(parametri!=null && parametri.Length>0) komanda.Parameters.AddRange(parametri);
                veza.Open();
                return komanda.ExecuteNonQuery();
            }
        }

        protected object IzvrsiSkalar(string sql,params SqlParameter[] parametri)
        {
            using(var veza=new SqlConnection(konekcija))
            using(var komanda=new SqlCommand(sql,veza))
            {
                if(parametri!=null && parametri.Length>0) komanda.Parameters.AddRange(parametri);
                veza.Open();
                return komanda.ExecuteScalar();
            }
        }
    }
}
