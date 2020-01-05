using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using MyShows.Configuration;

#pragma warning disable CS1998
namespace MyShows.MyShowsApi.Api18
{
    internal class MyShowsApi18 : IMyShowsApi
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _json;
        private readonly IHttpClient _httpClient;
        // PHPSESSID=0rkju2oojdbbsb7eui4q8jirs5; path=/; domain=.myshows.me; secure; HttpOnly
        public MyShowsApi18(ILogger logger, IJsonSerializer json, IHttpClient httpClient)
        {
            _logger = logger;
            _json = json;
            _httpClient = httpClient;
        }

        public async Task<bool> CheckEpisode(UserConfig user, Episode item)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetShowStatusToWatching(UserConfig user, Series item)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SyncEpisodes(UserConfig user, List<Episode> seen, List<Episode> unseen)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UnCheckEpisode(UserConfig user, Episode item)
        {
            throw new NotImplementedException();
        }
    }
}
