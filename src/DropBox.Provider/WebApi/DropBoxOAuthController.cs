using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using CluedIn.Core;
using CluedIn.Core.Accounts;
using CluedIn.Core.Configuration;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.DataStore;
using CluedIn.Core.Logging;
using CluedIn.Server.Common.WebApi.OAuth;
using CluedIn.Crawling.DropBox.Core;
using Dropbox.Api;

namespace CluedIn.Provider.DropBox.WebApi
{
    [RoutePrefix("api/" + DropBoxConstants.ProviderName + "oauth")]
    public class DropBoxOAuthController : OAuthCluedInApiController, IOAuth2Authentication
    {
        private readonly ILogger _log;
        private readonly IRelationalDataStore<Token> _tokenStore;
        private const string NoStateFoundInCluedin = "No state found in CluedIn";
        private const string NoStateWasReturnedFromAuthenticator = "No state was returned from authenticator";


        public DropBoxOAuthController([NotNull] DropBoxProviderComponent component, ILogger log, IRelationalDataStore<Token> tokenStore)
        : base(component)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
                 
            _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));;
        }

        // This method will be invoked as a call-back from an authentication service (e.g., https://login.windows.net/).
        // It is not intended to be called directly, only as a redirect from the authorization request.
        // On completion, the method will cache the refresh token and access tokens, and redirect to the URL
        // specified in the state parameter.
        [HttpGet]
        public async Task<HttpResponseMessage> Get(string code, string state)
        {
            using (var system = CreateRequestSystemExecutionContext())
            {
                // NOTE: In production, OAuth must be done over a secure HTTPS connection.
                if (Request.RequestUri.Scheme != "https" && !Request.RequestUri.IsLoopback)
                {
                    return await Task.FromResult(Get("Endpoint is not HTTPS"));
                }

                // Ensure there is a state value on the response.  If there is none, stop OAuth processing and display an error.
                if (state == null)
                {
                    return await Task.FromResult(Get(NoStateWasReturnedFromAuthenticator));
                }

                if (code == null)
                {
                    return await Task.FromResult(Get("No code was returned from authenticator"));
                }

                var clientId = ConfigurationManager.AppSettings.GetValue("Providers.DropBoxClientId", (string)null);
                var clientSecret = ConfigurationManager.AppSettings.GetValue("Providers.DropBoxClientSecret", (string)null);

                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                {
                    throw new Exception("Could not get DropBox ClientId or ClientSecret");
                }

                var stateObject = ValidateState(system, state);

                if (stateObject == null)
                {
                    _log.Error(new { code, state }, () => NoStateFoundInCluedin);
                    return await Task.FromResult(Get(NoStateFoundInCluedin));
                }

                var authenticationFlow = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, clientId, clientSecret, stateObject.RedirectUrl);

                var accessToken = authenticationFlow.AccessToken;

                using (var context = CreateRequestExecutionContext(stateObject.OrganizationId))
                {
                    DeleteExistingTokenIfNotAssociatedWithDropBox(context, DropBoxConstants.ProviderId, stateObject);

                    string accountId;
                    try
                    {
                        accountId = await GetDropBoxAccountId(accessToken);
                    }
                    catch (Exception exception)
                    {
                        _log.Warn(() => "Could not add DropBox provider", exception);
                        return Get("Could not access token store");
                    }

                    CheckForExistingToken(context, DropBoxConstants.ProviderId, accountId, stateObject);

                    var reAuthenticated = CheckForReAuthentication(context, DropBoxConstants.ProviderId, accountId, stateObject, accessToken, null);

                    // Return to the originating page where the user triggered the sign-in
                    var response = CreateResponse(context, reAuthenticated);

                    return response;
                }
            }

        }

        private HttpResponseMessage CreateResponse(ExecutionContext context, bool reAuthenticated)
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);

            if (ConfigurationManager.AppSettings.GetFlag("NewRedirectUrl", false))
            {
                response.Content = new StringContent($"<script>window.location = \"https://{context.Organization.ApplicationSubDomain}.{ConfigurationManager.AppSettings.GetValue("Domain", Constants.Configuration.Defaults.Domain)}/admin/integrations/callback/dropbox\";</script>", Encoding.UTF8, "text/html");
                if (reAuthenticated)
                {
                    response.Content = new StringContent($"<script>window.location = \"https://{context.Organization.ApplicationSubDomain}.{ConfigurationManager.AppSettings.GetValue("Domain", Constants.Configuration.Defaults.Domain)}/\";</script>", Encoding.UTF8, "text/html");
                    return response;
                }
            }
            else
            {
                response.Content = new StringContent($"<script>window.location = \"https://{context.Organization.ApplicationSubDomain}.{ConfigurationManager.AppSettings.GetValue("Domain", Constants.Configuration.Defaults.Domain)}/admin/#/administration/integration/dropbox/callback\";</script>", Encoding.UTF8, "text/html");
                if (reAuthenticated)
                {
                    response.Content = new StringContent($"<script>window.location = \"https://{context.Organization.ApplicationSubDomain}.{ConfigurationManager.AppSettings.GetValue("Domain", Constants.Configuration.Defaults.Domain)}/\";</script>", Encoding.UTF8, "text/html");
                    return response;
                }
            }

            return response;
        }

        private async Task<string> GetDropBoxAccountId(string accessToken)
        {
            string accountId;
            var client = GetDropBoxClient(accessToken);
            var result = await client.Users.GetCurrentAccountAsync();
            accountId = result.AccountId;
            return accountId;
        }

        private void DeleteExistingTokenIfNotAssociatedWithDropBox(ExecutionContext context, Guid dropBoxProviderId, StateObject stateObject)
        {
            var tokenCheck = _tokenStore.Select(context, t => t.ProviderId == dropBoxProviderId && t.AccountId == null && t.UserId == stateObject.UserId).ToList();

            if (tokenCheck.Any())
            {
                _log.Info(() => "Deleting Token that is not associated with a Provider for DropBox.");
                _tokenStore.Delete(context, tokenCheck);
            }
        }

        private bool CheckForReAuthentication(ExecutionContext context, Guid dropBoxProviderId, string accountId, StateObject stateObject, string accessToken, int? expires)
        {
            var reAuthenticated = false;
            //Check if you are just needing to re-authenticate
            var reAuthenticateCheck = _tokenStore.Select(context, t => t.ProviderId == dropBoxProviderId && t.AccountId == accountId && t.UserId == stateObject.UserId).ToList();

            if (reAuthenticateCheck.Any())
            {
                _log.Info(() => "ReAuthentication Token for DropBox.");
                var oldToken = _tokenStore.GetById(context, reAuthenticateCheck.First().Id);
                oldToken.AccessToken = accessToken;
                oldToken.RefreshToken = null;
                oldToken.ExpiresIn = expires;
                oldToken.AccessTokenCreationDate = DateTimeOffset.UtcNow;
                _tokenStore.Update(context, oldToken);
                reAuthenticated = true;
                if (oldToken.ConfigurationId.HasValue)
                {
                    try
                    {
                        context.Organization.Providers.ClearProviderDefinitionAuthenticationError(context, oldToken.ConfigurationId.Value);
                    }
                    catch (NotFoundException exception)
                    {
                        _log.Warn(() => "Could not clear provider definition authentication error.", exception);
                    }
                }
            }
            else
            {
                _tokenStore.Insert(context, new Token(context)
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = stateObject.OrganizationId,
                    UserId = stateObject.UserId,
                    AccessToken = accessToken,
                    RefreshToken = null,
                    ExpiresIn = expires,
                    ProviderId = dropBoxProviderId,
                    AccessTokenCreationDate = DateTimeOffset.UtcNow,
                    AccountId = accountId,
                    ConfigurationId = null
                });

                _log.Info(() => "Inserted Token for DropBox.");
            }

            return reAuthenticated;
        }

        private void CheckForExistingToken(ExecutionContext context, Guid dropBoxProviderId, string accountId, StateObject stateObject)
        {
//Have you bailed out of a provider where it has been hooked up to a provider but it is not yet enabled.
            var tokens = _tokenStore.Select(context, t => t.ProviderId == dropBoxProviderId && t.AccountId == accountId && t.UserId == stateObject.UserId && t.ConfigurationId != null).ToList();

            var providerEnabledCheck = tokens.FirstOrDefault();
            if (providerEnabledCheck != null)
            {
                if (providerEnabledCheck.ConfigurationId != null)
                {
                    var organizationProviderDataStore = context.Organization.DataStores.GetDataStore<ProviderDefinition>();
                    var organizationProvider = context.Organization.Providers.GetProviderDefinition(context, providerEnabledCheck.ConfigurationId.Value);
                    if (organizationProvider != null)
                    {
                        if (organizationProvider.Approved == false)
                        {
                            if (tokens.Any())
                            {
                                //Delete the connected token
                                _tokenStore.Delete(context, tokens);
                                //Delete the thing the token is connected to
                                organizationProviderDataStore.Delete(context, organizationProvider);
                            }
                        }
                    }
                    else
                    {
                        _log.Info(() => "No Organization Provider found for DropBox.");
                    }
                }
                else
                {
                    _log.Info(() => "Organization Provider for DropBox Configuration Id is null.");
                }
            }
        }

        /// <summary>Gets the specified error.</summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public HttpResponseMessage Get([NotNull] string error)   
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            var response = Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            using (var context = CreateRequestSystemExecutionContext())
            {
                _log.Warn(() => $"DropBox received an error on OAuth : {error}");
            }

            //I am not passed the state back, so I have nowhere to know where I cam from, hence the redirection to app.cluedin.net
            response.Content = new StringContent(
                $"<script>window.location = \"https://app.{ConfigurationManager.AppSettings.GetValue("Domain", Constants.Configuration.Defaults.Domain)}/admin/#/administration/integration/error/dropbox\";</script>", Encoding.UTF8, "text/html");

            // Ensure there is a state value on the response.  If there is none, stop OAuth processing and display an error.
            return response;
        }

        /// <summary>Gets the drop box client.</summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        protected DropboxClient GetDropBoxClient(string accessToken)
        {
            return new DropboxClient(accessToken);
        }
    }
}
