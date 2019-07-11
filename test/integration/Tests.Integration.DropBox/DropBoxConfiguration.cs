using System.Collections.Generic;
using CluedIn.Crawling.DropBox.Core;

namespace Tests.Integration.DropBox
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
