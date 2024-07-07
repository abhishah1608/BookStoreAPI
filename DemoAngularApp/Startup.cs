using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Stripe;

[assembly: OwinStartup(typeof(DemoAngularApp.Startup))]

namespace DemoAngularApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["secretKey"]?.ToString();

            app.MapSignalR();
            
        }
    }
}
