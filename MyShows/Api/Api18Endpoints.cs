using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;
using MyShows.Configuration;
using MyShows.MyShowsApi.Api18;

namespace MyShows.Api
{
    [Route("/MyShows/v1.8/login/{UserId}", "POST")]
    public class LoginV18
    {
        [ApiMember(Name = "login", IsRequired = true, DataType = "string", ParameterType = "form", Verb = "POST")]
        public string Login { get; set; }
        [ApiMember(Name = "password", IsRequired = true, DataType = "string", ParameterType = "form", Verb = "POST")]
        public string Password { get; set; }
        [ApiMember(Name = "id", Description = "Jellyfin's user id", IsRequired = true, DataType = "Guid", ParameterType = "path", Verb = "POST")]
        public string UserId { get; set; }
    }

    public class WebService18 : IService
    {
        private readonly IJsonSerializer _json;
        private readonly IHttpClient _httpClient;

        public WebService18(
            IJsonSerializer json,
            IHttpClient httpClient)
        {
            _json = json;
            _httpClient = httpClient;
        }

        public async Task<object> Post(LoginV18 request)
        {
            var password = GetMD5(request.Password);
            var uri = $"{ApiConstants.BaseUri}/profile/login?login={request.Login}&password={password}";
            var options = GetOptions(uri);
            var response = await _httpClient.GetResponse(options);
            if (!response.IsSuccessStatusCode())
            {
                return new 
                {
                    success = false,
                    error = "API error",
                    error_code = response.StatusCode.ToString(),
                };
                //throw new Exception("Error: " + response.StatusCode);
            }
            var cookies = response.Headers.GetValues("Set-Cookie");
            // TODO: parse cookie /PHPSESSID=(.+);/ and store
            foreach (var cookie in cookies)
            {
                if (cookie.Contains("PHPSESSID"))
                {
                    var rgx = new Regex(@"PHPSESSID=(.+);", RegexOptions.IgnoreCase);
                    var match = rgx.Match(cookie);

                    if (match.Success)
                    {
                        Plugin.Instance.PluginConfiguration.AddUser(new UserConfig
                        {
                            ApiVersion = MyShowsApi.MyShowsApiVersion.V18,
                            AccessToken = match.Value,
                            Id = request.UserId,
                            Name = request.Login
                        });
                        return new { success = true };
                    }
                }
            }

            return new
            {
                success = false,
                error = "Cookie not found"
            };
        }

        private static HttpRequestOptions GetOptions(string uri)
        {
            var options = new HttpRequestOptions
            {
                LogErrorResponseBody = true,
                EnableDefaultUserAgent = true,
                Url = uri,
            };
            
            return options;
        }

        private static string GetMD5(string password)
        {
            var encodedPassword = new UTF8Encoding().GetBytes(password);
            var hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
            return BitConverter.ToString(hash)
               .Replace("-", string.Empty)
               .ToLower();
        }
    }
}
