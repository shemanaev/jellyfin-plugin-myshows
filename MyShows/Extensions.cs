using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Common.Json;
using MediaBrowser.Model.Entities;

namespace MyShows
{
    public static class Extensions
    {
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

        public static async Task<T> DeserializeFromHttp<T>(HttpResponseMessage response)
        {
            var contentStream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<T>(contentStream, JsonDefaults.GetOptions());
            return result;
        }
    }
}
