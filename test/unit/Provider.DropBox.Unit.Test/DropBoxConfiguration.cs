using System.Collections.Generic;
using System.Configuration;
using CluedIn.Core.Configuration;
using CluedIn.Crawling.DropBox.Core;

namespace Provider.DropBox.Unit.Test
{
    public static class DropBoxConfiguration
    {
        public static Dictionary<string, object> Create()
        {
            return new Dictionary<string, object>
            {
                { DropBoxConstants.KeyName.ApiKey, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.ApiKey, "") },
                { DropBoxConstants.KeyName.AccessToken, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.AccessToken, "") },
                { DropBoxConstants.KeyName.BaseUri, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.BaseUri, "") },
                { DropBoxConstants.KeyName.ClientId, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.ClientId, "") },
                { DropBoxConstants.KeyName.ClientSecret, ConfigurationManager.AppSettings.GetValue(DropBoxConstants.KeyName.ClientSecret, "") },
                { DropBoxConstants.KeyName.LastCursor, new Dictionary<string, string>() }

                
            };
        }
    }
}
