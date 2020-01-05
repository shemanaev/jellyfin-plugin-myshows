using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MyShows.Configuration;
using System;
using System.Collections.Generic;

namespace MyShows
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public override string Name => "MyShows";

        public override string Description => "Scrobble your watched shows with MyShows.me";

        public override Guid Id => Guid.Parse("ef35f6b1-7fe6-44ca-a215-232089fb9bc7");

        public Plugin(IApplicationPaths appPaths, IXmlSerializer xmlSerializer) : base(appPaths, xmlSerializer)
        {
            Instance = this;
            //PollingTasks = new Dictionary<string, Task<bool>>();
        }

        public static Plugin Instance { get; private set; }

        public PluginConfiguration PluginConfiguration => Configuration;

        //public Dictionary<string, Task<bool>> PollingTasks { get; set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = string.Format("{0}.Configuration.configPage.html", GetType().Namespace)
                }
            };
        }
    }
}
