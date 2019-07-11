using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Core.Crawling;
using CluedIn.Core.Logging;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;

namespace CluedIn.Crawling.DropBox
{
    public class DropBoxCrawler : ICrawlerDataGenerator
    {
        private readonly IDropBoxClientFactory _clientFactory;
        private readonly ILogger _log;
        private static readonly IEnumerable<object> EmptyResult = Enumerable.Empty<object>();


        public DropBoxCrawler(IDropBoxClientFactory clientFactory, ILogger log)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IEnumerable<object> GetData(CrawlJobData jobData)
        {
            try
            {
                return GetDataAsync(jobData).Result;
            }
            catch (AggregateException e)
            {
                _log.Error(e.InnerExceptions.First().Message);
                throw e.InnerExceptions.First();
            }
        }

        public async Task<IEnumerable<object>> GetDataAsync(CrawlJobData jobData)
        {
            if (jobData == null)
            {
                throw new ArgumentNullException(nameof(jobData));
            }

            if (!(jobData is DropBoxCrawlJobData crawlerJobData))
            {
                return EmptyResult;
            }

            var client = _clientFactory.CreateNew(crawlerJobData);

            var data = new List<object>();

            var account = await client.GetAccountInformationAsync();

            //crawl data from provider and yield objects

            //foreach (var folder in client.GetFolders())
            //{
            //    yield return folder;
            //    foreach (var file in client.GetFilesForFolder(folder.Id))
            //    {
            //        yield return file;
            //    }
            //}


            return data;
        }
    }
}
