using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GodisnjiOdmori.Podaci
{
    [Table("Zahtev")]
    public class Zahtev
    {
        public Zahtev()
        {
            DaniOdmora=new HashSet<DanOdmora>();
            Dani=new List<DateTime>();
        }

        [Key]
        public int ZahtevID { get; set; }

        [Range(1,int.MaxValue)]
        public int ZaposleniID { get; set; }

        public DateTime DatumPodnosenja { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime DatumOd { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime DatumDo { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; }

        [Timestamp, Column("Verzija")]
        public byte[] RedVerzija { get; set; }

        public virtual Zaposleni ZaposleniPodatak { get; set; }
        public virtual ICollection<DanOdmora> DaniOdmora { get; set; }

        [NotMapped] public int SektorID { get; set; }
        [NotMapped] public string ImePrezime { get; set; }
        [NotMapped] public string Sektor { get; set; }
        [NotMapped] public string RadnoMesto { get; set; }
        [NotMapped] public string Verzija { get; set; }
        [NotMapped] public int BrojDana { get; set; }
        [NotMapped] public List<DateTime> Dani { get; set; }
    }
}
