using System;
using System.Linq;
using System.Xml.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Navigation;
using NGM.OpenAuthentication.Services;

namespace NGM.OpenAuthentication.Commands {
    public class OpenAuthenticationCommands : DefaultOrchardCommandHandler {
        private readonly IUserProviderServices _userProviderServices;
        private readonly IMembershipService _membershipService;
        private readonly IProviderConfigurationService _providerConfigurationService;
        public OpenAuthenticationCommands(
            IUserProviderServices userProviderServices,
            IMembershipService membershipService,
            IProviderConfigurationService providerConfigurationService
            ) {
            _userProviderServices = userProviderServices;
            _membershipService = membershipService;
            _providerConfigurationService = providerConfigurationService;
        }

        [OrchardSwitch]
        public string User { get; set; }

        [OrchardSwitch]
        public string Provider { get; set; }

        [OrchardSwitch]
        public string Value { get; set; }

        [OrchardSwitch]
        public string Secret { get; set; }

        [OrchardSwitch]
        public string Key { get; set; }

        [OrchardSwitch]
        public string Identifier { get; set; }

        [OrchardSwitch]
        public string DisplayName { get; set; }
        
        [CommandName("user associate")]
        [CommandHelp("user associate /User:<username> /Provider:<provider> /Value:<value>\r\n\t" + "Associates a user with an external authentication provider.")]
        [OrchardSwitches("User,Provider,Value")]
        public void Associate() {
            var user = _membershipService.GetUser(User);

            if (user == null) {
                Context.Output.WriteLine(T("Invalid username: {0}", User));
                return;
            }

            if (String.IsNullOrWhiteSpace(Provider)) {
                Context.Output.WriteLine(T("Provider can't be empty"));
                return;
            }

            if (String.IsNullOrWhiteSpace(Value)) {
                Context.Output.WriteLine(T("Value can't be empty"));
                return;
            }

            _userProviderServices.Create(Provider, Value, user);

            Context.Output.WriteLine(T("Account associated successfully"));
        }

        [CommandName("openauth delete")]
        [CommandHelp("openauth delete /Provider:<provider>\r\n\t" + "Delete a named OpenAuth provider.")]
        [OrchardSwitches("Provider")]
        public void Delete() {

            if (String.IsNullOrWhiteSpace(Provider)) {
                Context.Output.WriteLine(T("Provider can't be empty"));
                return;
            }

            var provider = _providerConfigurationService.Get(Provider);

            if (provider == null) {
                Context.Output.WriteLine(T("Provider not found: {0}", Provider));
                return;
            }

            _providerConfigurationService.Delete(provider.Id);

            Context.Output.WriteLine(T("Provider deleted successfully"));
        }


        [CommandName("openauth set")]
        [CommandHelp("openauth set /Provider:<provider> /DisplayName:<display name> /Key:<key> /Secret:<secret> [/Identifier:<identifier>]\r\n\t" + "Creates a named OpenAuth provider.")]
        [OrchardSwitches("Provider,DisplayName,Key,Secret,Identifier")]
        public void Create() {

            if (String.IsNullOrWhiteSpace(Provider)) {
                Context.Output.WriteLine(T("Provider can't be empty"));
                return;
            }

            if (String.IsNullOrWhiteSpace(Key)) {
                Context.Output.WriteLine(T("Key can't be empty"));
                return;
            }

            if (String.IsNullOrWhiteSpace(Secret)) {
                Context.Output.WriteLine(T("Secret can't be empty"));
                return;
            }

            if (String.IsNullOrWhiteSpace(DisplayName)) {
                Context.Output.WriteLine(T("DisplayName can't be empty"));
                return;
            }

            var provider = _providerConfigurationService.Get(Provider);

            // delete any existing provider with the same name
            if (provider != null) {
                _providerConfigurationService.Delete(provider.Id);
                Context.Output.WriteLine(T("A provider with the same name already exists, deleting it before continuing."));
            }

            _providerConfigurationService.Create(new ProviderConfiguration {
                DisplayName = DisplayName,
                ProviderIdentifier = Identifier,
                ProviderIdKey = Key,
                ProviderName = Provider,
                ProviderSecret = Secret
            });

            Context.Output.WriteLine(T("Provider created successfully"));
        }
    }
}