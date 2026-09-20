using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GodisnjiOdmori.Podaci
{
    public class Zaposleni
    {
        public int ZaposleniID { get; set; }
        public int SektorID { get; set; }
        public string ImePrezime { get; set; }
        public string Sektor { get; set; }
        public string RadnoMesto { get; set; }
    }
    public class Zahtev
    {
        public int ZahtevID { get; set; }
        [Range(1, int.MaxValue, ErrorMessage="Izaberite zaposlenog.")]
        public int ZaposleniID { get; set; }
        public int SektorID { get; set; }
        public string ImePrezime { get; set; }
        public string Sektor { get; set; }
        public string RadnoMesto { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime DatumOd { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime DatumDo { get; set; }
        public DateTime DatumPodnosenja { get; set; }
        public string Status { get; set; }
        public string Verzija { get; set; }
        public int BrojDana { get; set; }
        public List<DateTime> Dani { get; set; } = new List<DateTime>();
    }
    public class Odsustvo
    {
        public int ZaposleniID { get; set; }
        public DateTime Datum { get; set; }
    }
}
