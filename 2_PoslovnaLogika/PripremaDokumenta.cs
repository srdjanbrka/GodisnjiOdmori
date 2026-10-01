using System;
using GodisnjiOdmori.Podaci;

namespace GodisnjiOdmori.Logika
{
    public static class PripremaDokumenta
    {
        public static DokumentZahteva Pripremi(Zahtev zahtev)
        {
            if(zahtev==null) throw new ArgumentNullException("zahtev");
            return new DokumentZahteva {
                ZahtevID=zahtev.ZahtevID,ZaposleniID=zahtev.ZaposleniID,
                ImePrezime=zahtev.ImePrezime,Sektor=zahtev.Sektor,RadnoMesto=zahtev.RadnoMesto,
                DatumPodnosenja=zahtev.DatumPodnosenja,DatumOd=zahtev.DatumOd,DatumDo=zahtev.DatumDo,
                Status=zahtev.Status,BrojRadnihDana=zahtev.BrojDana,Verzija=zahtev.Verzija
            };
        }
    }
}
