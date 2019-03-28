using System;
using System.IO;
using System.Threading.Tasks;
using uhttpsharp;

namespace IPTVTuner.Handlers
{
    /**
     * Serves lineup.json from the data path.
     */
    class LineupHandler : MyHandler, IHttpRequestHandler
    {
        private readonly Config config;

        public LineupHandler(Config config)
        {
            this.config = config;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            context.Response = JsonResponse(context, File.OpenRead(Path.Combine(config.DataPath, "lineup.json")));
            
            return Task.Factory.GetCompleted();
        }
    }
}
