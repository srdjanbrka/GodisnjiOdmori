using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GodisnjiOdmori.Podaci
{
    [Table("Zaposleni")]
    public class Zaposleni
    {
        public Zaposleni() { Zahtevi=new HashSet<Zahtev>(); }

        [Key]
        public int ZaposleniID { get; set; }

        [Required, StringLength(100,MinimumLength=3)]
        public string ImePrezime { get; set; }

        [Range(1,int.MaxValue)]
        public int SektorID { get; set; }

        [Required, StringLength(100,MinimumLength=2)]
        public string RadnoMesto { get; set; }

        [NotMapped]
        public string Sektor { get; set; }

        public virtual Sektor SektorPodatak { get; set; }
        public virtual ICollection<Zahtev> Zahtevi { get; set; }
    }
}
