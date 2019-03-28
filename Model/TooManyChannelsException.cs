using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVTuner
{
    class TooManyChannelsException : Exception
    {
        public TooManyChannelsException(int count, int max) : base(String.Format("Too many channels: {0} > {1} - Plex is unlikely to work", count, max))
        {
        }
    }
}
