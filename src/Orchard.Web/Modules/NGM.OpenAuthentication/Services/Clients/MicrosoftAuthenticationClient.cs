using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using NGM.OpenAuthentication.Models;

namespace NGM.OpenAuthentication.Services.Clients {
    public class MicrosoftAuthenticationClient : IExternalAuthenticationClient {
        public string ProviderName {
            get { return "Microsoft"; }
        }

        public IAuthenticationClient Build(ProviderConfiguration providerConfigurationRecord) {
            return new MicrosoftClient(providerConfigurationRecord.ProviderIdKey, providerConfigurationRecord.ProviderSecret, "wl.signin" );
        }
    }
}