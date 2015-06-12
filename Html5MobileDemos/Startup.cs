using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Html5MobileDemos.Startup))]
namespace Html5MobileDemos
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}
