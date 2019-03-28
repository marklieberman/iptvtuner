using IPTVTuner.Model;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace IPTVTuner
{
    public partial class Service : ServiceBase
    {
        private Config config;
        private Lineup lineup;
        private Updater updater;
        private Server server;

        public Service()
        {
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args)
        {
            #if DEBUG
            // Debug builds wait for a debugger to attach.
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }
            #endif

            try
            {
                // Initialize the application.
                config = new Config(EventLog);
                lineup = new Lineup();
                updater = new Updater(config, lineup);
                server = new Server(config, lineup, updater);

                // Begin a lineup/EPG update from the provider.
                var discard = updater.Update();

                // Start the HTTP server.
                server.Start();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(string.Format("Failed to start: {0}", e.Message), EventLogEntryType.Error);
                Stop();
            }            
        }

        protected override void OnStop()
        {
            if (server != null)
            {
                server.Stop();
            }
        }
    }
}
