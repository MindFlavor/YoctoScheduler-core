using Owin;
using System.Linq;
using System.Web.Http;

namespace YoctoScheduler.WebAPI
{
    public class Startup
    {
        public static string ConnectionString;

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.EnableCors();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
           
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            // this will screw up curl but PowerShell will still work. JavaScript is to be tested.
            //var listener = (System.Net.HttpListener)appBuilder.Properties["System.Net.HttpListener"];
            //listener.AuthenticationSchemes = System.Net.AuthenticationSchemes.IntegratedWindowsAuthentication;

            appBuilder.UseWebApi(config);
        }
    }
}
