using System;
using CluedIn.Core;
using Dropbox.Api.Files;

namespace CluedIn.Crawling.DropBox.Infrastructure.UriBuilders
{
    /// <summary>The box file uri builder.</summary>
    public class BoxFileUriBuilder : IUriBuilder
    {
        private readonly Uri _baseUri;
        private readonly char[] _trimChars = { '/' };

        public BoxFileUriBuilder([NotNull] Uri baseUri)
        {
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }

        public Uri GetUri([NotNull] Metadata item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return new Uri(_baseUri.AbsoluteUri.TrimEnd('/') + ReplaceLastOccurrence(item.PathLower, "/", "").Replace(item.Name, "?preview=" + item.Name).Replace(" ", "+"));
        }

        public static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            var place = source.LastIndexOf(find);

            if (place == -1)
            {
                return string.Empty;
            }

            var result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }
    }
}
