using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoctoScheduler.Web
{
    public class Startup
    {
        public static string wwwRoot;

        public void Configuration(IAppBuilder appBuilder)
        {
            //var listener = (System.Net.HttpListener)appBuilder.Properties["System.Net.HttpListener"];
            //listener.AuthenticationSchemes = System.Net.AuthenticationSchemes.IntegratedWindowsAuthentication;

            appBuilder.UseFileServer(new FileServerOptions()
            {
                RequestPath = PathString.Empty,
                FileSystem = new PhysicalFileSystem(wwwRoot)
            });
        }
    }
}
