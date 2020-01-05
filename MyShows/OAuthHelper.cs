using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using MyShows.MyShowsApi.Api20;

namespace MyShows
{
    internal class OAuthHelper
    {
        public static async Task<(OAuthToken, OAuthError)> GetToken(
            IJsonSerializer json,
            IHttpClient http,
            string login,
            string password
            )
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", ApiConstants.ClientId),
                new KeyValuePair<string, string>("client_secret", ApiConstants.ClientSecret),
                new KeyValuePair<string, string>("username", login),
                new KeyValuePair<string, string>("password", password)
            });

            return await SendRequest(json, http, formContent);
        }

        public static async Task<(OAuthToken, OAuthError)> RefreshToken(
            IJsonSerializer json,
            IHttpClient http,
            string token
            )
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", ApiConstants.ClientId),
                new KeyValuePair<string, string>("client_secret", ApiConstants.ClientSecret),
                new KeyValuePair<string, string>("refresh_token", token)
            });

            return await SendRequest(json, http, formContent);
        }

        private static async Task<(OAuthToken, OAuthError)> SendRequest(
            IJsonSerializer json,
            IHttpClient http,
            FormUrlEncodedContent data
            )
        {
            var form = await data.ReadAsStringAsync();

            var options = GetOptions(form);
            var response = await http.Post(options).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode())
            {
                var error = json.DeserializeFromStream<OAuthError>(response.Content);
                return (null, error);
            }

            var resp = json.DeserializeFromStream<OAuthToken>(response.Content);
            return (resp, null);
        }

        private static HttpRequestOptions GetOptions(string content)
        {
            var options = new HttpRequestOptions
            {
                RequestContentType = "application/x-www-form-urlencoded",
                LogErrorResponseBody = true,
                EnableDefaultUserAgent = true,
                Url = ApiConstants.OauthTokenUri,
                RequestContent = content,
            };

            return options;
        }
    }

    public class OAuthToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class OAuthError
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
