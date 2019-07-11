using CluedIn.Core;
using CluedIn.Crawling.DropBox.Core;

using ComponentHost;

namespace CluedIn.Crawling.DropBox
{
    [Component(DropBoxConstants.CrawlerComponentName, "Crawlers", ComponentType.Service, Components.Server, Components.ContentExtractors, Isolation = ComponentIsolation.NotIsolated)]
    public class DropBoxCrawlerComponent : CrawlerComponentBase
    {
        public DropBoxCrawlerComponent([NotNull] ComponentInfo componentInfo)
            : base(componentInfo)
        {
        }
    }
}

