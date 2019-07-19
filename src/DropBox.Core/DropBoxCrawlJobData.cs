using System;
using System.Collections.Generic;
using System.Linq;
using CluedIn.Core;
using CluedIn.Core.Crawling;
using CluedIn.Core.Security;
using Newtonsoft.Json.Linq;

namespace CluedIn.Crawling.DropBox.Core
{
    public class DropBoxCrawlJobData : CrawlJobData
    {
        public DropBoxCrawlJobData(IDictionary<string, object> configuration)
        {
            if (configuration == null)
                return;

            IsAuthenticated = true;

            if (configuration.ContainsKey(DropBoxConstants.KeyName.Folders) || configuration.ContainsKey(DropBoxConstants.KeyName.Folders.ToLower()))
            {
                var start = configuration.ContainsKey(DropBoxConstants.KeyName.Folders) ? (JArray)configuration[DropBoxConstants.KeyName.Folders] : (JArray)configuration[DropBoxConstants.KeyName.Folders.ToLower()];

                var ids = new List<string>();
                foreach (JObject element in start)
                {
                    JToken status;
                    if (element.TryGetValue("id", out var token))
                    {
                        if (element.TryGetValue("Status", out status) && status.Value<string>() != null)
                        {
                            if (status.Value<string>() == "ACTIVE")
                            {
                                ids.Add(token.Value<string>());
                            }
                        }
                        else if (element.TryGetValue("Active", out status))
                        {
                            if (status.Value<bool>())
                            {
                                ids.Add(token.Value<string>());
                            }
                        }
                        else
                        {
                            ids.Add(token.Value<string>());
                        }
                    }
                    if (element.TryGetValue("Id", out token))
                    {
                        if (element.TryGetValue("Status", out status) && status.Value<string>() != null)
                        {
                            if (status.Value<string>() == "ACTIVE")
                            {
                                ids.Add(token.Value<string>());
                            }
                        }
                        else if (element.TryGetValue("Active", out status))
                        {
                            if (status.Value<bool>())
                            {
                                ids.Add(token.Value<string>());
                            }
                        }
                        else
                        {
                            ids.Add(token.Value<string>());
                        }
                    }


                }

                Folders = new List<CrawlEntry>();

                Folders.AddRange(ids.Select(s => new CrawlEntry()
                {
                    CrawlOptions = configuration.ContainsKey("crawlOptions")
                        ? ((CrawlOptions)configuration["crawlOptions"])
                        : CrawlOptions.Recursive,
                    EntryPoint = s,
                    IndexingOptions = IndexingOptions.Index,
                    CrawlPriority = configuration.ContainsKey("crawlPriority")
                        ? ((CrawlPriority)configuration["crawlPriority"])
                        : CrawlPriority.Normal
                }));
            };
            
            FileSizeLimit = Constants.MaxFileIndexingFileSize;

            configuration.TryGetValue(DropBoxConstants.KeyName.Accounts, out var accounts);

            configuration.TryGetValue(DropBoxConstants.KeyName.ClientId, out var clientId);

            configuration.TryGetValue(DropBoxConstants.KeyName.ClientSecret, out var clientSecret);

            LastCrawlFinishTime = ReadLastCrawlFinishTime(configuration);

            BaseUri = GetValue<string>(configuration, DropBoxConstants.KeyName.BaseUri);
            AdminMemberId = GetValue<string>(configuration, DropBoxConstants.KeyName.AdminMemberId);
            if (LastestCursors == null)
                LastestCursors = new Dictionary<string, string>();
            else
            {
                LastestCursors = configuration.ContainsKey(DropBoxConstants.KeyName.LastCursor)
                                      ? JsonUtility.Deserialize<IDictionary<string, string>>(configuration["lastCursor"].ToString())
                                      : new Dictionary<string, string>();
            }

            if (clientId != null && clientSecret != null)
            {
                ClientId = clientId.ToString();
                ClientSecret = clientSecret.ToString();
            }

            try
            {
                AgentToken user; //Maybe this should be done in a base class?

                if (accounts is AgentToken token)
                {
                    user = token;
                }
                else
                {
                    var jUser = (JObject)accounts;
                    if (jUser == null)
                    {
                        IsAuthenticated = false;
                        return;
                    }

                    var accessToken = jUser.GetValue("AccessToken").Value<string>();
                    //var refreshToken = jUser.GetValue("RefreshToken").Value<string>();


                    user = new AgentToken() { AccessToken = accessToken, RefreshToken = null, ExpiresIn = null };
                }

                Token = user;

            }
            catch (Exception)
            {
                IsAuthenticated = false;
            }
        }

        /// <summary>Gets or sets the token.</summary>
        /// <value>The token.</value>
        public AgentToken Token { get; set; }

        public IList<CrawlEntry> Folders { get; set; }

        public string BaseUri { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public bool IsAuthenticated { get; set; }

        public long? FileSizeLimit { get; set; }

        public string AdminMemberId { get; set; }
    }
}
