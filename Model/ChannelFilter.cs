using IPTVTuner.Model;
using System;
using System.Text.RegularExpressions;

namespace IPTVTuner
{
    class ChannelFilter
    {
        private readonly Regex regex;

        public ChannelFilter(Config config)
        {
            regex = new Regex(config.Filter, RegexOptions.IgnoreCase);
        }

        public Boolean Predicate(ProviderChannel entry)
        {
            if (String.IsNullOrEmpty(entry.Group))
            {
                return false;
            }

            return regex.IsMatch(entry.Group);
        }
    }
}
