using System.Collections.Generic;

namespace GodisnjiOdmori.MVC.ModeliPrikaza
{
    public class ZahteviListaModel
    {
        public ZahteviListaModel() { Zahtevi=new List<ZahtevStavkaModel>(); }
        public string Filter { get; set; }
        public List<ZahtevStavkaModel> Zahtevi { get; set; }
    }
}
