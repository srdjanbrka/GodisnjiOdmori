using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.Security;

namespace GodisnjiOdmori.MVC.Controllers
{
    // Dva demonstraciona naloga: zaposleni podnosi zahtev, kadrovska ga obrađuje.
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Index() { return View(); }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(string korisnickoIme,string lozinka)
        {
            if(lozinka==null || lozinka.Length>200) { ModelState.AddModelError("","Pogrešni podaci za prijavu."); return View(); }
            string uloga=null,podaci="";
            if(korisnickoIme==ConfigurationManager.AppSettings["KadrovskaUser"] && Poklapa(lozinka,"KadrovskaSalt","KadrovskaHash"))
                uloga="Kadrovska";
            else if(korisnickoIme==ConfigurationManager.AppSettings["ZaposleniUser"] && Poklapa(lozinka,"ZaposleniSalt","ZaposleniHash")) {
                uloga="Zaposleni"; podaci=ConfigurationManager.AppSettings["ZaposleniID"];
            }
            if(uloga==null) { ModelState.AddModelError("","Pogrešni podaci za prijavu."); return View(); }
            var sada=DateTime.Now;
            var tiket=new FormsAuthenticationTicket(1,korisnickoIme,sada,sada.AddMinutes(30),false,uloga+"|"+podaci,FormsAuthentication.FormsCookiePath);
            Response.Cookies.Add(new System.Web.HttpCookie(FormsAuthentication.FormsCookieName,FormsAuthentication.Encrypt(tiket)) { HttpOnly=true });
            return RedirectToAction("Index","Zahtev");
        }
        private static bool Poklapa(string lozinka,string saltKljuc,string hashKljuc)
        {
            var salt=Convert.FromBase64String(ConfigurationManager.AppSettings[saltKljuc]);
            var ocekivano=Convert.FromBase64String(ConfigurationManager.AppSettings[hashKljuc]);
            byte[] dobijeno;
            using(var k=new Rfc2898DeriveBytes(lozinka,salt,100000,HashAlgorithmName.SHA256)) dobijeno=k.GetBytes(32);
            int razlika=0; for(int i=0;i<ocekivano.Length;i++) razlika|=dobijeno[i]^ocekivano[i];
            return razlika==0;
        }
        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public ActionResult Odjava() { FormsAuthentication.SignOut(); return RedirectToAction("Index"); }
    }
}
