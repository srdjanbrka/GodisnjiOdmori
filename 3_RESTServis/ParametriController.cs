using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml.Linq;

namespace GodisnjiOdmori.Servis
{
    public class ParametriController : ApiController
    {
        [HttpGet, Route(""), Route("api/parametri")]
        public HttpResponseMessage DajParametre()
        {
            try {
                var xml=XDocument.Load(HostingEnvironment.MapPath("~/App_Data/Parametri.xml"));
                int x=int.Parse(xml.Root.Element("MaksimalnoOdsutnih").Value);
                if(x<1 || x>10000) throw new FormatException();
                var odgovor=new HttpResponseMessage(HttpStatusCode.OK) {
                    Content=new StringContent(new XElement("Parametri",new XElement("MaksimalnoOdsutnih",x)).ToString(),Encoding.UTF8,"application/xml")
                };
                odgovor.Headers.CacheControl=new System.Net.Http.Headers.CacheControlHeaderValue { NoStore=true };
                return odgovor;
            } catch(Exception) {
                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,"Nije moguće učitati ispravne parametre iz XML fajla.");
            }
        }
    }
}
