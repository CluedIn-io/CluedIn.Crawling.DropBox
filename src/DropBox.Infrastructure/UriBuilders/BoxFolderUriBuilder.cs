using System;
using System.IO;
using CluedIn.Core;
using Dropbox.Api.Files;

namespace CluedIn.Crawling.DropBox.Infrastructure.UriBuilders
{
    public class BoxFolderUriBuilder : IUriBuilder
    {
        private readonly Uri _baseUri;
        private readonly char[] _trimChars = { '/' };

        public BoxFolderUriBuilder([NotNull] Uri baseUri)
        {
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }

        public Uri GetUri([NotNull] Metadata item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return new UriBuilder(_baseUri)
            {
                Path = Path.Combine(_baseUri.LocalPath, item.PathLower.Trim(_trimChars))
            }.Uri;
        }
    }
}
