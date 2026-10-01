using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace GodisnjiOdmori.Podaci
{
    public class ZahtevRepozitorijum
    {
        private readonly GodisnjiOdmoriContext kontekst;
        public ZahtevRepozitorijum(GodisnjiOdmoriContext kontekst) { this.kontekst=kontekst; }

        private static void PopuniPrikaz(Zahtev zahtev)
        {
            if(zahtev==null || zahtev.ZaposleniPodatak==null) return;
            zahtev.ImePrezime=zahtev.ZaposleniPodatak.ImePrezime;
            zahtev.SektorID=zahtev.ZaposleniPodatak.SektorID;
            zahtev.RadnoMesto=zahtev.ZaposleniPodatak.RadnoMesto;
            zahtev.Sektor=zahtev.ZaposleniPodatak.SektorPodatak.Naziv;
            zahtev.Dani=zahtev.DaniOdmora.Select(d=>d.Datum).OrderBy(d=>d).ToList();
            zahtev.BrojDana=zahtev.Dani.Count;
            zahtev.Verzija=zahtev.RedVerzija==null?null:Convert.ToBase64String(zahtev.RedVerzija);
        }

        public List<Zahtev> DajSve(string filter="",int? zaposleniId=null)
        {
            filter=filter??"";
            var upit=kontekst.Zahtevi.AsNoTracking()
                .Include(z=>z.ZaposleniPodatak.SektorPodatak)
                .Include(z=>z.DaniOdmora).AsQueryable();
            if(zaposleniId.HasValue) upit=upit.Where(z=>z.ZaposleniID==zaposleniId.Value);
            if(filter.Length>0) upit=upit.Where(z=>z.ZaposleniPodatak.ImePrezime.Contains(filter)
                || z.ZaposleniPodatak.SektorPodatak.Naziv.Contains(filter) || z.Status.Contains(filter));
            var lista=upit.OrderByDescending(z=>z.ZahtevID).ToList();
            foreach(var zahtev in lista) PopuniPrikaz(zahtev);
            return lista;
        }

        public Zahtev Daj(int id)
        {
            var zahtev=kontekst.Zahtevi.Include(z=>z.ZaposleniPodatak.SektorPodatak)
                .Include(z=>z.DaniOdmora).SingleOrDefault(z=>z.ZahtevID==id);
            PopuniPrikaz(zahtev);
            return zahtev;
        }

        public void Dodaj(Zahtev zahtev) { kontekst.Zahtevi.Add(zahtev); }

        public void Izmeni(Zahtev zahtev)
        {
            if(kontekst.Entry(zahtev).State==EntityState.Detached) kontekst.Zahtevi.Attach(zahtev);
            kontekst.Entry(zahtev).State=EntityState.Modified;
        }

        public void Obrisi(int id)
        {
            var zahtev=Daj(id);
            if(zahtev!=null) kontekst.Zahtevi.Remove(zahtev);
        }

        public void PromeniStatus(Zahtev zahtev,string status)
        {
            zahtev.Status=status;
            kontekst.Entry(zahtev).Property(z=>z.Status).IsModified=true;
        }

        public bool ImaPreklapanje(Zahtev zahtev)
        {
            return kontekst.Database.SqlQuery<int>(
                "EXEC dbo.Zahtev_ImaPreklapanje @ZaposleniID,@ZahtevID,@DatumOd,@DatumDo",
                new SqlParameter("@ZaposleniID",zahtev.ZaposleniID),
                new SqlParameter("@ZahtevID",zahtev.ZahtevID),
                new SqlParameter("@DatumOd",zahtev.DatumOd),
                new SqlParameter("@DatumDo",zahtev.DatumDo)).Single()>0;
        }

        public List<Odsustvo> OdobrenaOdsustva(int sektorId,DateTime datumOd,DateTime datumDo,int izuzetiZahtevId)
        {
            return kontekst.Database.SqlQuery<Odsustvo>(
                "EXEC dbo.Zahtev_DajOdobrenaOdsustva @SektorID,@DatumOd,@DatumDo,@IzuzetiZahtevID",
                new SqlParameter("@SektorID",sektorId),new SqlParameter("@DatumOd",datumOd),
                new SqlParameter("@DatumDo",datumDo),new SqlParameter("@IzuzetiZahtevID",izuzetiZahtevId)).ToList();
        }
    }
}
