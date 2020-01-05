using System;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using MyShows.MyShowsApi.Api18;
using MyShows.MyShowsApi.Api20;

namespace MyShows.MyShowsApi
{
    public enum MyShowsApiVersion
    {
        V18,
        V20
    }

    internal class MyShowsApiFactory
    {
        private readonly IMyShowsApi _api18;
        private readonly IMyShowsApi _api20;

        public MyShowsApiFactory(ILogger logger, IJsonSerializer json, IHttpClient httpClient)
        {
            _api18 = new MyShowsApi18(logger, json, httpClient);
            _api20 = new MyShowsApi20(logger, json, httpClient);
        }

        public IMyShowsApi GetApi(MyShowsApiVersion version)
        {
            switch (version)
            {
                case MyShowsApiVersion.V18:
                    return _api18;

                case MyShowsApiVersion.V20:
                    return _api20;

                default:
                    throw new Exception("Unknown API version");
            }
        }
    }
}
