using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CluedIn.Core;
using CluedIn.Core.Crawling;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;
using CluedIn.Core.Webhooks;
using System.Configuration;
using System.Linq;
using CluedIn.Core.Configuration;
using CluedIn.Core.Data;
using CluedIn.Core.DataStore;
using CluedIn.Core.Logging;
using CluedIn.Core.Messages.WebApp;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Core.Models;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;
using CluedIn.Providers.Models;
using CluedIn.Providers.Webhooks.Models;

namespace CluedIn.Provider.DropBox
{
    public class DropBoxProvider : ProviderBase
    {
        private readonly IDropBoxClientFactory _dropboxClientFactory;
        private readonly ISystemNotifications _notifications;
        private readonly ILogger _log;

        public DropBoxProvider([NotNull] ApplicationContext appContext, IDropBoxClientFactory dropboxClientFactory, ILogger log, ISystemNotifications notifications)
            : base(appContext, DropBoxConstants.CreateProviderMetadata())
        {
            _dropboxClientFactory = dropboxClientFactory ?? throw new ArgumentNullException(nameof(dropboxClientFactory));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _notifications = notifications; // ?? throw new ArgumentNullException(nameof(notifications));
        }


        public override async Task<CrawlJobData> GetCrawlJobData(
            ProviderUpdateContext context,
            IDictionary<string, object> configuration,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var dropboxCrawlJobData = new DropBoxCrawlJobData(configuration);
            if (configuration.ContainsKey(DropBoxConstants.KeyName.ClientId))
            { dropboxCrawlJobData.ClientId = configuration[DropBoxConstants.KeyName.ClientId].ToString(); }

            return await Task.FromResult(dropboxCrawlJobData);
        }

        public override async Task<bool> TestAuthentication(
            ProviderUpdateContext context,
            IDictionary<string, object> configuration,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            var dropBoxCrawlJobData = new DropBoxCrawlJobData(configuration);
            try
            {
                if (dropBoxCrawlJobData.Token == null || string.IsNullOrEmpty(dropBoxCrawlJobData.Token.AccessToken))
                    return false;

                var client = _dropboxClientFactory.CreateNew(dropBoxCrawlJobData);

                var usage = await client.GetSpaceUsageAsync();
            }
            catch (Exception exception)
            {
                _log.Warn(() => "Could not add DropBox provider", exception);
                return false;
            }

            return true;
        }

        public override async Task<ExpectedStatistics> FetchUnSyncedEntityStatistics(ExecutionContext context, IDictionary<string, object> configuration, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            return await Task.FromResult(default(ExpectedStatistics));
        }

        public override async Task<IDictionary<string, object>> GetHelperConfiguration(
            ProviderUpdateContext context,
            [NotNull] CrawlJobData jobData,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            return await GetHelperConfiguration(context, jobData, organizationId, userId, providerDefinitionId, "/");
        }

