using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GodisnjiOdmori.Podaci
{
    [Table("Korisnik")]
    public class Korisnik
    {
        [Key]
        public int KorisnikID { get; set; }

        [Required, StringLength(50,MinimumLength=3)]
        public string KorisnickoIme { get; set; }

        [Required, StringLength(200)]
        public string LozinkaSalt { get; set; }

        [Required, StringLength(200)]
        public string LozinkaHash { get; set; }

        [Required, StringLength(20)]
        public string Uloga { get; set; }

        public int? ZaposleniID { get; set; }
        public bool Aktivan { get; set; }
    }
}
