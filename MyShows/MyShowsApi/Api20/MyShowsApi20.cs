using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using MyShows.Configuration;

namespace MyShows.MyShowsApi.Api20
{
    internal class MyShowsApi20 : IMyShowsApi
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _json;
        private readonly IHttpClient _httpClient;
        private int _counter = 1;
        private static readonly TimeSpan CACHED_SHOW_STORAGE_INTERVAL = TimeSpan.FromHours(24);
        private readonly ExpireableCache<string, ShowSummary> _showsCache = new ExpireableCache<string, ShowSummary>();
        private readonly List<Guid> _lastWatchedShows = new List<Guid>();

        public MyShowsApi20(ILogger logger, IJsonSerializer json, IHttpClient httpClient)
        {
            _logger = logger;
            _json = json;
            _httpClient = httpClient;
        }

        public async Task<bool> SetShowStatusToWatching(UserConfig user, Series item)
        {
            if (_lastWatchedShows.Contains(item.Id)) return true;

            var show = await GetShow(user, item);
            if (show == default(ShowSummary)) return false;

            var success = await Execute<bool>(user, "manage.SetShowStatus", new ManageSetShowStatusArgs
            {
                id = show.id,
                status = "watching"
            });

            if (success) _lastWatchedShows.Add(item.Id);
            return success;
        }

        public async Task<bool> CheckEpisode(UserConfig user, Episode item)
        {
            return await ToggleEpisode(user, item, true);
        }

        public async Task<bool> UnCheckEpisode(UserConfig user, Episode item)
        {
            return await ToggleEpisode(user, item, false);
        }

        private async Task<bool> ToggleEpisode(UserConfig user, Episode item, bool check)
        {
            var method = check ? "manage.CheckEpisode" : "manage.UnCheckEpisode";
            var show = await GetShow(user, item.Series);
            if (show == default(ShowSummary)) return false;
            var episode = show.episodes.First(e => e.seasonNumber == item.Season.IndexNumber && e.episodeNumber == item.IndexNumber);

            var success = await Execute<bool>(user, method, new ManageEpisodeArgs
            {
                id = episode.id
            });
            return success;
        }

        public async Task<bool> SyncEpisodes(UserConfig user, List<Episode> seen, List<Episode> unseen)
        {
            if (!seen.Any() && !unseen.Any()) return false;

            var firstEpisode = seen.Any() ? seen.First() : unseen.First();
            var show = await GetShow(user, firstEpisode.Series);
            if (show == default(ShowSummary)) return false;

            var seenIds = new List<int>();
            var unSeenIds = new List<int>();

            foreach (var ep in seen)
            {
                var episode = show.episodes.FirstOrDefault(e => e.seasonNumber == ep.Season.IndexNumber && e.episodeNumber == ep.IndexNumber);
                if (episode != default(EpisodeSummary))
                    seenIds.Add(episode.id);
            }
            foreach (var ep in unseen)
            {
                var episode = show.episodes.FirstOrDefault(e => e.seasonNumber == ep.Season.IndexNumber && e.episodeNumber == ep.IndexNumber);
                if (episode != default(EpisodeSummary))
                    unSeenIds.Add(episode.id);
            }

            var success = await Execute<bool>(user, "manage.SyncEpisodesDelta", new ManageSyncEpisodesDeltaArgs
            {
                showId = show.id,
                checkedIds = seenIds.ToArray(),
                unCheckedIds = unSeenIds.ToArray(),
            });
            return success;
        }

        protected async Task<ShowSummary> GetShow(UserConfig user, Series item)
        {
            var (id, source) = item.GetBestProviderId();
            if (source == null)
            {
                _logger.LogWarning("Not found any provider id for show '{0}'", item.Name);
                return default(ShowSummary);
            }

            var cacheKey = id.ToString() + source;

            var show = _showsCache.Get(cacheKey);
            if (show != default(ShowSummary))
            {
                return show;
            }

            show = await Execute<ShowSummary>(user, "shows.GetByExternalId", new ShowsGetByExternalIdArgs
            {
                id = id,
                source = source
            });

            if (show == default(ShowSummary)) return show;

            show = await Execute<ShowSummary>(user, "shows.GetById", new ShowsGetByIdArgs
            {
                showId = show.id,
                withEpisodes = true
            });

            _showsCache.Store(cacheKey, show, CACHED_SHOW_STORAGE_INTERVAL);

            return show;
        }

        private async Task<T> Execute<T>(UserConfig user, string method, object args)
        {
            var isTokenValid = await user.EnsureAccessTokenValid(_json, _httpClient);
            if (!isTokenValid)
            {
                _logger.LogWarning("AccessToken invalidated and RefreshToken isn't helped. Too bad.");
                return default(T);
            }

            var call = new JsonRpcCall
            {
                jsonrpc = "2.0",
                id = _counter++,
                method = method,
                @params = args,
            };
            var options = GetOptions(user.AccessToken, call);
            var response = await _httpClient.Post(options);

            var result = _json.DeserializeFromStream<JsonRpcResult<T>>(response.Content);
            if (result.error != null)
            {
                _logger.LogWarning("JSON-RPC error: {0}", result.error.message);
            }
            return result.result;
        }

        private HttpRequestOptions GetOptions(string accessToken, object data)
        {
            var options = new HttpRequestOptions
            {
                RequestContentType = "application/json",
                AcceptHeader = "application/json",
                LogErrorResponseBody = true,
                EnableDefaultUserAgent = true,
                Url = ApiConstants.RpcUri,
                RequestContent = _json.SerializeToString(data),
            };
            options.RequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            return options;
        }
    }
}