        public override async Task<IDictionary<string, object>> GetHelperConfiguration(
            ProviderUpdateContext context,
            CrawlJobData jobData,
            Guid organizationId,
            Guid userId,
            Guid providerDefinitionId,
            string folderId)
        {
            if (jobData == null)
                throw new ArgumentNullException(nameof(jobData));

            var configuration = new Dictionary<string, object>();

            if (jobData is DropBoxCrawlJobData dropBoxCrawlJobData)
            {
                configuration.Add(DropBoxConstants.KeyName.ClientId, dropBoxCrawlJobData.ClientId);
                configuration.Add(DropBoxConstants.KeyName.ClientSecret, dropBoxCrawlJobData.ClientSecret);

                try
                {
                    if (dropBoxCrawlJobData.Token == null || string.IsNullOrEmpty(dropBoxCrawlJobData.Token.AccessToken))
                        throw new ApplicationException("The crawl data does not contain an access token");

                    _notifications?.Publish(new ProviderMessageCommand() { OrganizationId = organizationId, ProviderDefinitionId = providerDefinitionId, ProviderId = Id, ProviderName = Name, Message = "Authenticating", UserId = userId });
                    var client = _dropboxClientFactory.CreateNew(dropBoxCrawlJobData);

                    _notifications?.Publish(new ProviderMessageCommand() { OrganizationId = organizationId, ProviderDefinitionId = providerDefinitionId, ProviderId = Id, ProviderName = Name, Message = "Fetching Folders", UserId = userId });

                    var full = await client.ListFolderAsync(string.Empty, DropBoxConstants.FetchLimit, false);
                    var folderProjection = full.Entries.Where(i => i.IsFolder).Select(item => new FolderProjection() { Id = item.PathLower, Name = item.Name, Parent = string.IsNullOrEmpty(item.PathLower.Remove(item.PathLower.LastIndexOf('/'))) ? "/" : item.PathLower.Remove(item.PathLower.LastIndexOf('/')), Sensitive = ConfigurationManager.AppSettings["Configuration.Sensitive"] != null && ConfigurationManager.AppSettings["Configuration.Sensitive"].Split(',').Contains(item.Name), Permissions = new List<CluedInPermission>(), Active = true }).ToList();

                    folderProjection.Add(new FolderProjection() { Id = "/", Name = "Root Folder", Parent = (string)null, Sensitive = false, Permissions = (List<CluedInPermission>)null, Active = true });

                    var cursor = full.Cursor;
                    var hasMore = full.HasMore;
                    while (cursor != null && hasMore)
                    {
                        _notifications?.Publish(new ProviderMessageCommand() { OrganizationId = organizationId, ProviderDefinitionId = providerDefinitionId, ProviderId = Id, ProviderName = Name, Message = "Fetching Folders", UserId = userId });

                        var fullWithCursor = await client.ListFolderContinueAsync(cursor);

                        foreach (var fol in fullWithCursor.Entries.Where(i => i.IsFolder))
                        {
                            if (!folderProjection.Select(d => d.Id).Contains(fol.PathLower))
                                folderProjection.Add(new FolderProjection() { Id = fol.PathLower, Name = fol.Name, Parent = string.IsNullOrEmpty(fol.PathLower.Remove(fol.PathLower.LastIndexOf('/'))) ? "/" : fol.PathLower.Remove(fol.PathLower.LastIndexOf('/')), Sensitive = ConfigurationManager.AppSettings["Configuration.Sensitive"] != null && ConfigurationManager.AppSettings["Configuration.Sensitive"].Split(',').Contains(fol.Name), Permissions = new List<CluedInPermission>(), Active = true });
                        }

                        cursor = fullWithCursor.Cursor;
                        hasMore = fullWithCursor.HasMore;
                    }

                    _notifications?.Publish(new ProviderMessageCommand() { OrganizationId = organizationId, ProviderDefinitionId = providerDefinitionId, ProviderId = Id, ProviderName = Name, Message = "Fetching Shared Folders", UserId = userId });

                    var responseFromBox = await client.GetFolderListViaRestAsync();
                    if (responseFromBox != null)
                    {
                        foreach (var sharedFolder in responseFromBox.entries)
                        {
                            var responseFromBox1 = await client.GetFolderPermissions(sharedFolder);
                            if (responseFromBox1 != null)
                            {
                                if (sharedFolder.path_lower != null)
                                    if (!folderProjection.Select(d => d.Id).Contains(sharedFolder.path_lower))
                                        folderProjection.Add(new FolderProjection() { Id = sharedFolder.path_lower, Name = sharedFolder.name, Parent = "/", Sensitive = false, Permissions = responseFromBox1.groups.Select(s => new CluedInPermission() { }).Concat(responseFromBox1.invitees.Select(s => new CluedInPermission() { })).Concat(responseFromBox1.users.Select(s => new CluedInPermission() { })).ToList(), Active = true });
                            }
                        }
                    }

                    //Created,Uploaded,Commented,Downloaded,Previewed,Moved,Copied
                    configuration.Add("webhooks", new List<object>()
                    {
                        new { DisplayName = "Created",      Name = "Created",     Status = "ACTIVE", Description = "When a new file or folder is created."},
                        new { DisplayName = "Uploaded",      Name = "Uploaded",     Status = "ACTIVE", Description = "When a new file or folder is uploaded."},
                        new { DisplayName = "Commented",      Name = "Commented",     Status = "ACTIVE", Description = "When a file or folder is commented upon."},
                        new { DisplayName = "Downloaded",      Name = "Downloaded",     Status = "ACTIVE", Description = "When a file or folder is downloaded."},
                        new { DisplayName = "Previewed",      Name = "Previewed",     Status = "ACTIVE", Description = "When a file is previewed."},
                        new { DisplayName = "Moved",      Name = "Moved",     Status = "ACTIVE", Description = "When a file or folder is moved."},
                        new { DisplayName = "Copied",      Name = "Copied",     Status = "ACTIVE", Description = "When a file or folder is copied."}
                    });

                    configuration.Add("folders", folderProjection);

                    if (configuration.ContainsKey("previousfolderProjectionCount"))
                    {
                        if (int.Parse(configuration["previousfolderProjectionCount"].ToString()) != folderProjection.Count)
                        {
                            configuration["previousfolderProjectionCount"] = folderProjection.Count;
                        }
                    }
                    else
                    {
                        configuration.Add("previousfolderProjectionCount", folderProjection.Count);
                    }

                    //This MUST be called after the previosFolderCount has been assigned
                    CheckForNewConfiguration(context, organizationId, providerDefinitionId, dropBoxCrawlJobData, folderProjection.Count);

                    _notifications?.Publish(new ProviderMessageCommand() { OrganizationId = organizationId, ProviderDefinitionId = providerDefinitionId, ProviderId = Id, ProviderName = Name, Message = "Forecasting projected processing time", UserId = userId });

                    var usage = await client.GetSpaceUsageAsync();
                    configuration.Add("usage", new Usage() { UsedSpace = (long)usage.Used, NumberOfClues = null, TotalSpace = null });

                    dropBoxCrawlJobData.ExpectedStatistics?.EntityTypeStatistics?.Add(new EntityTypeStatistics(EntityType.Files.Directory, folderProjection.Count, 0));
                    configuration.Add("expectedStatistics", dropBoxCrawlJobData.ExpectedStatistics);

                }
                catch (Exception exception)
                {
                    _log.Warn(() => "Could not add DropBox provider", exception);
                    return new Dictionary<string, object>() { { "error", "Could not fetch configuration data from DropBox" } };
                }
            }

            return await Task.FromResult(configuration);
        }

