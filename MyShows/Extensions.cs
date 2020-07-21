using MediaBrowser.Common.Net;
using MediaBrowser.Model.Entities;

namespace MyShows
{
    public static class Extensions
    {
        public static bool IsSuccessStatusCode(this HttpResponseInfo response)
            => ((int)response.StatusCode >= 200) && ((int)response.StatusCode <= 299);

        public static (int, string) GetBestProviderId(this IHasProviderIds item)
        {
            var imdb = item.GetProviderId(MetadataProvider.Imdb);
            if (!string.IsNullOrEmpty(imdb)) return (int.Parse(imdb.Replace("tt", "")), "imdb");

            var tvrage = item.GetProviderId(MetadataProvider.TvRage);
            if (!string.IsNullOrEmpty(tvrage)) return (int.Parse(tvrage), "tvrage");

            var tvdb = item.GetProviderId(MetadataProvider.Tvdb);
            if (!string.IsNullOrEmpty(tvdb)) return (int.Parse(tvdb), "thetvdb");

            var tvmaze = item.GetProviderId(MetadataProvider.TvMaze);
            if (!string.IsNullOrEmpty(tvmaze)) return (int.Parse(tvmaze), "tvmaze");

            return (-1, null);
        }
    }
}
