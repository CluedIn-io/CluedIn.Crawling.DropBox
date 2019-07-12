using System.Collections.Generic;
using CluedIn.Crawling.DropBox.Core;

namespace Crawling.DropBox.Integration.Test
{
  public static class DropBoxConfiguration
  {
    public static Dictionary<string, object> Create()
    {
      return new Dictionary<string, object>
            {
                { DropBoxConstants.KeyName.ApiKey, "demo" }
            };
    }
  }
}
