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

        // TODO Create parameterless constructor, make privates as properties of interface so we can mock this class

        public DropBoxClient(ILogger log, DropBoxCrawlJobData dropBoxCrawlJobData, IRestClient restClient) 
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));

            _restClient.BaseUrl = string.IsNullOrEmpty(dropBoxCrawlJobData.BaseUri) ? new Uri(DropBoxConstants.ApiUri) : new Uri(dropBoxCrawlJobData.BaseUri);
           //_restClient.AddDefaultParameter("api_key", dropBoxCrawlJobData.ClientId, ParameterType.QueryString);
            _restClient.AddDefaultHeader("Authorization", "Bearer " + dropBoxCrawlJobData.Token.AccessToken);
            //_restClient.AddDefaultHeader("API-Select-Admin", dropBoxCrawlJobData.AdminMemberId); // TODO confirm we want to access as admin in DropBox Business accounts (see https://www.dropbox.com/developers/documentation/http/teams)

            _dropBoxClient = new DropboxClient(dropBoxCrawlJobData.Token.AccessToken); // TODO figure out to DI this
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
            await Execute(async () => await _dropBoxClient.Users.GetCurrentAccountAsync());

        public async Task<ListFoldersResult> ListFoldersAsync() =>
            await Execute(async () => await _dropBoxClient.Sharing.ListFoldersAsync());

        public async Task<ListFolderResult> ListFolderContinueAsync(string cursor) =>
            await Execute(async () => await _dropBoxClient.Files.ListFolderContinueAsync(cursor));

        public async Task<ListFoldersResult> ListFoldersContinueAsync(string cursor) =>
            await Execute(async () => await _dropBoxClient.Sharing.ListFoldersContinueAsync(cursor));

        public async Task<ListFolderResult> ListFolderAsync(string path, uint? limit = 10, bool includeDeleted = false) =>
            await Execute(async () => await _dropBoxClient.Files.ListFolderAsync(path: path, includeDeleted: includeDeleted, limit: limit));

        public async Task<ListRevisionsResult> ListRevisionsAsync(string path, ulong limit = 100) =>
            await Execute(async () => await _dropBoxClient.Files.ListRevisionsAsync(path, limit: limit));

        public async Task<Metadata> GetMetadataAsync(string path, bool includeMediaInfo = false, bool includeDeleted = false) =>
            await Execute(async () => await _dropBoxClient.Files.GetMetadataAsync(path, includeMediaInfo, includeDeleted));

        public async Task<ListFolderGetLatestCursorResult> ListFolderGetLatestCursorAsync(string path, bool recursive = true, bool includeMediaInfo = false) =>
            await Execute(async () => await _dropBoxClient.Files.ListFolderGetLatestCursorAsync(string.Empty, recursive: recursive, includeMediaInfo: includeMediaInfo));

        public async Task<IDownloadResponse<FileMetadata>> GetThumbnailAsync(string path, ThumbnailFormat format = null, ThumbnailSize size = null, ThumbnailMode mode = null) =>
            await _dropBoxClient.Files.GetThumbnailAsync(path, format, size, mode);

        public async Task<FolderList> GetFolderListViaRestAsync() =>
            await PostAsync<FolderList>("/sharing/list_folders", null);

        public async Task<Permissions> GetFolderPermissions(Entry folder, int limit = 10) =>
            await PostAsync<Permissions>("sharing/list_folder_members", new MemberPost {shared_folder_id = folder.shared_folder_id, limit = limit}, new Dictionary<string, string>
            {
                {"Content-Type", "application/json" }}
            );

        public async Task<IDownloadResponse<FileMetadata>> DownloadAsync(string path, string revision) =>
            await _dropBoxClient.Files.DownloadAsync(path, revision);

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
                    return await func();
                }
                catch (AggregateException ex)
                {
                    var flat = ex.Flatten();

                    if (flat.InnerExceptions.Count == 1 && flat.InnerExceptions[0] is RateLimitException rateEx)
                        await Task.Delay(TimeSpan.FromSeconds(rateEx.RetryAfter)); //, _state.CancellationTokenSource.Token);  // TODO Find out how to access state
                    else
                        throw;
                }
                catch (RateLimitException ex)
                {
                    await Task.Delay(TimeSpan.FromSeconds(ex.RetryAfter)); //, _state.CancellationTokenSource.Token);  // TODO Find out how to access state
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