        public override async Task<AccountInformation> GetAccountInformation(ExecutionContext context, [NotNull] CrawlJobData jobData, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            if (jobData == null)
                throw new ArgumentNullException(nameof(jobData));

            var crawlJobData = (DropBoxCrawlJobData)jobData;

            if (crawlJobData == null)
            {
                throw new Exception("Wrong CrawlJobData type");
            }

            var client = _dropboxClientFactory.CreateNew(crawlJobData);
            return await client.GetAccountInformationAsync();
        }

        private void CheckForNewConfiguration(ExecutionContext context, Guid organizationId, Guid providerDefinitionId, CrawlJobData crawlJobData, int projectFoldersCount)
        {
            if (ConfigurationManager.AppSettings.GetFlag("Feature.Configuration.Messages", false))
            {
                if (crawlJobData != null)
                {
                    {
                        var configurationDataStore = appContext.Container.Resolve<IConfigurationRepository>();
                        var configuration = configurationDataStore.GetConfigurationById(context, providerDefinitionId);
                        if (configuration == null)
                            return;

                        var currentlySelectedProjectId = projectFoldersCount;
                        if (configuration.ContainsKey("previousfolderProjectionCount"))
                        {
                            currentlySelectedProjectId = int.Parse(configuration["previousfolderProjectionCount"].ToString());
                        }

                        if (projectFoldersCount != currentlySelectedProjectId)
                        {
                            // You have new or deleted folders.
                            // Just place a notification in their notification centre to add a watch to this folder
                            // Also group them, e.g. you have 2 new folders.
                            try
                            {
                                var notificationDataStore = context.Organization.DataStores.GetDataStore<Notification>();
                                var userProfiles = context.Organization.DataStores.GetDataStore<UserProfile>();
                                foreach (var user in userProfiles.Select(context, i => i.OrganizationId == organizationId))
                                {
                                    var notification1 = $"You have new configuration to set for {Name}";
                                    if (!notificationDataStore.Select(context, n => n.SenderName == user.Email && n.Notification1 == notification1).Any())
                                    {
                                        var properties = new Dictionary<string, string>
                                        {
                                            { "ProviderId", Id.ToString() },
                                            { "NotificationType", "Edit_Provider" },
                                            { "ProviderDefinitionId", providerDefinitionId.ToString() },
                                            { "AccountId", "" },
                                            { "UserId", user.Id.ToString() },
                                            { "Username", user.Email },
                                            { "Configuration", "folders" }
                                        };

                                        notificationDataStore.Insert(context, new Notification()
                                        {
                                            Count = 1,
                                            CreatedDate = DateTime.UtcNow,
                                            Id = Guid.NewGuid(),
                                            Notification1 = notification1,
                                            SenderName = "CluedIn",
                                            Receiver = user.Email,
                                            Type = "Edit",
                                            UrlId = providerDefinitionId.ToString(),
                                            Url = $"https://{context.Organization.ApplicationSubDomain}.{ConfigurationManager.AppSettings.GetValue("Domain", Constants.Configuration.Defaults.Domain)}/admin/integrations/edit/{Id.ToString()}/{providerDefinitionId}",
                                            Properties = properties
                                        });

                                        context.ApplicationContext.System.Notifications.Publish(
                                            new NewConfigurationCommand()
                                            {
                                                OrganizationId = context.Organization.Id.ToString(),
                                                Configuration = "folders",
                                                UserId = user.Id.ToString(),
                                                AccountId = "",
                                                ProviderId = "",
                                                Username = user.Email,
                                                ProviderDefinitionId = providerDefinitionId.ToString()
                                            });
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                _log.Warn(() => "Could not alert users of changed configuration.", exception);
                            }
                        }
                    }
                }
            }
        }


        public override string Schedule(DateTimeOffset relativeDateTime, bool webHooksEnabled)
        {
            return webHooksEnabled && ConfigurationManager.AppSettings.GetFlag("Feature.Webhooks.Enabled", false) ? $"{relativeDateTime.Minute} 0/23 * * *" : $"{relativeDateTime.Minute} 0/4 * * *";
        }

        public override async Task<IEnumerable<WebHookSignature>> CreateWebHook(ExecutionContext context, [NotNull] CrawlJobData jobData, [NotNull] IWebhookDefinition webhookDefinition, [NotNull] IDictionary<string, object> config)
        {
            if (jobData == null)
                throw new ArgumentNullException(nameof(jobData));
            if (webhookDefinition == null)
                throw new ArgumentNullException(nameof(webhookDefinition));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return await Task.Run(() =>
            {
                var webhookSignatures = new List<WebHookSignature>();

                var webhookSignature = new WebHookSignature() { Signature = webhookDefinition.ProviderDefinitionId.ToString(), ExternalVersion = "v1", ExternalId = null, EventTypes = "Created,Uploaded,Commented,Downloaded,Previewed,Moved,Copied" };

                webhookSignatures.Add(webhookSignature);

                webhookDefinition.Uri = new Uri(appContext.System.Configuration.WebhookReturnUrl.Trim('/') + ConfigurationManager.AppSettings["Providers.Dropbox.WebhookEndpoint"]);

                var organizationProviderDataStore = context.Organization.DataStores.GetDataStore<ProviderDefinition>();
                if (organizationProviderDataStore != null)
                {
                    if (webhookDefinition.ProviderDefinitionId != null)
                    {
                        var webhookEnabled = organizationProviderDataStore.GetById(context, webhookDefinition.ProviderDefinitionId.Value);
                        if (webhookEnabled != null)
                        {
                            webhookEnabled.WebHooks = true;
                            organizationProviderDataStore.Update(context, webhookEnabled);
                        }
                    }
                }

                webhookDefinition.Verified = true;

                return webhookSignatures;
            });
        }

        public override async Task<IEnumerable<WebhookDefinition>> GetWebHooks(ExecutionContext context)
        {
            var webhookDefinitionDataStore = context.Organization.DataStores.GetDataStore<WebhookDefinition>();
            return await webhookDefinitionDataStore.SelectAsync(context, s => s.Verified != null && s.Verified.Value);

        }

        public override async Task DeleteWebHook(ExecutionContext context, [NotNull] CrawlJobData jobData, [NotNull] IWebhookDefinition webhookDefinition)
        {
            if (jobData == null)
                throw new ArgumentNullException(nameof(jobData));
            if (webhookDefinition == null)
                throw new ArgumentNullException(nameof(webhookDefinition));

            await Task.Run(() =>
            {
                var organizationProviderDataStore = context.Organization.DataStores.GetDataStore<ProviderDefinition>();
                if (organizationProviderDataStore != null)
                {
                    if (webhookDefinition.ProviderDefinitionId != null)
                    {
                        var webhookEnabled = organizationProviderDataStore.GetById(context, webhookDefinition.ProviderDefinitionId.Value);
                        if (webhookEnabled != null)
                        {
                            webhookEnabled.WebHooks = false;
                            organizationProviderDataStore.Update(context, webhookEnabled);
                        }
                    }
                }

                var webhookDefinitionProviderDataStore = context.Organization.DataStores.GetDataStore<WebhookDefinition>();
                if (webhookDefinitionProviderDataStore != null)
                {
                    var webhook = webhookDefinitionProviderDataStore.GetById(context, webhookDefinition.Id);
                    if (webhook != null)
                    {
                        webhookDefinitionProviderDataStore.Delete(context, webhook);
                    }
                }
            });
        }

        public override IEnumerable<string> WebhookManagementEndpoints([NotNull] IEnumerable<string> ids)
        {
            return new List<string>();
        }

        public override async Task<CrawlLimit> GetRemainingApiAllowance(ExecutionContext context, [NotNull] CrawlJobData jobData, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            //There is no limit set, so you can pull as often and as much as you want.
            return await Task.FromResult(new CrawlLimit(-1, TimeSpan.Zero));
        }


    }
}
