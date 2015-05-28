using Cygnus.Managers;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Cygnus.Startup))]
namespace Cygnus
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // TODO: wrap these in some other management class or use dependency injection
            GatewayResourceProxy.Instance.RegisterAllResources();
            NlpDecisionEngine.Instance.Initialize();
        }
    }
}
