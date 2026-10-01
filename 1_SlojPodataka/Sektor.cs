using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GodisnjiOdmori.Podaci
{
    [Table("Sektor")]
    public class Sektor
    {
        public Sektor() { Zaposleni=new HashSet<Zaposleni>(); }

        [Key]
        public int SektorID { get; set; }

        [Required, StringLength(100,MinimumLength=2)]
        public string Naziv { get; set; }

        public virtual ICollection<Zaposleni> Zaposleni { get; set; }
    }
}
