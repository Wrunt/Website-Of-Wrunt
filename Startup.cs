using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WruntsTools.Startup))]
namespace WruntsTools
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
