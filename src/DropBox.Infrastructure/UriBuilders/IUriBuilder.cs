using System;
using Dropbox.Api.Files;

namespace CluedIn.Crawling.DropBox.Infrastructure.UriBuilders
{
    /// <summary>The UriBuilder interface.</summary>
    public interface IUriBuilder
    {
        Uri GetUri(Metadata item);
    }
}
