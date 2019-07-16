using System.Collections.Generic;
using System.Configuration;
using CluedIn.Core.Configuration;
using CluedIn.Core.Security;
using CluedIn.Crawling.DropBox.Core;

namespace Crawling.DropBox.Integration.Test
{
  public static class DropBoxConfiguration
  {
    public static Dictionary<string, object> Create()
    {
      return new Dictionary<string, object>
            {
                { DropBoxConstants.KeyName.ApiKey, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.ApiKey, "") },
                { DropBoxConstants.KeyName.Accounts, new AgentToken
                {
                    AccessToken = ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.AccessToken, "")
                } },
                { DropBoxConstants.KeyName.BaseUri, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.BaseUri, "") },
                { DropBoxConstants.KeyName.ClientId, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.ClientId, "") },
                { DropBoxConstants.KeyName.ClientSecret, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.ClientSecret, "") },
                { DropBoxConstants.KeyName.LastCursor, new Dictionary<string, string>() },
                { DropBoxConstants.KeyName.AdminMemberId, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.AdminMemberId, "") }

            };
    }
  }
}
