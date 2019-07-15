using System;
using CluedIn.Core.Logging;
using CluedIn.Core.Providers;
using CluedIn.Crawling.DropBox.Core;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CluedIn.Core.Agent.Jobs;
using CluedIn.Crawling.DropBox.Core.Models;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using Dropbox.Api.Stone;
using Dropbox.Api.Users;
using Newtonsoft.Json;
using RestSharp;

namespace CluedIn.Crawling.DropBox.Infrastructure
{
    public class DropBoxClient : IDropBoxClient
    {
        private readonly IRestClient _restClient;
        private readonly DropboxClient _dropBoxClient;
        private readonly ILogger _log;

        public DropBoxClient(ILogger log, DropBoxCrawlJobData crawlJobData, IRestClient restClient) 
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            //_dropBoxClient = dropBoxClient ?? throw new ArgumentNullException(nameof(dropBoxClient));

            restClient.BaseUrl = new Uri(crawlJobData.BaseUri);
            restClient.AddDefaultParameter("api_key", crawlJobData.ApiKey, ParameterType.QueryString);
            restClient.AddDefaultHeader("Authorization", "Bearer " + crawlJobData.Token.AccessToken);

            _dropBoxClient = new DropboxClient(crawlJobData.Token.AccessToken);
        }


        public async Task<AccountInformation> GetAccountInformationAsync()
        {
            var accountId = string.Empty;
            var accountDisplay = string.Empty;
            try
            {
                var accountInformation = await GetCurrentAccountAsync();
                accountId = accountInformation.AccountId.ToString(CultureInfo.InvariantCulture);
                accountDisplay = accountInformation.Name.DisplayName;
            }
            catch (Exception exception)
            {
                _log.Warn(() => "Could not add DropBox provider", exception);
                return new AccountInformation(accountId, accountDisplay) { Errors = new Dictionary<string, string>() { { "error", "Please contact CluedIn support in the top menu to help you setup with Dropbox." } } };
            }

            return new AccountInformation(accountId, accountDisplay);
        }

        public async Task<SpaceUsage> GetSpaceUsageAsync() =>
            await _dropBoxClient.Users.GetSpaceUsageAsync();

        public async Task<FullAccount> GetCurrentAccountAsync() =>
            await Execute(async () => await _dropBoxClient.Users.GetCurrentAccountAsync()).ConfigureAwait(false);

        public async Task<ListFoldersResult> ListFoldersAsync() =>
            await Execute(async () => await _dropBoxClient.Sharing.ListFoldersAsync()).ConfigureAwait(false);

        public async Task<ListFolderResult> ListFolderContinueAsync(string cursor) =>
            await Execute(async () => await _dropBoxClient.Files.ListFolderContinueAsync(cursor)).ConfigureAwait(false);

        public async Task<ListFoldersResult> ListFoldersContinueAsync(string cursor) =>
            await Execute(async () => await _dropBoxClient.Sharing.ListFoldersContinueAsync(cursor)).ConfigureAwait(false);

        public async Task<ListFolderResult> ListFolderAsync(string path, bool includeDeleted = false) =>
            await Execute(async () => await _dropBoxClient.Files.ListFolderAsync(path: path, includeDeleted: includeDeleted)).ConfigureAwait(false);

        public async Task<ListRevisionsResult> ListRevisionsAsync(string path, ulong limit = 100) =>
            await Execute(async () => await _dropBoxClient.Files.ListRevisionsAsync(path, limit: limit)).ConfigureAwait(false);

        public async Task<Metadata> GetMetadataAsync(string path, bool includeMediaInfo = false, bool includeDeleted = false) =>
            await Execute(async () => await _dropBoxClient.Files.GetMetadataAsync(path, includeMediaInfo, includeDeleted)).ConfigureAwait(false);

