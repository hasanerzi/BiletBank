using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BiletAtolyesi.Startup))]
namespace BiletAtolyesi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
