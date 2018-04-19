using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Budgeter3.Startup))]
namespace Budgeter3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
