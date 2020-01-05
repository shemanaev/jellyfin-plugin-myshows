//#define LOCAL_SERVER
namespace MyShows.MyShowsApi.Api20
{
    internal class ApiConstants
    {
#if LOCAL_SERVER
        public const string RpcUri = "https://api.myshows.me/v2/rpc/";
        public const string OauthAuthorizeUri = "http://localhost:3000/dialog/authorize";
        public const string OauthTokenUri = "http://localhost:3000/oauth/token";
        public const string ClientId = "abc123";
        public const string ClientSecret = "ssh-secret";
#else
        public const string RpcUri = "https://api.myshows.me/v2/rpc/";
        public const string OauthAuthorizeUri = "https://myshows.me/oauth/authorize";
        public const string OauthTokenUri = "https://myshows.me/oauth/token";

#if DEBUG
        public const string ClientId = "apidoc";
        public const string ClientSecret = "apidoc";
#else
        public const string ClientId = "apidoc";
        public const string ClientSecret = "apidoc";
#endif
#endif
    }
}
