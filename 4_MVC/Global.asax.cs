using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace GodisnjiOdmori.MVC
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.MapRoute("Default","{controller}/{action}/{id}",new { controller="Zahtev",action="Index",id=UrlParameter.Optional });
        }
        protected void Application_AuthenticateRequest(object sender,EventArgs e)
        {
            var cookie=Request.Cookies[FormsAuthentication.FormsCookieName];
            if(cookie==null || String.IsNullOrWhiteSpace(cookie.Value)) return;
            try {
                var ticket=FormsAuthentication.Decrypt(cookie.Value);
                if(ticket==null || ticket.Expired) return;
                var delovi=(ticket.UserData??"").Split('|');
                var uloga=delovi.Length>0?delovi[0]:"";
                Context.User=new GenericPrincipal(new FormsIdentity(ticket),new[]{uloga});
            } catch { FormsAuthentication.SignOut(); }
        }
    }
}
