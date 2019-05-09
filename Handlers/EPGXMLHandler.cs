using System;
using System.IO;
using System.Threading.Tasks;
using uhttpsharp;

namespace IPTVTuner.Handlers
{
    /**
     * Serves epg.xml from the data path.
     */
    class EPGXMLHandler : MyHandler, IHttpRequestHandler
    {
        private readonly Config config;

        public EPGXMLHandler(Config config)
        {
            this.config = config;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            context.Response = XMLResponse(context, File.OpenRead(Path.Combine(config.DataPath, "epg.xml")));

            return Task.Factory.GetCompleted();
        }
    }
}
