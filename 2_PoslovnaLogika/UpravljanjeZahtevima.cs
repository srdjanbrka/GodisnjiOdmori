using System;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using GodisnjiOdmori.Podaci;

namespace GodisnjiOdmori.Logika
{
    public sealed class UpravljanjeZahtevima
    {
        private readonly string konekcija;
        private static readonly HttpClient http=new HttpClient { Timeout=TimeSpan.FromSeconds(10) };
        public UpravljanjeZahtevima(string konekcija) { this.konekcija=konekcija; }

        public async Task<int> UcitajLimit()
        {
            try {
                var url=ConfigurationManager.AppSettings["ParametriUrl"];
                var xml=XDocument.Parse(await http.GetStringAsync(url));
                int x=int.Parse(xml.Root.Element("MaksimalnoOdsutnih").Value);
                if(x<1 || x>10000) throw new FormatException();
                return x;
            }
            catch(Exception e) when(e is HttpRequestException || e is TaskCanceledException
                || e is FormatException || e is System.Xml.XmlException || e is NullReferenceException)
            {
                throw new InvalidOperationException(
                    "Parametri nisu dostupni. Pokrenite REST servis i proverite XML. Zahtev nije izmenjen.",e);
            }
        }

        private static Zahtev Aktuelni(ZahtevRepozitorijum repozitorijum,int id,string verzija)
        {
            var zahtev=repozitorijum.Daj(id);
            if(zahtev==null) throw new InvalidOperationException("Zahtev ne postoji.");
            if(zahtev.Verzija!=verzija)
                throw new InvalidOperationException("Zahtev je u međuvremenu promenjen. Osvežite stranicu.");
            return zahtev;
        }

        private static void ZakljucajUpise(GodisnjiOdmoriContext kontekst)
        {
            int rezultat=kontekst.Database.SqlQuery<int>(@"DECLARE @r int;
                EXEC @r=sys.sp_getapplock @Resource=N'GodisnjiOdmoriUpis', @LockMode='Exclusive',
                @LockOwner='Transaction', @LockTimeout=10000; SELECT @r;").Single();
            if(rezultat<0) throw new InvalidOperationException("Drugi zahtev se obrađuje. Pokušajte ponovo.");
        }

        public int Sacuvaj(Zahtev unos,int ocekivaniVlasnik)
        {
            unos.DatumOd=unos.DatumOd.Date;
            unos.DatumDo=unos.DatumDo.Date;
            unos.Dani=Pravilo.RadniDani(unos.DatumOd,unos.DatumDo);

            using(var kontekst=new GodisnjiOdmoriContext(konekcija))
            using(var transakcija=kontekst.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                ZakljucajUpise(kontekst);
                var zahtevi=new ZahtevRepozitorijum(kontekst);
                var zaposleni=new ZaposleniRepozitorijum(kontekst);
                var dani=new DanOdmoraRepozitorijum(kontekst);

                if(zaposleni.Daj(ocekivaniVlasnik)==null) throw new InvalidOperationException("Zaposleni ne postoji.");
                unos.ZaposleniID=ocekivaniVlasnik;
                if(zahtevi.ImaPreklapanje(unos))
                    throw new InvalidOperationException("Zaposleni već ima aktivan zahtev za deo tog perioda.");

                if(unos.ZahtevID==0)
                {
                    unos.Status="Podnet";
                    unos.DatumPodnosenja=DateTime.Now;
                    foreach(var datum in unos.Dani) unos.DaniOdmora.Add(new DanOdmora { Datum=datum });
                    zahtevi.Dodaj(unos);
                }
                else
                {
                    var zahtev=Aktuelni(zahtevi,unos.ZahtevID,unos.Verzija);
                    if(zahtev.ZaposleniID!=ocekivaniVlasnik)
                        throw new InvalidOperationException("Nemate pravo da menjate ovaj zahtev.");
                    if(zahtev.Status=="Odobren")
                        throw new InvalidOperationException("Odobren zahtev prvo odbijte ako treba da ga izmenite.");

                    dani.ObrisiZaZahtev(zahtev.ZahtevID);
                    kontekst.SaveChanges();
                    zahtev.DaniOdmora.Clear();
                    zahtev.DatumOd=unos.DatumOd;
                    zahtev.DatumDo=unos.DatumDo;
                    zahtev.Status="Podnet";
                    foreach(var datum in unos.Dani)
                        dani.Dodaj(new DanOdmora { ZahtevID=zahtev.ZahtevID,Datum=datum });
                    zahtevi.Izmeni(zahtev);
                    unos=zahtev;
                }

                try { kontekst.SaveChanges(); }
                catch(DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException("Zahtev je u međuvremenu promenjen. Osvežite stranicu.");
                }
                transakcija.Commit();
                return unos.ZahtevID;
            }
        }

        public async Task<string> Odobri(int id,string verzija)
        {
            int x=await UcitajLimit();
            using(var kontekst=new GodisnjiOdmoriContext(konekcija))
            using(var transakcija=kontekst.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                ZakljucajUpise(kontekst);
                var zahtevi=new ZahtevRepozitorijum(kontekst);
                var zahtev=Aktuelni(zahtevi,id,verzija);
                if(zahtev.Status!="Podnet" && zahtev.Status!="Na čekanju")
                    throw new InvalidOperationException("Samo podnet zahtev ili zahtev na čekanju može se odobriti.");

                var odsustva=zahtevi.OdobrenaOdsustva(zahtev.SektorID,zahtev.DatumOd,zahtev.DatumDo,zahtev.ZahtevID);
                string status=Pravilo.PrekoracenLimit(zahtev.Dani,odsustva,zahtev.ZaposleniID,x)
                    ?"Na čekanju":"Odobren";
                zahtevi.PromeniStatus(zahtev,status);
                try { kontekst.SaveChanges(); }
                catch(DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException("Zahtev je u međuvremenu promenjen. Osvežite stranicu.");
                }
                transakcija.Commit();
                return status;
            }
        }

        public void OdbijIliObrisi(int id,string verzija,bool obrisi,int? ocekivaniVlasnik=null)
        {
            using(var kontekst=new GodisnjiOdmoriContext(konekcija))
            using(var transakcija=kontekst.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                ZakljucajUpise(kontekst);
                var zahtevi=new ZahtevRepozitorijum(kontekst);
                var zahtev=Aktuelni(zahtevi,id,verzija);
                if(ocekivaniVlasnik.HasValue && zahtev.ZaposleniID!=ocekivaniVlasnik.Value)
                    throw new InvalidOperationException("Nemate pravo da obrišete ovaj zahtev.");
                if(obrisi && zahtev.Status=="Odobren")
                    throw new InvalidOperationException("Odobren zahtev prvo odbijte, pa obrišite.");

                if(obrisi) zahtevi.Obrisi(id); else zahtevi.PromeniStatus(zahtev,"Odbijen");
                try { kontekst.SaveChanges(); }
                catch(DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException("Zahtev je u međuvremenu promenjen. Osvežite stranicu.");
                }
                transakcija.Commit();
            }
        }
    }
}
