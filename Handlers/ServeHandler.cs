using IPTVTuner.Model;
using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace IPTVTuner.Handlers
{
    /**
     * Redirects requests for a channel to the underlying provider stream.
     */
    class ServeHandler : MyHandler, IHttpRequestHandler
    {
        private readonly Config config;
        private readonly Lineup lineup;

        public ServeHandler(Config config, Lineup lineup)
        {
            this.config = config;
            this.lineup = lineup;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            // Extract the channel number from the request.
            var path = context.Request.Uri.ToString();
            var channelNumber = path.Substring(path.LastIndexOf("/") + 1);

            // Find the actual URL of the stream for the channel.
            var tuning = lineup.Channels.Find(channel => channel.ChannelNumber.Equals(channelNumber));
            if (tuning != null)
            {
                // Redirect Plex to the stream.                
                context.Response = Found(tuning.URL);
                config.WriteLog(false, "Serving channel {0} ({1}) from {2} to {3}", tuning.ChannelNumber, tuning.Name, tuning.URL, context.RemoteEndPoint);
            }
            else
            {
                // Channel not found.
                context.Response = NotFound();
            }
            
            return Task.Factory.GetCompleted();
        }
    }
}
