using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using CluedIn.Core;
using CluedIn.Core.Configuration;
using CluedIn.Server.Common.WebApi.OAuth;
using CluedIn.Crawling.DropBox.Core;

namespace CluedIn.Provider.DropBox.WebApi
{
    [Authorize(Roles = "Admin, OrganizationAdmin")]
    [RoutePrefix("api/providers/" + DropBoxConstants.ProviderName)]
    public class DropBoxController : OAuthCluedInApiController
    {
        public DropBoxController([NotNull] DropBoxProviderComponent component) : base(component)
        {
        }


        // GET: Authenticate and Fetch Data
        public async Task<HttpResponseMessage> Get(string authError)
        {
            using (var context = CreateRequestExecutionContext(UserPrincipal))
            {
                if (authError != null)  
                {
                    // Tell the OAuth provider where to redirect to once you have the code.
                    var redirectUri = new Uri(Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/api/" + DropBoxConstants.ProviderName + "oauth");

                    var state = GenerateState(context, UserPrincipal.Identity.UserId, redirectUri.AbsoluteUri, context.Organization.Id);

                    var clientId = ConfigurationManager.AppSettings.GetValue("Providers.DropBoxClientId", "");

                    // Web App needs to take this url and redirect the user to it. This will prompt them for the provider username and password. It will then redirect to the /api/oauth endpoint automatically. (See OAuthController.cs)
                    return await Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, $"https://www.dropbox.com/oauth2/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&state={state}"));
                }

                return await Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, "DropBox Provider Crawled"));

            }
        }
    }
}
