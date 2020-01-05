using MediaBrowser.Common.Net;

namespace MyShows
{
    public static class Extensions
    {
        public static bool IsSuccessStatusCode(this HttpResponseInfo response)
            => ((int)response.StatusCode >= 200) && ((int)response.StatusCode <= 299);
    }
}
