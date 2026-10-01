using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using GodisnjiOdmori.Logika;
using GodisnjiOdmori.MVC.ModeliPrikaza;
using GodisnjiOdmori.Podaci;

namespace GodisnjiOdmori.MVC.Controllers
{
    [Authorize(Roles="Zaposleni,Kadrovska")]
    public class ZahtevController : Controller
    {
        private string Konekcija { get { return ConfigurationManager.ConnectionStrings["Odmori"].ConnectionString; } }
        private UpravljanjeZahtevima Logika { get { return new UpravljanjeZahtevima(Konekcija); } }

        private int UlogovaniZaposleniID
        {
            get {
                var identitet=User.Identity as FormsIdentity;
                var delovi=identitet==null?new string[0]:(identitet.Ticket.UserData??"").Split('|');
                int id;
                if(delovi.Length<2 || !Int32.TryParse(delovi[1],out id))
                    throw new InvalidOperationException("Nalog zaposlenog nije povezan sa zaposlenim u bazi.");
                return id;
            }
        }

        public ActionResult Index(string filter)
        {
            ViewBag.JeZaposleni=User.IsInRole("Zaposleni");
            try {
                string skraceni=(filter??"").Length>100?filter.Substring(0,100):filter;
                using(var kontekst=new GodisnjiOdmoriContext(Konekcija))
                {
                    var zahtevi=new ZahtevRepozitorijum(kontekst).DajSve(skraceni,
                        User.IsInRole("Zaposleni")?(int?)UlogovaniZaposleniID:null);
                    return View(new ZahteviListaModel {
                        Filter=skraceni,
                        Zahtevi=zahtevi.Select(z=>new ZahtevStavkaModel {
                            ZahtevID=z.ZahtevID,ImePrezime=z.ImePrezime,Sektor=z.Sektor,
                            DatumOd=z.DatumOd,DatumDo=z.DatumDo,BrojDana=z.BrojDana,Status=z.Status
                        }).ToList()
                    });
                }
            }
            catch(Exception e) when(e is SqlException || e is EntityException)
            {
                return View("Greska",(object)"Veza sa bazom nije uspela. Proverite SQL server i izvršite Baza/01_Baza.sql ili Baza/03_KorekcijeProfesor.sql.");
            }
        }

        public ActionResult Detalji(int id)
        {
            using(var kontekst=new GodisnjiOdmoriContext(Konekcija))
            {
                var zahtev=new ZahtevRepozitorijum(kontekst).Daj(id);
                if(zahtev==null || (User.IsInRole("Zaposleni") && zahtev.ZaposleniID!=UlogovaniZaposleniID))
                    return HttpNotFound();
                var dokument=PripremaDokumenta.Pripremi(zahtev);
                return View(new ZahtevDetaljiModel {
                    ZahtevID=dokument.ZahtevID,ZaposleniID=dokument.ZaposleniID,
                    ImePrezime=dokument.ImePrezime,Sektor=dokument.Sektor,RadnoMesto=dokument.RadnoMesto,
                    DatumPodnosenja=dokument.DatumPodnosenja,DatumOd=dokument.DatumOd,DatumDo=dokument.DatumDo,
                    Status=dokument.Status,BrojRadnihDana=dokument.BrojRadnihDana,Verzija=dokument.Verzija
                });
            }
        }

        private void PostaviZaposlenog()
        {
            using(var kontekst=new GodisnjiOdmoriContext(Konekcija))
            {
                var zaposleni=new ZaposleniRepozitorijum(kontekst).Daj(UlogovaniZaposleniID);
                ViewBag.ZaposleniIme=zaposleni==null?"Nepoznat zaposleni":zaposleni.ImePrezime+" — "+zaposleni.Sektor;
            }
        }

        [Authorize(Roles="Zaposleni")]
        public ActionResult Dodaj()
        {
            PostaviZaposlenog();
            return View("Forma",new ZahtevFormaModel { DatumOd=DateTime.Today,DatumDo=DateTime.Today });
        }

