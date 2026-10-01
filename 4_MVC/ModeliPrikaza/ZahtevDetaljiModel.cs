using System;

namespace GodisnjiOdmori.MVC.ModeliPrikaza
{
    public class ZahtevDetaljiModel
    {
        public int ZahtevID { get; set; }
        public int ZaposleniID { get; set; }
        public string ImePrezime { get; set; }
        public string Sektor { get; set; }
        public string RadnoMesto { get; set; }
        public DateTime DatumPodnosenja { get; set; }
        public DateTime DatumOd { get; set; }
        public DateTime DatumDo { get; set; }
        public string Status { get; set; }
        public int BrojRadnihDana { get; set; }
        public string Verzija { get; set; }
    }
}
