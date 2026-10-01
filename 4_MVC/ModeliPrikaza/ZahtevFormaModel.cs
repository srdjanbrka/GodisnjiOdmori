using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GodisnjiOdmori.MVC.ModeliPrikaza
{
    public class ZahtevFormaModel : IValidatableObject
    {
        public int ZahtevID { get; set; }

        [Required(ErrorMessage="Datum početka je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name="Prvi dan odsustva")]
        public DateTime? DatumOd { get; set; }

        [Required(ErrorMessage="Datum završetka je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name="Poslednji dan odsustva")]
        public DateTime? DatumDo { get; set; }

        [StringLength(200)]
        public string Verzija { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(!DatumOd.HasValue || !DatumDo.HasValue) yield break;
            if(DatumOd.Value.Date<DateTime.Today)
                yield return new ValidationResult("Datum početka ne može biti pre današnjeg datuma.",new[]{"DatumOd"});
            if(DatumDo.Value.Date<DatumOd.Value.Date)
                yield return new ValidationResult("Datum završetka mora biti jednak ili veći od datuma početka.",new[]{"DatumDo"});
            if(DatumOd.Value.Year<2000 || DatumDo.Value.Year>2100
                || (DatumDo.Value.Date-DatumOd.Value.Date).TotalDays>366)
                yield return new ValidationResult("Unesite period između 2000. i 2100. godine, do 366 dana.",new[]{"DatumDo"});
        }
    }
}
