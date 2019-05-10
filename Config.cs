using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace IPTVTuner
{
    class Config
    {
        private readonly EventLog eventLog;

        private string ipAddress;
        private int port;
        private int startChannel;
        private string filter;
        private string m3uUrl;
        private string epgUrl;
        private string logoFontFamily;
        private uint logoColor;
        private uint logoBackground;
        private int gapFillAmount;
        private string gapFillTitle;

        public Config(EventLog eventLog) {
            this.eventLog = eventLog;
            Read();
        }

        public void Read()
        {
            // The configuration registry key should exist.
            using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (var reg = root.OpenSubKey(@"SOFTWARE\IPTVTuner")) {
                    // Ensure that the registry key exists.
                    if (reg == null)
                    {
                        throw new Exception(@"Configuration at HKLM\SOFTWARE\IPTVTuner does not exist");
                    }

                    // Get the M3U URL from the registry.
                    this.m3uUrl = (string)reg.GetValue("M3UURL");
                    if (String.IsNullOrEmpty(this.m3uUrl))
                    {
                        throw new Exception("M3U URL not provided");
                    }

                    // Get the EPG URL from the registry.
                    this.epgUrl = (string)reg.GetValue("EPGURL");
                    if (String.IsNullOrEmpty(this.epgUrl))
                    {
                        throw new Exception("EPG URL not provided");
                    }

                    // Read all values that provide default values.
                    this.ipAddress = (string)reg.GetValue("IpAddress", "127.0.0.1");
                    this.port = (int)reg.GetValue("Port", 6079);
                    this.startChannel = (int)reg.GetValue("StartChannel", 1);
                    this.filter = (string)reg.GetValue("Filter", ".*");
                    this.logoFontFamily = (string)reg.GetValue("LogoFontFamily", "Segoe UI");
                    this.logoColor = (uint)reg.GetValue("LogoColor", 0xFFDCDCDC);
                    this.logoBackground = (uint)reg.GetValue("LogoBackground", (uint)1);
                    this.gapFillAmount = (int)reg.GetValue("GapFillAmount", 0);
                    this.gapFillTitle = (string)reg.GetValue("GapFillTitle", "Unknown Airing");

                }
            }
        }

        public string DataPath
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
        }

        public string IpAddress
        {
            get { return ipAddress; }
        }

        public int Port
        {
            get { return port; }
        }

        public string M3UURL
        {
            get { return m3uUrl; }
        }

        public string EPGURL
        {
            get { return epgUrl; }
        }

        public string Filter
        {
            get { return filter; }
        }

        public int StartChannel
        {
            get { return startChannel; }
        }

        public int MaxChannels
        {
            get { return 420; }
        }

        public string LogoFontFamily
        {
            get { return logoFontFamily; }
        }

        public uint LogoColor
        {
            get { return logoColor; }
        }

        public uint LogoBackground
        {
            get { return logoBackground; }
        }

        public int GapFillAmount
        {
            get { return gapFillAmount; }
        }

        public string GapFillTitle
        {
            get { return gapFillTitle; }
        }

        /**
         * Write an entry in the event log and console if available. 
         */
        public void WriteLog(bool error, string format, params object[] args)
        {
            var message = String.Format(format, args);
            Console.WriteLine(message);
            if (eventLog != null)
            {
                eventLog.WriteEntry(message, error ? EventLogEntryType.Error : EventLogEntryType.Information);
            }
        }

        /**
         * Format a URL that points to the IPTV HTTP server. 
         */
        public string ServerUrl(string path = "")
        {
            return String.Format("http://{0}:{1}{2}", IpAddress, Port, path);
        }
    }
}
