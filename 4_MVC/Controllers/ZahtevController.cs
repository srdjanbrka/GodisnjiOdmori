using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;
using GodisnjiOdmori.Podaci;
using GodisnjiOdmori.Logika;

namespace GodisnjiOdmori.MVC.Controllers
{
    [Authorize(Roles="Zaposleni,Kadrovska")]
    public class ZahtevController : Controller
    {
        private string Konekcija { get { return ConfigurationManager.ConnectionStrings["Odmori"].ConnectionString; } }
        private UpravljanjeZahtevima Logika { get { return new UpravljanjeZahtevima(Konekcija); } }
        private int UlogovaniZaposleniID { get { return Int32.Parse(ConfigurationManager.AppSettings["ZaposleniID"]); } }
        public ActionResult Index(string filter)
        {
            ViewBag.Filter=filter;
            ViewBag.JeZaposleni=User.IsInRole("Zaposleni");
            try { using(var r=new Repozitorijum(Konekcija)) return View(r.Zahtevi((filter??"").Length>100?filter.Substring(0,100):filter,User.IsInRole("Zaposleni")?(int?)UlogovaniZaposleniID:null)); }
            catch(SqlException) { return View("Greska",(object)"Veza sa bazom nije uspela. Proverite SQL server, connection string i izvršite Baza/01_Baza.sql."); }
        }
        public ActionResult Detalji(int id)
        { using(var r=new Repozitorijum(Konekcija)) { var z=r.Daj(id); if(z==null || (User.IsInRole("Zaposleni") && z.ZaposleniID!=UlogovaniZaposleniID)) return HttpNotFound(); return View(z); } }
        private void PostaviZaposlenog()
        {
            using(var r=new Repozitorijum(Konekcija)) {
                var zaposleni=System.Linq.Enumerable.SingleOrDefault(r.Zaposleni(),z=>z.ZaposleniID==UlogovaniZaposleniID);
                ViewBag.ZaposleniIme=zaposleni==null?"Nepoznat zaposleni":zaposleni.ImePrezime+" — "+zaposleni.Sektor;
            }
        }
        [Authorize(Roles="Zaposleni")]
        public ActionResult Dodaj()
        {
            PostaviZaposlenog();
            return View("Forma",new Zahtev { ZaposleniID=UlogovaniZaposleniID,DatumOd=DateTime.Today,DatumDo=DateTime.Today });
        }
        [Authorize(Roles="Zaposleni")]
        public ActionResult Izmeni(int id)
        {
            Zahtev z;
            using(var r=new Repozitorijum(Konekcija)) z=r.Daj(id);
            if(z==null || z.ZaposleniID!=UlogovaniZaposleniID) return HttpNotFound();
            if(z.Status=="Odobren") { TempData["Poruka"]="Odobren zahtev nije moguće izmeniti.";return RedirectToAction("Detalji",new{id}); }
            PostaviZaposlenog(); return View("Forma",z);
        }
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Zaposleni")]
        public ActionResult Sacuvaj([Bind(Include="ZahtevID,DatumOd,DatumDo,Napomena,Verzija")] Zahtev z)
        {
            z.ZaposleniID=UlogovaniZaposleniID;
            ModelState.Remove("ZaposleniID");
            if(ModelState.IsValid) {
                try { int id=Logika.Sacuvaj(z,UlogovaniZaposleniID); TempData["Poruka"]="Zahtev je podnet kadrovskoj službi.";return RedirectToAction("Detalji",new{id}); }
                catch(InvalidOperationException e) { ModelState.AddModelError("",e.Message); }
                catch(SqlException) { ModelState.AddModelError("","Greška pri radu sa bazom. Promene nisu sačuvane."); }
            }
            PostaviZaposlenog(); return View("Forma",z);
        }
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Kadrovska")]
        public async Task<ActionResult> Odobri(int id,string verzija)
        {
            try { var status=await Logika.Odobri(id,verzija);TempData["Poruka"]=status=="Odobren"?"Zahtev je odobren.":"Limit X bi bio prekoračen. Zahtev je automatski postavljen na čekanje."; }
            catch(InvalidOperationException e) { TempData["Poruka"]=e.Message; }
            catch(SqlException) { TempData["Poruka"]="Greška baze. Odobrenje nije sačuvano."; }
            return RedirectToAction("Detalji",new{id});
        }
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Kadrovska")]
        public ActionResult Odbij(int id,string verzija)
        {
            try { Logika.OdbijIliObrisi(id,verzija,false); TempData["Poruka"]="Zahtev je odbijen."; }
            catch(InvalidOperationException e) { TempData["Poruka"]=e.Message; }
            catch(SqlException) { TempData["Poruka"]="Greška baze. Promene nisu sačuvane."; }
            return RedirectToAction("Index");
        }
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Zaposleni")]
        public ActionResult Obrisi(int id,string verzija)
        {
            try { Logika.OdbijIliObrisi(id,verzija,true,UlogovaniZaposleniID); TempData["Poruka"]="Zahtev je obrisan."; }
            catch(InvalidOperationException e) { TempData["Poruka"]=e.Message; }
            catch(SqlException) { TempData["Poruka"]="Greška baze. Promene nisu sačuvane."; }
            return RedirectToAction("Index");
        }
        [Authorize(Roles="Kadrovska")]
        public ActionResult Zaposleni()
        { using(var r=new Repozitorijum(Konekcija)) return View(r.Zaposleni()); }
    }
}
