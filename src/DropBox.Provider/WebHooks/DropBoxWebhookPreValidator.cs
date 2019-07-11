using CluedIn.Core.Webhooks;
using CluedIn.Crawling.DropBox.Core;

namespace CluedIn.Provider.DropBox.WebHooks
{
    public class Name_WebhookPreValidator : BaseWebhookPrevalidator
    {
        public Name_WebhookPreValidator()
            : base(DropBoxConstants.ProviderId, DropBoxConstants.ProviderName)
        {
        }
    }
}
