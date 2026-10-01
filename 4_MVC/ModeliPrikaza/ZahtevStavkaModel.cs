using System;

namespace GodisnjiOdmori.MVC.ModeliPrikaza
{
    public class ZahtevStavkaModel
    {
        public int ZahtevID { get; set; }
        public string ImePrezime { get; set; }
        public string Sektor { get; set; }
        public DateTime DatumOd { get; set; }
        public DateTime DatumDo { get; set; }
        public int BrojDana { get; set; }
        public string Status { get; set; }
    }
}
