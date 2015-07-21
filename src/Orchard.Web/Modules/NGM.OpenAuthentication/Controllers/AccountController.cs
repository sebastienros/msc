using System.Transactions;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.AspNet;
using NGM.OpenAuthentication.Extensions;
using NGM.OpenAuthentication.Security;
using NGM.OpenAuthentication.Services;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using System;
using System.Configuration;

namespace NGM.OpenAuthentication.Controllers {
    [Themed]
    public class AccountController : Controller {
        private readonly INotifier _notifier;
        private readonly IOrchardOpenAuthWebSecurity _orchardOpenAuthWebSecurity;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOpenAuthMembershipServices _openAuthMembershipServices;
        private readonly IOpenAuthSecurityManagerWrapper _securityManagerWrapper;

        public AccountController(
            INotifier notifier,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IAuthenticationService authenticationService,
            IOpenAuthMembershipServices openAuthMembershipServices,
            IOpenAuthSecurityManagerWrapper securityManagerWrapper) {
            _notifier = notifier;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _authenticationService = authenticationService;
            _openAuthMembershipServices = openAuthMembershipServices;
            _securityManagerWrapper = securityManagerWrapper;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        [AlwaysAccessible]
        public ActionResult SingleSignOn(string returnUrl)
        {
            // copy all query parameters to pass it to the returnUrl
            foreach (string query in Request.QueryString.Keys)
            {
                if (String.Equals("returnUrl", query, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                returnUrl += "&" + query + "=" + HttpUtility.UrlEncode(Request.QueryString[query]);
            }

            return this.RedirectLocal(returnUrl);
        }

        private string CreateSingleSignOnUrl(string returnUrl)
        {
            var ssoRedirect = ConfigurationManager.AppSettings["ssoRedirect"];
            var originUrl = Url.OpenAuthLogOn(returnUrl);
            if (!String.IsNullOrWhiteSpace(ssoRedirect))
            {
                var singleSignOn = ssoRedirect.TrimEnd('/') +"/SingleSignOn?returnUrl=" + HttpUtility.UrlEncode(originUrl);
                return singleSignOn;
            }

            return originUrl;
        }

        [HttpPost]
        [AlwaysAccessible]
        public ActionResult ExternalLogOn(string providerName, string returnUrl) {
            var singleSignOn = CreateSingleSignOnUrl(returnUrl);
            return new OpenAuthLoginResult(_securityManagerWrapper, providerName, singleSignOn);
        }

        [AlwaysAccessible]
        public ActionResult ExternalLogOn(string returnUrl) {
            var singleSignOn = CreateSingleSignOnUrl(returnUrl);

            AuthenticationResult result = _orchardOpenAuthWebSecurity.VerifyAuthentication(singleSignOn);

            if (!result.IsSuccessful) {
                _notifier.Error(T("Your authentication request failed."));

                return new RedirectResult(Url.LogOn(returnUrl));
            }

            if (_orchardOpenAuthWebSecurity.Login(result.Provider, result.ProviderUserId)) {
                _notifier.Information(T("You have been logged using your {0} account.", result.Provider));

                return this.RedirectLocal(returnUrl);
            }

            var authenticatedUser = _authenticationService.GetAuthenticatedUser();

            if (authenticatedUser != null) {
                // If the current user is logged in add the new account
                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId,
                                                                  authenticatedUser);

                _notifier.Information(T("Your {0} account has been attached to your local account.", result.Provider));

                return this.RedirectLocal(returnUrl);
            }

            if (_openAuthMembershipServices.CanRegister()) {
                var newUser =
                    _openAuthMembershipServices.CreateUser(new OpenAuthCreateUserParams(result.UserName, 
                                                                                        result.Provider,
                                                                                        result.ProviderUserId,
                                                                                        result.ExtraData));

                _notifier.Information(
                    T("You have been logged in using your {0} account. We have created a local account for you with the name '{1}'", result.Provider, newUser.UserName));

                _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(result.Provider,
                                                                  result.ProviderUserId,
                                                                  newUser);

                _authenticationService.SignIn(newUser, false);

                return this.RedirectLocal(returnUrl);
            }

            string loginData = _orchardOpenAuthWebSecurity.SerializeProviderUserId(result.Provider,
                                                                                   result.ProviderUserId);

            ViewBag.ProviderDisplayName = _orchardOpenAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;

            return new RedirectResult(Url.LogOn(returnUrl, result.UserName, HttpUtility.UrlEncode(loginData)));
        }
    }

    internal class OpenAuthLoginResult : ActionResult {
        private readonly string _providerName;
        private readonly string _returnUrl;
        private readonly IOpenAuthSecurityManagerWrapper _securityManagerWrapper;

        public OpenAuthLoginResult(IOpenAuthSecurityManagerWrapper securityManagerWrapper, string providerName, string returnUrl)
        {
            _securityManagerWrapper = securityManagerWrapper;
            _providerName = providerName;
            _returnUrl = returnUrl;
        }

        public override void ExecuteResult(ControllerContext context) {
            _securityManagerWrapper.RequestAuthentication(_providerName, _returnUrl);
        }
    }
}