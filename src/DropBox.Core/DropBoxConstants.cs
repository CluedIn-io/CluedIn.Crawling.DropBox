using System;
using System.Collections.Generic;
using CluedIn.Core.Net.Mail;
using CluedIn.Core.Providers;

namespace CluedIn.Crawling.DropBox.Core
{
    public class DropBoxConstants
    {
        public struct KeyName
        {
            public static readonly string ApiKey = nameof(ApiKey);
            public static readonly string BaseUri = nameof(BaseUri);
            public static readonly string AdminMemberId = nameof(AdminMemberId);
            public static readonly string AccessToken = nameof(AccessToken);
            public static readonly string Folders = nameof(Folders);
            public static readonly string LastCursor = nameof(LastCursor);
            public static readonly string Accounts = nameof(Accounts);
            public static readonly string ClientId = "Providers.DropBoxClientId";
            public static readonly string ClientSecret = "Providers.DropBoxClientSecret";
        }

        public const string CodeOrigin = "DropBox";
        public const string ProviderRootCodeValue = "DropBox";
        public const string CrawlerName = "DropBox";
        public const string CrawlerComponentName = "DropBoxCrawler";
        public const string CrawlerDescription = "DropBox is a web and mobile application designed to enable sharing and storage of files.";
        public const string CrawlerDisplayName = "DropBox";
        public const string Uri = "https://www.dropbox.com/home ";
        public const string ApiUri = "https://api.dropboxapi.com/2";    // TODO Does this come from configuration?


        //public const string ClientID = "123456";
        //public const string ClientSecret = "12345";


        public static readonly Guid ProviderId = Guid.Parse("32811664-085F-4551-BCD0-033CC5171179"); 
        public const string ProviderName = "DropBox";         
        public const bool SupportsConfiguration = true;             // TODO: Replace value
        public const bool SupportsWebHooks = false;             // TODO: Replace value
        public const bool SupportsAutomaticWebhookCreation = true;             // TODO: Replace value
        public const bool RequiresAppInstall = false;            // TODO: Replace value
        public const string AppInstallUrl = null;             // TODO: Replace value
        public const string ReAuthEndpoint = null;             // TODO: Replace value
        public const string IconUri = "https://s3-eu-west-1.amazonaws.com/staticcluedin/bitbucket.png"; // TODO: Replace value

        public static readonly ComponentEmailDetails ComponentEmailDetails = new ComponentEmailDetails
        {
            Features = new Dictionary<string, string>
            {
                { "Tracking",        "Expenses and Invoices against customers" },
                { "Intelligence",    "Aggregate types of invoices and expenses against customers and companies." }
            },

            Icon = new Uri(IconUri),
            ProviderName = ProviderName,
            ProviderId = ProviderId,
            Webhooks = SupportsWebHooks
        };

        public static IProviderMetadata CreateProviderMetadata()
        {
            return new ProviderMetadata
            {
                Id = ProviderId,
                ComponentName = CrawlerName,
                Name = ProviderName,
                Type = "Cloud",
                SupportsConfiguration = SupportsConfiguration,
                SupportsWebHooks = SupportsWebHooks,
                SupportsAutomaticWebhookCreation = SupportsAutomaticWebhookCreation,
                RequiresAppInstall = RequiresAppInstall,
                AppInstallUrl = AppInstallUrl,
                ReAuthEndpoint = ReAuthEndpoint,
                ComponentEmailDetails = ComponentEmailDetails
            };
        }
    }
}
