using IPTVTuner.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IPTVTuner
{
    /**
     * Builds the local lineup and EPG data by from the provider data.
     */
    class Updater
    {
        private readonly Config config;
        private readonly Lineup lineup;

        public Updater(Config config, Lineup lineup)
        {
            this.config = config;
            this.lineup = lineup;
        }
        
        /**
         * Update the local lineup and EPG.
         */
        public async Task Update()
        {
            try
            {
                // Update the available channels from.
                var channels = await UpdateChannels();

                // Use the channels to build a local EPG.
                await UpdateEPG(channels);
            }
            catch (Exception e)
            {
                config.WriteLog(true, "Channel update failed: {0}", e.Message);
            }
        }

        /**
         * Download the provider data and build the local lineup.
         */
        public async Task<ProviderChannels> UpdateChannels()
        {
            // Log the start of this long running update process.
            config.WriteLog(false, "Beginning provider channel update for {0}.", config.M3UURL);

            // Obtain the channel list from the provider.
            var channels = await GetChannelsFromM3U();
            
            // Fail if there are too many channels for Plex.
            if (channels.Count() > config.MaxChannels)
            {
                throw new TooManyChannelsException(channels.Count(), config.MaxChannels);
            }

            // Generate IDs for channels without one.
            channels.ForEach(channel => channel.ID = channel.GetOrCreateID());

            // Generate a lineup item for each channel.
            var localChannels = channels.Select((channel, index) =>
            {
                var channelNumber = (config.StartChannel + index).ToString();

                return new HDHomeRunLineupItem
                {
                    GuideName = channel.Name,
                    GuideNumber = channelNumber,
                    HD = channel.IsHD(),

                    // The URL to tune this channel on our proxy service.
                    URL = config.ServerUrl("/auto/" + channelNumber)
                };
            }).ToList();

            // Store the lineup in memory so channels may be served.
            lineup.Channels = channels.Select((channel, index) =>
            {
                var channelNumber = (config.StartChannel + index).ToString();

                return new ChannelQuad
                {
                    ID = channel.ID,
                    Name = channel.Name,
                    ChannelNumber = channelNumber,

                    // The actual URL of the provider's stream.
                    URL = channel.URL
                };
            }).ToList();

            // Serialize the lineup items to lineup.json on disk.
            var outputPath = Path.Combine(config.DataPath, "lineup.json");
            using (StreamWriter writer = new StreamWriter(outputPath, false))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, localChannels);
            }

            // Log the number of channels loaded into the lineup.
            config.WriteLog(false, "Loaded {0} channels into the lineup.", localChannels.Count());

            // Log the end of this long running update process.
            config.WriteLog(false, "Wrote updated local lineup to {0}.", outputPath);

            return channels;
        }

        /**
         * Fetch and filter the M3U from the provider.
         */
        public async Task<ProviderChannels> GetChannelsFromM3U()
        {
            // Prepare a filter to match channels.
            var filter = new ChannelFilter(config);

            // Prepare a parser to parse the M3U.
            var parser = new M3UParser
            {
                Filter = filter.Predicate
            };
            
            using (HttpClient client = new HttpClient())
            {
                // Open a stream to the Provider M3U.
                var res = await client.GetStreamAsync(config.M3UURL);
                var streamReader = new StreamReader(res);

                // Parse the M3U and extract the channels.
                return await parser.Parse(streamReader);
            }
        }

        /**
         * Download the provider EPG data and build the local EPG.
         */
        public async Task UpdateEPG(ProviderChannels channels)
        {
            // Log the start of this long running update process.
            config.WriteLog(false, "Beginning provider EPG update for {0}.", config.EPGURL);

            // Build a skeleton EPG using the channels in the lineup.
            EPGBuilder epg = new EPGBuilder(config, lineup);
            epg.CreateChannels(channels);

            // Fetch and merge the EPG from the provider.
            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetStreamAsync(config.EPGURL);
                var streamReader = new StreamReader(res);
                epg.MergeXMLTV(streamReader);
            }

            // Serialize the EPG items to epg.xml on disk.
            var outputPath = Path.Combine(config.DataPath, "epg.xml");
            epg.WriteToDisk(outputPath);

            // Log the end of this long running update process.
            config.WriteLog(false, "Wrote updated local EPG to {0}.", outputPath);
        }
        
    }
}
