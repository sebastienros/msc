using System.Collections.Generic;
using NGM.OpenAuthentication.Models;
using NGM.OpenAuthentication.Services;

namespace NGM.OpenAuthentication.ViewModels {
    public class IndexViewModel {
        public bool AutoRegistrationEnabled { get; set; }

        public IEnumerable<ProviderConfiguration> CurrentProviders { get; set; }
    }
}