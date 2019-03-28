using System;
using System.Configuration.Install;
using System.Net.Http;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace IPTVTuner
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            // Install or uninstall the service when invoked interactively.
            string parameter = string.Concat(args);
            switch (parameter)
            {
                case "--install":
                    ManagedInstallerClass.InstallHelper(new[] {
                            Assembly.GetExecutingAssembly().Location
                        });
                    break;
                case "--uninstall":
                    ManagedInstallerClass.InstallHelper(new[] {
                            "/u", Assembly.GetExecutingAssembly().Location
                        });
                    break;
                case "--update-epg":
                    // Update the EPG by rebuilding epg.xml.
                    // Use a scheduled task to periodically refresh the EPG data.
                    UpdateEpg().Wait();
                    break;
                default:
                    // Run as a service.
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[] { new Service() };
                    ServiceBase.Run(ServicesToRun);
                    break;
            }            
        }

        /**
         * Invoke the service and ask it to update the EPG.
         */
        static async Task UpdateEpg()
        {
            try
            {
                var config = new Config(null);
                using (HttpClient client = new HttpClient())
                {
                    // Open a stream to the Provider M3U.
                    var res = await client.GetAsync(config.ServerUrl("/update-epg"));
                    if (res.IsSuccessStatusCode)
                    {
                        // EPG update request succeeded.
                        Console.WriteLine("Started an EPG update.");
                    }
                    else
                    {
                        // Something unexpected happened.
                        throw new Exception(String.Format("Unexpected response code {0}.", res.StatusCode));
                    }
                }
            }
            catch (Exception e)
            {
                // There was an error; probably the service is not running.
                Console.WriteLine("Failed to request an EPG update: " + e.Message);
            }
            
        }
    }
}
