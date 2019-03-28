using IPTVTuner.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace IPTVTuner
{
    /**
     * Build an EPG in XMLTV format combining the local lineup and provider supplied XML. 
     */
    class EPGBuilder
    {
        private readonly Lineup lineup;
        private readonly XDocument doc;
        private readonly Dictionary<string, XElement> epgChannels;

        public EPGBuilder(Config config, Lineup lineup)
        {
            this.lineup = lineup;

            // Prepare a blank XML document for the EPG.
            doc = new XDocument(new XElement("tv"));
            
            epgChannels = new Dictionary<string,XElement>();
        }

        /**
         * Create a channel element in the EPG for each local channel.
         */
        public void CreateChannels(ProviderChannels channels)
        {
            channels.ForEach(channel =>
            {
                // Find the local channel number associated with the channel ID.
                var channelNumber = lineup.Channels.Find(c => c.ID.Equals(channel.ID)).ChannelNumber;
                
                // Ignore channels with duplicate IDs.
                if (!epgChannels.ContainsKey(channel.ID))
                {
                    var channelNode = new XElement("channel", new XAttribute("id", channel.ID),
                        new XElement("display-name", channel.Name),
                        new XElement("lcn", channelNumber));

                    // Add a logo if one is available.
                    if (!String.IsNullOrEmpty(channel.Logo))
                    {
                        channelNode.Add(new XElement("icon", new XAttribute("src", channel.Logo)));
                    }

                    // Remember that we've seen this channel ID.
                    epgChannels.Add(channel.ID, channelNode);

                    // Add this channel to the guide.
                    doc.Root.Add(channelNode);
                }
            });
        }

        /**
         * Merge XMLTV data from the provider into the local EPG.
         */
        public void MergeXMLTV(StreamReader stream)
        {
            var epgDoc = XDocument.Load(stream);

            // Merge channel metadata from the provider XML.
            // TODO Not implemented yet.

            // Add all program elements that reference a channel in the lineup.
            foreach (var programme in epgDoc.Descendants("programme"))
            {
                var channelId = programme.Attribute("channel").Value;
                if (epgChannels.ContainsKey(channelId))
                {
                    doc.Root.Add(programme);
                }
            }            
        }

        /**
         * Write the EPG to epg.xml on disk.
         */
        public void WriteToDisk(string path)
        {
            doc.Save(path);
        }
    }
}
