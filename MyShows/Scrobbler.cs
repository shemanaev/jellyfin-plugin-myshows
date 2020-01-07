using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using MyShows.Configuration;
using MyShows.MyShowsApi;

namespace MyShows
{
    public class Scrobbler : IServerEntryPoint
    {
        private readonly ILogger _logger;
        private readonly ISessionManager _sessionManager;
        private readonly IUserDataManager _userDataManager;
        private MyShowsApiFactory _client;
        private UserDataHelper _userDataHelper;
        private DateTime _nextTry;
        private List<Guid> _lastScrobbled;

        public Scrobbler(
            IJsonSerializer json,
            ISessionManager sessionManager,
            IUserDataManager userDataManager,
            ILoggerFactory logger,
            IHttpClient httpClient
            )
        {
            _sessionManager = sessionManager;
            _userDataManager = userDataManager;
            _logger = logger.CreateLogger("MyShows");

            _client = new MyShowsApiFactory(_logger, json, httpClient);
            _userDataHelper = new UserDataHelper(_logger, _client);

            _nextTry = DateTime.UtcNow;
            _lastScrobbled = new List<Guid>();
        }

        public void Dispose()
        {
            _userDataManager.UserDataSaved -= OnUserDataSaved;
            _sessionManager.PlaybackStart -= OnPlaybackStart;
            _sessionManager.PlaybackStopped -= OnPlaybackStopped;
            _sessionManager.PlaybackProgress -= OnPlaybackProgress;

            _client = null;
            _userDataHelper = null;
        }

        public Task RunAsync()
        {
            _userDataManager.UserDataSaved += OnUserDataSaved;
            _sessionManager.PlaybackStart += OnPlaybackStart;
            _sessionManager.PlaybackStopped += OnPlaybackStopped;
            _sessionManager.PlaybackProgress += OnPlaybackProgress;

            return Task.CompletedTask;
        }

        private async void OnPlaybackProgress(object sender, PlaybackProgressEventArgs e)
        {
            if (DateTime.UtcNow < _nextTry) return; // postpone
            _nextTry = DateTime.UtcNow.AddSeconds(30);

            if (!CheckConstraintsAndGetUser(e.Session.UserId, e.Item, out var user)) return;

            // don't scrobble if percentage watched is below 90%
            float percentageWatched = (float)e.Session.PlayState.PositionTicks / (float)e.Session.NowPlayingItem.RunTimeTicks * 100f;
            if (percentageWatched < user.ScrobbleAt) return;

            var episode = e.Item as Episode;
            if (_lastScrobbled.Contains(episode.Id)) return;

            try
            {
                _logger.LogInformation("Item is played 90%. Scrobble");

                var result = await _client.GetApi(user.ApiVersion).CheckEpisode(user, episode);
                _logger.LogInformation("Checked episode '{0}' S{1}E{2} {3}", episode.Series.Name,
                    episode.Season.IndexNumber, episode.IndexNumber, result ? "successfully" : "failed");
                if (result) _lastScrobbled.Add(episode.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending watching status update");
            }
        }

        private async void OnUserDataSaved(object sender, UserDataSaveEventArgs e)
        {
            // ignore change events for any reason other than manually toggling played.
            if (e.SaveReason != UserDataSaveReason.TogglePlayed)
            {
                return;
            }

            if (e.Item is BaseItem baseItem)
            {
                var user = Plugin.Instance.Configuration.GetUserById(e.UserId);

                // Can't progress
                if (user == null || !CanSync(user, baseItem))
                {
                    return;
                }

                await _userDataHelper.AddEvent(user, e);
            }
        }

        private async void OnPlaybackStart(object sender, PlaybackProgressEventArgs e)
        {
            _logger.LogInformation("Playback Started");

            if (!CheckConstraintsAndGetUser(e.Session.UserId, e.Item, out var user)) return;

            try
            {
                var episode = e.Item as Episode;
                var result = await _client.GetApi(user.ApiVersion).SetShowStatusToWatching(user, episode.Series);
                _logger.LogDebug("Started watching show '{0}' {1}", episode.Series.Name, result ? "successfully" : "failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending watching status update");
            }

        }

        private async void OnPlaybackStopped(object sender, PlaybackStopEventArgs e)
        {
            _logger.LogInformation("Playback Stopped");

            if (!CheckConstraintsAndGetUser(e.Session.UserId, e.Item, out var user)) return;

            if (!e.PlayedToCompletion) return;

            var episode = e.Item as Episode;
            if (_lastScrobbled.Contains(episode.Id)) return;

            try
            {
                _logger.LogInformation("Item is played. Scrobble");
                
                var result = await _client.GetApi(user.ApiVersion).CheckEpisode(user, episode);
                _logger.LogInformation("Checked episode '{0}' S{1}E{2} {3}", episode.Series.Name,
                    episode.Season.IndexNumber, episode.IndexNumber, result ? "successfully" : "failed");
                if (result) _lastScrobbled.Add(episode.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending watching status update");
            }
        }

        private bool CheckConstraintsAndGetUser(Guid userId, BaseItem item, out UserConfig user)
        {
            user = Plugin.Instance.PluginConfiguration.GetUserById(userId);

            if (user == null)
            {
                _logger.LogInformation("Could not match user with any stored credentials");
                return false;
            }

            if (!CanSync(user, item))
            {
                _logger.LogDebug("Can not sync this type of items: {0}", item.MediaType);
                return false;
            }

            return true;
        }

        public bool CanSync(UserConfig user, BaseItem item)
        {
            if (item.Path == null || item.LocationType == LocationType.Virtual)
            {
                return false;
            }

            if (item is Episode episode
                && episode.Series != null
                && !episode.IsMissingEpisode
                && (episode.IndexNumber.HasValue || !string.IsNullOrEmpty(episode.GetProviderId(MetadataProviders.Tvdb))))
            {
                var series = episode.Series;

                return !string.IsNullOrEmpty(series.GetProviderId(MetadataProviders.Imdb))
                    || !string.IsNullOrEmpty(series.GetProviderId(MetadataProviders.Tvdb))
                    || !string.IsNullOrEmpty(series.GetProviderId(MetadataProviders.TvRage))
                    || !string.IsNullOrEmpty(series.GetProviderId(MetadataProviders.TvMaze));
            }

            return false;
        }
    }
}
