using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;
using MyShows.Configuration;
using MyShows.MyShowsApi.Api20;

// https://github.com/MediaBrowser/Emby/wiki/Creating-Api-Endpoints
namespace MyShows.Api
{
    [Route("/MyShows/v2/oauth/{UserId}", "GET")]
    public class OAuthCallback
    {
        [ApiMember(Name = "code", IsRequired = true, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string Code { get; set; }
        [ApiMember(Name = "id", Description = "Jellyfin's user id", IsRequired = true, DataType = "Guid", ParameterType = "path", Verb = "GET")]
        public string UserId { get; set; }
    }

    [Route("/MyShows/v2/login/{UserId}", "POST")]
    public class LoginV2
    {
        [ApiMember(Name = "login", IsRequired = true, DataType = "string", ParameterType = "form", Verb = "POST")]
        public string Login { get; set; }
        [ApiMember(Name = "password", IsRequired = true, DataType = "string", ParameterType = "form", Verb = "POST")]
        public string Password { get; set; }
        [ApiMember(Name = "id", Description = "Jellyfin's user id", IsRequired = true, DataType = "Guid", ParameterType = "path", Verb = "POST")]
        public string UserId { get; set; }
    }

    [Route("/MyShows/v2/initialize", "GET")]
    public class Initialize { }

    public class WebService : IService, IRequiresRequest
    {
        private const string HTMLContentType = "text/html; charset=UTF-8";
        private readonly IJsonSerializer _json;
        private readonly IHttpClient _httpClient;
        private readonly IHttpResultFactory _resultFactory;

        public IRequest Request { get; set; }

        public WebService(
            IJsonSerializer json,
            IHttpClient httpClient,
            IHttpResultFactory httpResultFactory)
        {
            _json = json;
            _httpClient = httpClient;
            _resultFactory = httpResultFactory;
        }

        public object Get(Initialize request)
        {
            var result = new Dictionary<string, string>
            {
                { "plugin_id", Plugin.Instance.Id.ToString() },
                { "client_id", ApiConstants.ClientId },
                { "api_uri", ApiConstants.OauthAuthorizeUri },
            };

            return result;
        }

        public async Task<object> Post(LoginV2 request)
        {
            try
            {
                //var formContent = new FormUrlEncodedContent(new[]
                //{
                //    new KeyValuePair<string, string>("grant_type", "password"),
                //    new KeyValuePair<string, string>("client_id", ApiConstants.ClientId),
                //    new KeyValuePair<string, string>("client_secret", ApiConstants.ClientSecret),
                //    new KeyValuePair<string, string>("username", request.Login),
                //    new KeyValuePair<string, string>("password", request.Password)
                //});
                //var form = await formContent.ReadAsStringAsync();

                //var options = GetOptions(form);
                //var response = await _httpClient.Post(options).ConfigureAwait(false);

                var (token, error) = await OAuthHelper.GetToken(_json, _httpClient, request.Login, request.Password);

                if (error != null)
                {
                    return new
                    {
                        success = false,
                        statusText = error.error_description
                    };
                }

                //var resp = _json.DeserializeFromStream<OAuthToken>(response.Content);

                Plugin.Instance.PluginConfiguration.AddUser(new UserConfig
                {
                    ApiVersion = MyShowsApi.MyShowsApiVersion.V20,
                    AccessToken = token.access_token,
                    RefreshToken = token.refresh_token,
                    ExpirationTime = DateTime.Now.AddSeconds(token.expires_in),
                    Id = request.UserId,
                    Name = request.Login
                });
            }
            catch (HttpRequestException e)
            {
                return new
                {
                    success = false,
                    statusText = e.Message
                };
            }

            return new { success = true };
        }

        public async Task<object> Get(OAuthCallback request)
        {
            string result = @"<html><body>";

            //var client = new HttpClient();
            //client.DefaultRequestHeaders.UserAgent.ParseAdd(ApiConstants.UserAgent);
            try
            {
                var uri = new Uri(Request.AbsoluteUri);
                var baseUri = uri.GetLeftPart(UriPartial.Authority);
                var redirectUri = $"{baseUri}/MyShows/oauth/{request.UserId}";
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_id", ApiConstants.ClientId),
                    new KeyValuePair<string, string>("client_secret", ApiConstants.ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("code", request.Code)
                });
                var form = await formContent.ReadAsStringAsync();

                var options = GetOptions(form);
                var response = await _httpClient.Post(options).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode())
                {
                    throw new Exception("Error: " + response.StatusCode);
                }

                //var response = await client.PostAsync(ApiConstants.OauthTokenUri, formContent);
                //response.EnsureSuccessStatusCode();
                //string responseBody = await response.Content.ReadAsStringAsync();

#if DEBUG
                var mem = new MemoryStream();
                response.Content.CopyTo(mem);
                mem.Seek(0, SeekOrigin.Begin);
                var resp = _json.DeserializeFromStream<OAuthToken>(mem);
                mem.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(mem);
                string text = reader.ReadToEnd();
                result += "<b>body: " + text + "\n\ntoken: " + resp.access_token + "\n\nexp: " + resp.expires_in + "</b>";
#else
                var resp = _json.DeserializeFromStream<OAuthToken>(response.Content);
                result += "<b>token: " + resp.access_token + "\n\nexp: " + resp.expires_in + "</b>"; // TODO: replace with self-close html code
#endif
                Plugin.Instance.PluginConfiguration.AddUser(new UserConfig
                {
                    ApiVersion = MyShowsApi.MyShowsApiVersion.V20,
                    AccessToken = resp.access_token,
                    Id = request.UserId,
                    //Name = resp.username
                });
            }
            catch (HttpRequestException e)
            {
                result += $"Error: {e.Message}";
            }

            result += @"</body></html>";
            return _resultFactory.GetResult(Request, result, HTMLContentType);
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
}
