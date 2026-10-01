using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace GodisnjiOdmori.Podaci
{
    public class DanOdmoraRepozitorijum
    {
        private readonly GodisnjiOdmoriContext kontekst;
        public DanOdmoraRepozitorijum(GodisnjiOdmoriContext kontekst) { this.kontekst=kontekst; }

        public List<DanOdmora> DajSve() { return kontekst.DaniOdmora.AsNoTracking().OrderBy(d=>d.Datum).ToList(); }
        public DanOdmora Daj(int zahtevId,DateTime datum) { return kontekst.DaniOdmora.Find(zahtevId,datum.Date); }
        public void Dodaj(DanOdmora dan) { kontekst.DaniOdmora.Add(dan); }
        public void Izmeni(int zahtevId,DateTime stariDatum,DateTime noviDatum)
        {
            Obrisi(zahtevId,stariDatum);
            Dodaj(new DanOdmora { ZahtevID=zahtevId,Datum=noviDatum.Date });
        }
        public void Obrisi(int zahtevId,DateTime datum) { var dan=Daj(zahtevId,datum); if(dan!=null) kontekst.DaniOdmora.Remove(dan); }
        public void ObrisiZaZahtev(int zahtevId) { kontekst.DaniOdmora.RemoveRange(kontekst.DaniOdmora.Where(d=>d.ZahtevID==zahtevId)); }
    }
}
