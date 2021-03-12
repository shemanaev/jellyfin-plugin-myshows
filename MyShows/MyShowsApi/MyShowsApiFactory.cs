using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
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
        private readonly IMyShowsApi _api20;

        public MyShowsApiFactory(ILogger logger, IHttpClientFactory httpClient)
        {
            _api20 = new MyShowsApi20(logger, httpClient);
        }

        public IMyShowsApi GetApi(MyShowsApiVersion version)
        {
            return version switch
            {
                MyShowsApiVersion.V18 => throw new NotImplementedException(),
                MyShowsApiVersion.V20 => _api20,
                _ => throw new Exception("Unknown API version"),
            };
        }
    }
}
