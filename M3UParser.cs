using IPTVTuner.Model;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPTVTuner
{
    /**
     * A streaming M3UEXT parser with filtering.
     */
    partial class M3UParser
    {
        private readonly Regex idRegex;
        private readonly Regex nameRegex;
        private readonly Regex logoRegex;
        private readonly Regex groupRegex;

        public M3UParser()
        {
            // TODO Support custom attribute names in configuration.
            idRegex = new Regex("tvg-id=\"([^\"]+)\"", RegexOptions.IgnoreCase);
            nameRegex = new Regex("tvg-name=\"([^\"]+)\"", RegexOptions.IgnoreCase);
            logoRegex = new Regex("tvg-logo=\"([^\"]+)\"", RegexOptions.IgnoreCase);
            groupRegex = new Regex("group-title=\"([^\"]+)\"", RegexOptions.IgnoreCase);
        }

        /**
         * Filter function to remove channels from the M3U.
         */
        public Func<ProviderChannel, bool> Filter
        {
            get; 
            set;
        }

        /**
         * Parse an M3U stream with a filter to remove unwanted entries.
         */
        public async Task<ProviderChannels> Parse(StreamReader stream)
        {
            var currentEntry = new ProviderChannel();
            var m3u = new ProviderChannels();

            using (stream)
            {
                string line;
                while ((line = await stream.ReadLineAsync()) != null)
                {
                    if (line.StartsWith("#EXTM3U"))
                    {
                        // Ignore the header.
                    }
                    else
                    if (line.StartsWith("#EXTINF"))
                    {
                        // Populate the entry ID.
                        currentEntry.ID = extractAttribute(idRegex, line);

                        // Populate the entry name.
                        currentEntry.Name = extractAttribute(nameRegex, line);

                        if (logoRegex != null)
                        {
                            // Populate the entry logo.
                            currentEntry.Logo = extractAttribute(logoRegex, line);
                        }

                        if (groupRegex != null)
                        {
                            // Populate the entry group.
                            currentEntry.Group = extractAttribute(groupRegex, line);
                        }
                    }
                    else
                    if (line.StartsWith("#"))
                    {
                        // Ignore comments.                    
                    }
                    else
                    {
                        // Populate the entry URL.
                        currentEntry.URL = line;

                        // Filter entries from the list.
                        if ((Filter == null) || Filter.Invoke(currentEntry))
                        {
                            // Add the entry to the list.
                            m3u.Add(currentEntry);
                        }

                        // Moving to the next entry.
                        currentEntry = new ProviderChannel();
                    }
                }
            }

            return m3u;
        }

        /**
         * Extract an attribute value from the extended info.
         */
        private string extractAttribute(Regex regex, string line)
        {
            var match = regex.Match(line);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
