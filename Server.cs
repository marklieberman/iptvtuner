using IPTVTuner.Handlers;
using IPTVTuner.Model;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace IPTVTuner
{
    class Server
    {
        private readonly Config config;
        private readonly Lineup lineup;
        private readonly Updater updater;

        private HttpServer httpServer;

        public Server(Config config, Lineup lineup, Updater updater)
        {
            this.config = config;
            this.lineup = lineup;
            this.updater = updater;
        }
        
        /**
         * Start the HTTP server.
         */
        public void Start()
        {
            httpServer = new HttpServer(new HttpRequestProvider());

            // Listen on the configured interface and port.
            var ip = IPAddress.Parse(config.IpAddress);
            httpServer.Use(new TcpListenerAdapter(new TcpListener(ip, config.Port)));

            // Add discovery, lineup, and EPG routes.
            httpServer.Use(new HttpRouter()
                // Routes used to emulate a tuner for Plex.
                .With("discover.json", new DiscoverHandler(config))
                .With("lineup_status.json", new LineupStatusHandler(config))
                .With("lineup.json", new LineupHandler(config))
                .With("epg.xml", new EPGXMLHandler(config))
                // Admin routes not used for Plex tuner emulation.
                .With("update-epg", new UpdateHandler(updater)));

            // Match routes that need more than exact match.
            var serveHandler = new ServeHandler(config, lineup);
            var epgIconHandler = new EPGLogoHandler(config, lineup);
            httpServer.Use((context, next) =>
            {
                // Handle calls to begin streaming a channel.
                // This is paths that begin with /auto/.
                var path = context.Request.Uri.ToString();
                if (path.StartsWith("/auto/"))
                {
                    return serveHandler.Handle(context, next);
                }

                // Handle calls to begin streaming a channel.
                // This is paths that begin with /auto/.
                if (path.StartsWith("/logo/"))
                {
                    return epgIconHandler.Handle(context, next);
                }

                return next();
            });

            // Serve 404 Not Found for all other requests.
            httpServer.Use((context, next) =>
            {
                context.Response = new HttpResponse(HttpResponseCode.NotFound, string.Empty, false);
                return Task.Factory.GetCompleted();
            });

            // Start accepting HTTP connections.
            config.WriteLog(false, "IPTVTuner HTTP server listening at {0}", config.ServerUrl());
            httpServer.Start();
        }

        /**
         *  Stop the HTTP server.
         */
        public void Stop()
        {
            httpServer.Dispose();
            httpServer = null;
        }        
    }
}
