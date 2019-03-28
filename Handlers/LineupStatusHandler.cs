using System;
using System.IO;
using System.Threading.Tasks;
using uhttpsharp;

namespace IPTVTuner.Handlers
{
    /**
     * Mocks the lineup status to allow the tuner to be installed.
     */
    class LineupStatusHandler : MyHandler, IHttpRequestHandler
    {
        private readonly Config config;

        public LineupStatusHandler(Config config)
        {
            this.config = config;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            // Consider the server to be scanning as long as the lineup does not exist.
            var scanning = !File.Exists(Path.Combine(config.DataPath, "lineup.json"));

            context.Response = JsonResponse(context, new
            {
                ScanInProgress = scanning,
                ScanPossible = true,
                Source = "Cable",
                SourceList = new string[] { "Cable" },
                Progress = 1,
                Found = 1
            });

            return Task.Factory.GetCompleted();
        }
    }
}
