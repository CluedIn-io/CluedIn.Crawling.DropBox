using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Facilities.TypedFactory;

using CluedIn.Core;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;
using CluedIn.Crawling.DropBox.Infrastructure.Indexing;
using CluedIn.Crawling.DropBox.Infrastructure.UriBuilders;
using RestSharp;

namespace CluedIn.Crawling.DropBox.Infrastructure.Installers
{
    public class InstallComponents : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .AddFacilityIfNotExists<TypedFactoryFacility>()
                .Register(Component.For<IDropBoxClientFactory>().AsFactory())
                .Register(Component.For<IFileIndexer, FileIndexer>())
                .Register(Component.For<BoxFolderUriBuilder>().Instance(new BoxFolderUriBuilder(new Uri(DropBoxConstants.ApiUri))))
                .Register(Component.For<BoxFileUriBuilder>().Instance(new BoxFileUriBuilder(new Uri(DropBoxConstants.ApiUri))))
                .Register(Component.For<IDropBoxClient, DropBoxClient>().LifestyleTransient())
                .Register(Component.For<ISystemNotifications, SystemNotifications>());

            
            if (!container.Kernel.HasComponent(typeof(IRestClient)) && !container.Kernel.HasComponent(typeof(RestClient)))
            {
                container.Register(Component.For<IRestClient, RestClient>());
            }

            if (!container.Kernel.HasComponent(typeof(ISystemVocabularies)) && !container.Kernel.HasComponent(typeof(SystemVocabularies)))
            {
                container.Register(Component.For<ISystemVocabularies, SystemVocabularies>());
            }
        }
    }
}
