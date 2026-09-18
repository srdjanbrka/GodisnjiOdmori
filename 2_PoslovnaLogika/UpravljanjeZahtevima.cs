using System;
using System.Configuration;
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
        public UpravljanjeZahtevima(string k) { konekcija=k; }
        public async Task<int> UcitajLimit()
        {
            try {
                var url=ConfigurationManager.AppSettings["ParametriUrl"];
                var xml=XDocument.Parse(await http.GetStringAsync(url));
                int x=int.Parse(xml.Root.Element("MaksimalnoOdsutnih").Value);
                if(x<1 || x>10000) throw new FormatException();
                return x;
            } catch(Exception e) when(e is HttpRequestException || e is TaskCanceledException || e is FormatException || e is System.Xml.XmlException || e is NullReferenceException) {
                throw new InvalidOperationException("Parametri nisu dostupni. Pokrenite REST servis i proverite XML. Zahtev nije izmenjen.",e);
            }
        }
        private static Zahtev Aktuelni(Repozitorijum r,int id,string verzija)
        {
            var z=r.Daj(id);
            if(z==null) throw new InvalidOperationException("Zahtev ne postoji.");
            if(z.Verzija!=verzija) throw new InvalidOperationException("Zahtev je u međuvremenu promenjen. Osvežite stranicu.");
            return z;
        }
        public int Sacuvaj(Zahtev z,int ocekivaniVlasnik)
        {
            z.DatumOd=z.DatumOd.Date; z.DatumDo=z.DatumDo.Date;
            z.Dani=Pravilo.RadniDani(z.DatumOd,z.DatumDo);
            using(var r=new Repozitorijum(konekcija)) {
                r.PocniIzmenu();
                if(z.ZahtevID!=0) {
                    var stari=Aktuelni(r,z.ZahtevID,z.Verzija);
                    if(stari.ZaposleniID!=ocekivaniVlasnik) throw new InvalidOperationException("Nemate pravo da menjate ovaj zahtev.");
                    if(stari.Status=="Odobren") throw new InvalidOperationException("Odobren zahtev prvo odbijte ako treba da ga izmenite.");
                }
                z.ZaposleniID=ocekivaniVlasnik;
                var zaposleni=r.Zaposleni().SingleOrDefault(e=>e.ZaposleniID==z.ZaposleniID);
                if(zaposleni==null) throw new InvalidOperationException("Zaposleni ne postoji.");
                if(r.ImaPreklapanje(z)) throw new InvalidOperationException("Zaposleni već ima aktivan zahtev za deo tog perioda.");
                z.Status="Podnet";
                int id=r.Sacuvaj(z); r.Potvrdi(); return id;
            }
        }
        public async Task<string> Odobri(int id,string verzija)
        {
            int x=await UcitajLimit();
            using(var r=new Repozitorijum(konekcija)) {
                r.PocniIzmenu(); var z=Aktuelni(r,id,verzija);
                if(z.Status!="Podnet" && z.Status!="Na čekanju") throw new InvalidOperationException("Samo podnet zahtev ili zahtev na čekanju može se odobriti.");
                var odsustva=r.OdobrenaOdsustva(z.SektorID,z.DatumOd,z.DatumDo,z.ZahtevID);
                string status=Pravilo.PrekoracenLimit(z.Dani,odsustva,z.ZaposleniID,x)?"Na čekanju":"Odobren";
                r.Status(id,status); r.Potvrdi(); return status;
            }
        }
        public void OdbijIliObrisi(int id,string verzija,bool obrisi,int? ocekivaniVlasnik=null)
        {
            using(var r=new Repozitorijum(konekcija)) {
                r.PocniIzmenu(); var z=Aktuelni(r,id,verzija);
                if(ocekivaniVlasnik.HasValue && z.ZaposleniID!=ocekivaniVlasnik.Value)
                    throw new InvalidOperationException("Nemate pravo da obrišete ovaj zahtev.");
                if(obrisi && z.Status=="Odobren") throw new InvalidOperationException("Odobren zahtev prvo odbijte, pa obrišite.");
                if(obrisi) r.Obrisi(id); else r.Status(id,"Odbijen");
                r.Potvrdi();
            }
        }
    }
}
