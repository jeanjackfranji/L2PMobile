using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MobileL2P.Startup))]
namespace MobileL2P
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
