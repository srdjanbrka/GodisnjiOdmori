using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.Security;
using GodisnjiOdmori.MVC.ModeliPrikaza;
using GodisnjiOdmori.Podaci;

namespace GodisnjiOdmori.MVC.Controllers
{
    public class LoginController : Controller
    {
        private string Konekcija { get { return ConfigurationManager.ConnectionStrings["Odmori"].ConnectionString; } }

        [HttpGet]
        public ActionResult Index()
        {
            if(User.Identity.IsAuthenticated) return RedirectToAction("Index","Zahtev");
            return View(new PrijavaModel());
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Index(PrijavaModel model)
        {
            if(!ModelState.IsValid) return View(model);

            Korisnik korisnik;
            try {
                korisnik=new KorisnikRepozitorijum(Konekcija)
                    .DajPoKorisnickomImenu(model.KorisnickoIme.Trim());
            }
            catch(Exception e) when(e is SqlException || e is EntityException)
            {
                ModelState.AddModelError("","Prijava nije dostupna. Proverite bazu i izvršite SQL skriptu.");
                return View(model);
            }

            if(korisnik==null || !korisnik.Aktivan
                || !Poklapa(model.Lozinka,korisnik.LozinkaSalt,korisnik.LozinkaHash))
            {
                ModelState.AddModelError("","Pogrešni podaci za prijavu.");
                return View(model);
            }

            var sada=DateTime.Now;
            string zaposleniId=korisnik.ZaposleniID.HasValue?korisnik.ZaposleniID.Value.ToString():"";
            var tiket=new FormsAuthenticationTicket(1,korisnik.KorisnickoIme,sada,sada.AddMinutes(30),false,
                korisnik.Uloga+"|"+zaposleniId,FormsAuthentication.FormsCookiePath);
            Response.Cookies.Add(new System.Web.HttpCookie(
                FormsAuthentication.FormsCookieName,FormsAuthentication.Encrypt(tiket)) {
                    HttpOnly=true,Secure=Request.IsSecureConnection,SameSite=System.Web.SameSiteMode.Lax
                });
            return RedirectToAction("Index","Zahtev");
        }

        private static bool Poklapa(string lozinka,string saltBase64,string hashBase64)
        {
            byte[] salt,ocekivano;
            try {
                salt=Convert.FromBase64String(saltBase64);
                ocekivano=Convert.FromBase64String(hashBase64);
            }
            catch(FormatException) { return false; }

            byte[] dobijeno;
            using(var k=new Rfc2898DeriveBytes(lozinka,salt,100000,HashAlgorithmName.SHA256))
                dobijeno=k.GetBytes(32);
            if(dobijeno.Length!=ocekivano.Length) return false;
            int razlika=0;
            for(int i=0;i<ocekivano.Length;i++) razlika|=dobijeno[i]^ocekivano[i];
            return razlika==0;
        }

        [HttpPost,ValidateAntiForgeryToken,Authorize]
        public ActionResult Odjava()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}
