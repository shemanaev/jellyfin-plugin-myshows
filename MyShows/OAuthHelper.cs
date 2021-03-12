using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MyShows.MyShowsApi.Api20;

namespace MyShows
{
    internal class OAuthHelper
    {
        public static async Task<(OAuthToken, OAuthError)> GetToken(
            HttpClient http,
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

            return await SendRequest(http, formContent);
        }

        public static async Task<(OAuthToken, OAuthError)> RefreshToken(
            HttpClient http,
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

            return await SendRequest(http, formContent);
        }

        private static async Task<(OAuthToken, OAuthError)> SendRequest(
            HttpClient httpClient,
            FormUrlEncodedContent data
            )
        {
            var response = await httpClient.PostAsync(ApiConstants.OauthTokenUri, data).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await Extensions.DeserializeFromHttp<OAuthError>(response);
                return (null, error);
            }

            var resp = await Extensions.DeserializeFromHttp<OAuthToken>(response);
            return (resp, null);
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