        public async Task<ListFolderGetLatestCursorResult> ListFolderGetLatestCursorAsync(string path, bool recursive = true, bool includeMediaInfo = false) =>
            await Execute(async () => await _dropBoxClient.Files.ListFolderGetLatestCursorAsync(string.Empty, recursive: recursive, includeMediaInfo: includeMediaInfo)).ConfigureAwait(false);

        public Task<IDownloadResponse<FileMetadata>> GetThumbnailAsync(string path, ThumbnailFormat format = null, ThumbnailSize size = null, ThumbnailMode mode = null)
        {
            throw new NotImplementedException();
        }

        public async Task<FolderList> GetFolderListViaRestAsync() =>
            await PostAsync<FolderList>("sharing/list_folders", null);

        public async Task<Permissions> GetFolderPermissions(Entry folder, int limit = 10) =>
            await PostAsync<Permissions>("sharing/list_folder_members", new MemberPost {shared_folder_id = folder.shared_folder_id, limit = limit}, new Dictionary<string, string>
            {
                {"Content-Type", "application/json" }}
            );

        private async Task<T> GetAsync<T>(string url, IList<QueryStringParameter> parameters = null)
        {
            var request = new RestRequest(url, Method.GET);

            AddParametersToRequest(parameters, request);

            var response = await _restClient.ExecuteTaskAsync(request);

            return GetRequestResponse<T>(url, response);
        }

        private async Task<T> PostAsync<T>(string url, object body, IDictionary<string, string> headers = null, IList<QueryStringParameter> parameters = null)
        {
            var request = new RestRequest(url, Method.POST);

            AddParametersToRequest(parameters, request);
            AddHeadersToRequest(headers, request);

            if (body != null)
            {
                request.AddJsonBody(body);
            }

            var response = await _restClient.ExecuteTaskAsync(request);

            return GetRequestResponse<T>(url, response);
        }

        protected async Task<T> Execute<T>(Func<Task<T>> func)
        {
            do
            {
                //if (_state.CancellationTokenSource.IsCancellationRequested) // TODO Find out how to access state
                //    throw new OperationCanceledException();

                try
                {
                    return await func().ConfigureAwait(false);
                }
                catch (AggregateException ex)
                {
                    var flat = ex.Flatten();

                    if (flat.InnerExceptions.Count == 1 && flat.InnerExceptions[0] is RateLimitException rateEx)
                        await Task.Delay(TimeSpan.FromSeconds(rateEx.RetryAfter)); //, _state.CancellationTokenSource.Token).ConfigureAwait(false);  // TODO Find out how to access state
                    else
                        throw;
                }
                catch (RateLimitException ex)
                {
                    await Task.Delay(TimeSpan.FromSeconds(ex.RetryAfter)); //, _state.CancellationTokenSource.Token).ConfigureAwait(false);  // TODO Find out how to access state
                }
            }
            while (true);
        }

        private static void AddParametersToRequest(IList<QueryStringParameter> parameters, RestRequest request)
        {
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    request.AddParameter(parameter.Parameter);
                }
            }
        }

        private static void AddHeadersToRequest(IDictionary<string, string> headers, RestRequest request)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }
        }

        private T GetRequestResponse<T>(string url, IRestResponse response)
        {
            _log.Verbose($"DropBoxClient.GetAsync calling {url}");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var diagnosticMessage = $"Request to {_restClient.BaseUrl}{url} failed, response {response.ErrorMessage} ({response.StatusCode})";

                _log.Error(() => diagnosticMessage);

                throw new InvalidOperationException($"Communication to DropBox unavailable. {diagnosticMessage}");
            }

            var data = JsonConvert.DeserializeObject<T>(response.Content);

            _log.Verbose($"DropBoxClient returning {data}");

            return data;
        }

        private class QueryStringParameter
        {
            public Parameter Parameter { get; }
            public QueryStringParameter(string name, object value)
            {
                Parameter =
                  new Parameter()
                  {
                      Type = ParameterType.QueryString,
                      Name = name,
                      Value = value.ToString()
                  };
            }
        }
    }
}
