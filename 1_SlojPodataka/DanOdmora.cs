using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GodisnjiOdmori.Podaci
{
    [Table("DanOdmora")]
    public class DanOdmora
    {
        [Key, Column(Order=0)]
        public int ZahtevID { get; set; }

        [Key, Column(Order=1), DataType(DataType.Date)]
        public DateTime Datum { get; set; }

        public virtual Zahtev Zahtev { get; set; }
    }
}