        [Authorize(Roles="Zaposleni")]
        public ActionResult Izmeni(int id)
        {
            Zahtev zahtev;
            using(var kontekst=new GodisnjiOdmoriContext(Konekcija))
                zahtev=new ZahtevRepozitorijum(kontekst).Daj(id);
            if(zahtev==null || zahtev.ZaposleniID!=UlogovaniZaposleniID) return HttpNotFound();
            if(zahtev.Status=="Odobren")
            {
                TempData["Poruka"]="Odobren zahtev nije moguće izmeniti.";
                return RedirectToAction("Detalji",new{id});
            }
            PostaviZaposlenog();
            return View("Forma",new ZahtevFormaModel {
                ZahtevID=zahtev.ZahtevID,DatumOd=zahtev.DatumOd,DatumDo=zahtev.DatumDo,Verzija=zahtev.Verzija
            });
        }

        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Zaposleni")]
        public ActionResult Sacuvaj(ZahtevFormaModel model)
        {
            if(ModelState.IsValid)
            {
                var zahtev=new Zahtev {
                    ZahtevID=model.ZahtevID,ZaposleniID=UlogovaniZaposleniID,
                    DatumOd=model.DatumOd.Value,DatumDo=model.DatumDo.Value,Verzija=model.Verzija
                };
                try {
                    int id=Logika.Sacuvaj(zahtev,UlogovaniZaposleniID);
                    TempData["Poruka"]="Zahtev je podnet kadrovskoj službi.";
                    return RedirectToAction("Detalji",new{id});
                }
                catch(InvalidOperationException e) { ModelState.AddModelError("",e.Message); }
                catch(Exception e) when(e is SqlException || e is EntityException
                    || e is DbUpdateException || e is DbEntityValidationException)
                {
                    ModelState.AddModelError("","Greška pri radu sa bazom. Promene nisu sačuvane.");
                }
            }
            PostaviZaposlenog();
            return View("Forma",model);
        }

        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Kadrovska")]
        public async Task<ActionResult> Odobri(int id,string verzija)
        {
            try {
                var status=await Logika.Odobri(id,verzija);
                TempData["Poruka"]=status=="Odobren"?"Zahtev je odobren.":
                    "Limit X bi bio prekoračen. Zahtev je automatski postavljen na čekanje.";
            }
            catch(InvalidOperationException e) { TempData["Poruka"]=e.Message; }
            catch(Exception e) when(e is SqlException || e is EntityException || e is DbUpdateException)
            { TempData["Poruka"]="Greška baze. Odobrenje nije sačuvano."; }
            return RedirectToAction("Detalji",new{id});
        }

        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Kadrovska")]
        public ActionResult Odbij(int id,string verzija)
        {
            try { Logika.OdbijIliObrisi(id,verzija,false); TempData["Poruka"]="Zahtev je odbijen."; }
            catch(InvalidOperationException e) { TempData["Poruka"]=e.Message; }
            catch(Exception e) when(e is SqlException || e is EntityException || e is DbUpdateException)
            { TempData["Poruka"]="Greška baze. Promene nisu sačuvane."; }
            return RedirectToAction("Index");
        }

        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Zaposleni")]
        public ActionResult Obrisi(int id,string verzija)
        {
            try {
                Logika.OdbijIliObrisi(id,verzija,true,UlogovaniZaposleniID);
                TempData["Poruka"]="Zahtev je obrisan.";
            }
            catch(InvalidOperationException e) { TempData["Poruka"]=e.Message; }
            catch(Exception e) when(e is SqlException || e is EntityException || e is DbUpdateException)
            { TempData["Poruka"]="Greška baze. Promene nisu sačuvane."; }
            return RedirectToAction("Index");
        }

        [Authorize(Roles="Kadrovska")]
        public ActionResult Zaposleni()
        {
            using(var kontekst=new GodisnjiOdmoriContext(Konekcija))
            {
                var zaposleni=new ZaposleniRepozitorijum(kontekst).DajSve();
                return View(zaposleni.Select(z=>new ZaposleniStavkaModel {
                    ZaposleniID=z.ZaposleniID,ImePrezime=z.ImePrezime,
                    Sektor=z.Sektor,RadnoMesto=z.RadnoMesto
                }).ToList());
            }
        }
    }
}
