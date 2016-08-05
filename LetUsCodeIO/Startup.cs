using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LetUsCodeIO.Startup))]
namespace LetUsCodeIO
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
