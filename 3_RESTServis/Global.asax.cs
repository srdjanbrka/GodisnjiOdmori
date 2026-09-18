using System.Web;
using System.Web.Http;

namespace GodisnjiOdmori.Servis
{
    public class WebApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(c=>c.MapHttpAttributeRoutes());
        }
    }
}
