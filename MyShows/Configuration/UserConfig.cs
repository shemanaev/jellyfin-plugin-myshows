using System;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using MyShows.MyShowsApi;

namespace MyShows.Configuration
{
    public class UserConfig
    {
        /// <summary>
        /// Jellyfin's user id (Guid).
        /// </summary>
        public string Id { get; set; }

        public string Name { get; set; }

        public MyShowsApiVersion ApiVersion { get; set; }

        /// <summary>
        /// OAuth access token. Or PHPSESSID cookie.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// OAuth refresh token.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// OAuth expires_in.
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// Ensure OAuth access_token is vaild and refresh if it's not.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="http"></param>
        /// <returns>true if you can use token</returns>
        #pragma warning disable CS1998
        public async Task<bool> EnsureAccessTokenValid(IJsonSerializer json, IHttpClient http)
        {
            //if (DateTime.Compare(ExpirationTime, DateTime.Now) >= 0)
            //{
            //    return true;
            //}

            //try
            //{
            //    var (token, error) = await OAuthHelper.RefreshToken(json, http, RefreshToken);
            //    if (error != null)
            //    {
            //        foreach (var config in Plugin.Instance.Configuration.Users)
            //        {
            //            if (config.Id.Equals(Id)) Plugin.Instance.Configuration.RemoveUser(config);
            //        }
            //        return false;
            //    }
            //    AccessToken = token.access_token;
            //    ExpirationTime = DateTime.Now.AddSeconds(token.expires_in);
            //    return true;
            //}
            //catch (Exception e)
            //{
            //    return false;
            //}
            // TODO: fix this shit.
            // for some reason refresh_token not working.
            // returns {"error":"invalid_grant","error_description":"Invalid refresh token"}
            return true;
        }
    }
}
