using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Agent.Jobs;
using CluedIn.Core.Configuration;
using CluedIn.Core.Crawling;
using CluedIn.Core.Logging;
using CluedIn.Crawling.DropBox.Core;
using CluedIn.Crawling.DropBox.Infrastructure;
using CluedIn.Crawling.DropBox.Infrastructure.Factories;
using Dropbox.Api.Files;

namespace CluedIn.Crawling.DropBox
{
    public class DropBoxCrawler : ICrawlerDataGenerator
    {
        private readonly IDropBoxClientFactory _clientFactory;
        private readonly ILogger _log;

        private static readonly IEnumerable<object> EmptyResult = Enumerable.Empty<object>();
        private readonly IAgentJobProcessorState<CrawlJobData> _state;

        public DropBoxCrawler(IDropBoxClientFactory clientFactory, ILogger log, IAgentJobProcessorState<CrawlJobData> state)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _state = state;
        }

        public IEnumerable<object> GetData(CrawlJobData jobData)
        {
            try
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

                var account = client.GetCurrentAccountAsync().Result;
                if (account == null)
                {
                    _log.Error("Settings could not be obtained from DropBox");
                    return EmptyResult;
                }

                var list = new List<object> { account };

                GetFolderItems(CrawlOptions.Recursive, client, crawlerJobData, list);
                GetSharedFolders(client, list);

                // Index files

                // Get thumbnails

                return list;
            }
            catch (AggregateException e)
            {
                _log.Error(e.InnerExceptions.First().Message);
                throw e.InnerExceptions.First();
            }
        }


        private void GetFolderItems(CrawlOptions options, IDropBoxClient client, DropBoxCrawlJobData jobData, IList<object> list)
        {
            try
            {
                // Files & Folders
                if (jobData.LastCrawlFinishTime > DateTimeOffset.MinValue && jobData.LastestCursors != null && jobData.LastestCursors.ContainsKey("Files") && !string.IsNullOrEmpty(jobData.LastestCursors["Files"]))
                {
                    var cursor = jobData.LastestCursors["Files"];
                    GetFolderItemsFromCursor(options, cursor, client, jobData, list);
                }
                else
                {
                    var folders = (jobData.Folders?.Select(sp => sp.EntryPoint) ?? new string[] { }).ToHashSet();

                    if (!folders.Any() || folders.Contains("/") || folders.Contains(string.Empty))
                    {
                        GetFolderItems(options, client, jobData,"/", new HashSet<string>(), list);
                    }
                    else
                    {
                        foreach (var path in folders)
                        {
                            GetFolderItems(options, client, jobData, path, new HashSet<string>(), list);
                        }
                    }
                }


                // Cursors
                SetCursor(options, client, jobData);
            }
            catch (OperationCanceledException)
            {
                // Swallow
            }
            catch (Exception ex)
            {
                _log.Fatal(() => GetType().Name + " Failed: " + ex.Message, ex);
                _state.Status.Statistics.Tasks.IncrementTaskFailureCount();  //
                _state.Result.Exceptions.Add(ex);
            }
        }

        private void SetCursor(CrawlOptions options, IDropBoxClient client, DropBoxCrawlJobData jobData)
        {
            if (_state.CancellationTokenSource.IsCancellationRequested)
                return;

            try
            {

                var cursor = client.ListFolderGetLatestCursorAsync(string.Empty, recursive: true, includeMediaInfo: false).Result;
                if (cursor != null)
                {
                    jobData.LastestCursors["Files"] = cursor.Cursor;
                    _state.Result.LastestCursors["Files"] = cursor.Cursor; 
                }

            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _log.Error(() => "Could not fetch data from Dropbox", exception);
                 _state.Status.Statistics.Tasks.IncrementTaskFailureCount(); 
            }
        }

        private void GetFolderItemsFromCursor(CrawlOptions options, string cursor, IDropBoxClient client, DropBoxCrawlJobData jobData, IList<object> list)
        {
            if (_state.CancellationTokenSource.IsCancellationRequested)
                return;

            if (string.IsNullOrEmpty(cursor))
            {
                return;
            }

            var dateTime = GetModifiedLastCrawlFinishTime(jobData);

            try
            {
                var items = client.ListFolderContinueAsync(jobData.LastestCursors["Files"]).Result;

                EnumerateFolderItems(options, client, jobData, items, dateTime, list);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _log.Error(() => "Could not fetch data from path in Dropbox", exception);
                _state.Status.Statistics.Tasks.IncrementTaskFailureCount();
            }
        }

        private void GetSharedFolders(IDropBoxClient client, IList<object> list)
        {
            if (_state.CancellationTokenSource.IsCancellationRequested) return;

            try
            {
                var sharedFolders = client.ListFoldersAsync().Result;

                if (sharedFolders == null)
                {
                    return;
                }

                do
                {
                    foreach (var sharedFolder in sharedFolders.Entries)
                    {
                        if (_state.CancellationTokenSource.IsCancellationRequested)
                            break;

                        list.Add(sharedFolder);
                    }

                    if (!string.IsNullOrEmpty(sharedFolders.Cursor))
                    {
                        sharedFolders = client.ListFoldersContinueAsync(sharedFolders.Cursor).Result;
                    }
                    else
                    {
                        break;
                    }
                } while (sharedFolders != null && !_state.CancellationTokenSource.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _log.Error(() => "Could not fetch data from Dropbox", exception);
                _state.Status.Statistics.Tasks.IncrementTaskFailureCount();
            }
        }

        private void EnumerateFolderItems(CrawlOptions options, IDropBoxClient client, DropBoxCrawlJobData jobData, ListFolderResult items, DateTimeOffset dateTime, IList<object> list, bool iterateFolders = true, HashSet<string> visitedFolders = null)
        {
            if (_state.CancellationTokenSource.IsCancellationRequested)
                return;

            try
            {
                var ids = (jobData.Folders?.Select(sp => sp.EntryPoint) ?? new string[] { }).ToList();

                do
                {
                    var files = items.Entries.Where(i => i != null && i.IsFile).Select(i => i.AsFile);
                    var folders = items.Entries.Where(i => i != null && i.IsFolder).Select(i => i.AsFolder);

                    var concurrencyLevel = ConfigurationManager.AppSettings.GetValue("Providers.Dropbox.CrawlConcurrencyLevel", Environment.ProcessorCount);

                    var parallelOptions = new ParallelOptions
                    {
                        CancellationToken = _state.CancellationTokenSource.Token,
                        MaxDegreeOfParallelism = concurrencyLevel,
                        TaskScheduler = _state.TaskScheduler
                    };

                    Parallel.ForEach(files, parallelOptions, file =>
                    {
                        if (_state.CancellationTokenSource.IsCancellationRequested)
                            return;

                        list.Add(GetFileAsync(file, client, dateTime));
                    });

                    foreach (var folder in folders)
                    {
                        if (_state.CancellationTokenSource.IsCancellationRequested)
                            break;

                        if (!ids.Any() || ids.Contains(folder.PathLower))
                        {
                            list.Add(folder);

                            if (iterateFolders)
                            {
                                GetFolderItems(options, client, jobData, folder.PathLower, visitedFolders, list);
                            }
                        }
                    }

                    if (items.HasMore)
                    {
                        items = client.ListFolderContinueAsync(items.Cursor).Result;
                    }
                    else
                    {
                        break;
                    }
                }
                while (items != null && !_state.CancellationTokenSource.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _log.Error(() => "Could not enumerate folder items in Dropbox", exception);
                _state.Status.Statistics.Tasks.IncrementTaskFailureCount();
            }
        }
        private static string NormalizePath(string path)
        {
            path = path?.Trim() ?? string.Empty;

            if (path == "/")
            {
                path = string.Empty;
            }

            return path;
        }

        private void GetFolderItems(CrawlOptions options, IDropBoxClient client, DropBoxCrawlJobData jobData, string path, HashSet<string> visitedFolders, IList<object> list)
        {
            if (_state.CancellationTokenSource.IsCancellationRequested) return;

            path = NormalizePath(path);

            if (visitedFolders != null)
            {
                if (visitedFolders.Contains(path))
                {
                    return;
                }

                visitedFolders.Add(path);
            }

            var dateTime = GetModifiedLastCrawlFinishTime(jobData);

            try
            {
                var items = client.ListFolderAsync(path: path, limit:DropBoxConstants.FetchLimit, includeDeleted: false).Result;

                EnumerateFolderItems(options, client, jobData, items, dateTime, list, iterateFolders: true, visitedFolders: visitedFolders);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _log.Error(() => "Could not fetch data from path in Dropbox", exception);
                 _state.Status.Statistics.Tasks.IncrementTaskFailureCount();
            }
        }

        private Metadata GetFileAsync(FileMetadata file, IDropBoxClient client, DateTimeOffset dateTime)
        {
            if (_state.CancellationTokenSource.IsCancellationRequested) return default(Metadata);

            try
            {
                var versions = client.ListRevisionsAsync(file.PathLower, limit: 100).Result;
                if (versions != null)
                {
                    foreach (var version in versions.Entries)
                    {
                        if (_state.CancellationTokenSource.IsCancellationRequested)
                            return default(Metadata);

                        if (version.ServerModified < dateTime)
                        {
                            continue;
                        }

                        return version;
                    }
                }
                else
                {
                    return file;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _log.Error(new { Path = file.PathLower }, () => "Could not fetch versions data from path in Dropbox", exception);

                // If I could not fetch the individual versions then go just get the MetaData for the latest version
                try
                {
                    var metaData = client.GetMetadataAsync(file.PathLower, false, false).Result;
                    if (metaData != null)
                    {
                        if (metaData.AsFile.ServerModified >= dateTime)
                        {
                            return metaData;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exc)
                {
                    _log.Error(new { Path = file.PathLower }, () => "Could not fetch individual file metadata after failing to fetch the versions in Dropbox", exc);
                    _state.Status.Statistics.Tasks.IncrementTaskFailureCount();
                }
            }

            return default(Metadata);
        }
        
        private DateTimeOffset GetModifiedLastCrawlFinishTime(DropBoxCrawlJobData jobData)
        {
            var dateTime = jobData.LastCrawlFinishTime;
            if (dateTime > DateTimeOffset.MinValue)
            {
                dateTime = dateTime.AddDays(-2);
            }

            return dateTime;
        }
    }
}
