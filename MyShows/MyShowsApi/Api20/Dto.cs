namespace MyShows.MyShowsApi.Api20
{
    public class JsonRpcCall
    {
        public string jsonrpc { get; set; }
        public string method { get; set; }
        public int id { get; set; }
        public object @params { get; set; }
    }

    public class JsonRpcResult<T>
    {
        public string jsonrpc { get; set; }
        public T result { get; set; }
        public int id { get; set; }
        public JsonRpcError error { get; set; }
    }

    public class JsonRpcError
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class ShowsGetByIdArgs
    {
        public int showId { get; set; }
        public bool withEpisodes { get; set; }
    }

    public class ShowsGetByExternalIdArgs
    {
        public int id { get; set; }
        public string source { get; set; }
    }

    public class ManageSetShowStatusArgs
    {
        public int id { get; set; }
        public string status { get; set; }
    }

    public class ManageEpisodeArgs
    {
        public int id { get; set; }
    }

    public class ManageSyncEpisodesDeltaArgs
    {
        public int showId { get; set; }
        public int[] checkedIds { get; set; }
        public int[] unCheckedIds { get; set; }
    }

    public class ShowSummary
    {
        public int id { get; set; }
        public string title { get; set; }
        public string titleOriginal { get; set; }
        public string status { get; set; }
        public EpisodeSummary[] episodes { get; set; }
    }

    public class EpisodeSummary
    {
        public int id { get; set; }
        public int seasonNumber { get; set; }
        public int episodeNumber { get; set; }
    }

    //public class SuccessAnswer
    //{
    //    public bool result { get; set; }
    //}
}
