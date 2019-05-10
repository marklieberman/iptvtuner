using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace IPTVTuner.Handlers
{
    class DiscoverHandler : MyHandler, IHttpRequestHandler
    {
        private readonly Config config;

        public DiscoverHandler(Config config)
        {
            this.config = config;
        }
        
        public Task Handle(IHttpContext context, Func<Task> next)
        {
            context.Response = JsonResponse(context, new
            {
                FriendlyName = "IPTVTuner",
                Manufacturer = "Silicondust",
                ModelNumber = "HDTC-2US",
                FirmwareName = "hdhomeruntc_atsc",
                FirmwareVersion = "20150826",
                TunerCount = 1,
                DeviceID = "12345678",
                DeviceAuth = "telly123",
                BaseURL = config.ServerUrl(),
                LineupURL = config.ServerUrl("/lineup.json")
            });

            return Task.Factory.GetCompleted();
        }
    }
}
