using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace GodisnjiOdmori.Podaci
{
    public class ZaposleniRepozitorijum
    {
        private readonly GodisnjiOdmoriContext kontekst;
        public ZaposleniRepozitorijum(GodisnjiOdmoriContext kontekst) { this.kontekst=kontekst; }

        public List<Zaposleni> DajSve()
        {
            var lista=kontekst.Zaposleni.AsNoTracking().Include(z=>z.SektorPodatak).OrderBy(z=>z.ImePrezime).ToList();
            foreach(var z in lista) z.Sektor=z.SektorPodatak.Naziv;
            return lista;
        }

        public Zaposleni Daj(int id)
        {
            var zaposleni=kontekst.Zaposleni.Include(z=>z.SektorPodatak).SingleOrDefault(z=>z.ZaposleniID==id);
            if(zaposleni!=null) zaposleni.Sektor=zaposleni.SektorPodatak.Naziv;
            return zaposleni;
        }

        public void Dodaj(Zaposleni zaposleni) { kontekst.Zaposleni.Add(zaposleni); }
        public void Izmeni(Zaposleni zaposleni) { kontekst.Entry(zaposleni).State=EntityState.Modified; }
        public void Obrisi(int id) { var zaposleni=Daj(id); if(zaposleni!=null) kontekst.Zaposleni.Remove(zaposleni); }
    }
}
