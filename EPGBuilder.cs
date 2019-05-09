using IPTVTuner.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace IPTVTuner
{
    /**
     * Build an EPG in XMLTV format combining the local lineup and provider supplied XML. 
     */
    class EPGBuilder
    {

        private readonly IFormatProvider xmltvFormat = new DateTimeFormatInfo
        {
            TimeSeparator = string.Empty
        };

        private readonly Config config;
        private readonly Lineup lineup;
        private readonly XDocument doc;
        private readonly Dictionary<string, XElement> epgChannels;
        private readonly Dictionary<string, List<XElement>> epgProgrammes;

        public EPGBuilder(Config config, Lineup lineup)
        {
            this.config = config;
            this.lineup = lineup;

            // Prepare a blank XML document for the EPG.
            doc = new XDocument(new XElement("tv"));
            
            epgChannels = new Dictionary<string,XElement>();
            epgProgrammes = new Dictionary<string, List<XElement>>();
        }

        private DateTime parseXMLDate (string value)
        {
            DateTime date;

            if (DateTime.TryParseExact(value, "yyyyMMddHHmmss zzz", xmltvFormat, DateTimeStyles.None, out date))
            {
                return date;
            };

            if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", xmltvFormat, DateTimeStyles.None, out date))
            {
                return date;
            };

            if (DateTime.TryParseExact(value, "yyyyMMdd", xmltvFormat, DateTimeStyles.None, out date))
            {
                return date;
            };

            return DateTime.MinValue;
        }

        private string formatXMLDate (DateTime date)
        {
            return date
                .ToString("yyyyMMddHHmmss zzz", xmltvFormat)
                .Replace(":", string.Empty);
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

                    // Stub a programme list for this channel.
                    epgProgrammes.Add(channel.ID, new List<XElement>());
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

            // Collect all programmes into a list by channel.
            foreach (var programme in epgDoc.Descendants("programme"))
            {
                var channelId = programme.Attribute("channel").Value;
                if (epgProgrammes.TryGetValue(channelId, out List<XElement> programmes))
                {
                    programmes.Add(programme);
                }
            }

            // Process the programmes for each channel.
            foreach (var channel in epgProgrammes.Keys)
            {
                var programmes = epgProgrammes[channel];

                // Sort the existing programmes by start time.
                programmes.Sort((v1, v2) =>
                {
                    var v1Start = parseXMLDate(v1.Attribute("start").Value);
                    var v2Start = parseXMLDate(v2.Attribute("start").Value);
                    return v1Start.CompareTo(v2Start);
                });

                if (config.GapFill)
                {
                    // Ensure every channel has programmes even when data is not available.
                    fillProgrammeGaps(channel, programmes, DateTime.Today.AddDays(2));
                }

                // Add all of the program elements to the document.
                foreach (var programme in programmes)
                {
                    doc.Root.Add(programme);
                }
            }
        }

        private DateTime roundUpHalfHour (DateTime value)
        {
            if (value.Minute < 30)
            {
                return value
                    .AddMinutes(30 - value.Minute)
                    .AddSeconds(-value.Second);
            }
            else
            {
                return value
                    .AddMinutes(60 - value.Minute)
                    .AddSeconds(-value.Second);
            }
        }

        private DateTime getLastProgrammeStop (List<XElement> programmes)
        {
            if (programmes.Count > 0)
            {
                var lastElement = programmes[programmes.Count - 1];
                return parseXMLDate(lastElement.Attribute("stop").Value);
            }
            else
            {
                var now = DateTime.Now;
                return now
                    .AddHours(-1)
                    .AddMinutes(-now.Minute)                    
                    .AddSeconds(-now.Second);
            }
        }

        private XElement getNoDataProgramme(string channelId, DateTime start, DateTime stop)
        {
            return new XElement("programme",
                new XAttribute("channel", channelId),
                new XAttribute("start", formatXMLDate(start)),
                new XAttribute("stop", formatXMLDate(stop)),
                new XElement("title")
                {
                    Value = "Missing Data"
                },
                new XElement("desc"));
        }

        private void fillProgrammeGaps (String channel, List<XElement> programmes, DateTime end)
        {
            // TODO Fill gaps between episodes.

            // Prepare initial values for the fill operation.
            var start = getLastProgrammeStop(programmes);
            var stop = roundUpHalfHour(start);
            
            // Fill the remaining time until the end date.
            while (stop < end)
            {
                programmes.Add(getNoDataProgramme(channel, start, stop));

                // Next time slice.
                start = stop;
                stop = start.AddMinutes(30);
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
