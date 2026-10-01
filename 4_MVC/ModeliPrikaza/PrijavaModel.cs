using System.ComponentModel.DataAnnotations;

namespace GodisnjiOdmori.MVC.ModeliPrikaza
{
    public class PrijavaModel
    {
        [Required(ErrorMessage="Korisničko ime je obavezno.")]
        [StringLength(50)]
        [Display(Name="Korisničko ime")]
        public string KorisnickoIme { get; set; }

        [Required(ErrorMessage="Lozinka je obavezna.")]
        [StringLength(200)]
        [DataType(DataType.Password)]
        public string Lozinka { get; set; }
    }
}
