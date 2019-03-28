namespace IPTVTuner.Model
{
    class ProviderChannel
    {
        public string ID;
        public string Name;
        public string Logo;
        public string Group;
        public string URL;
        
        /**
         * Get the ID if known or generate a unique ID by hashing the name.
         */
        public string GetOrCreateID()
        {
            return ID ?? (Name.GetHashCode().ToString("X") + ".generated.tv");
        }

        public bool IsHD()
        {
            return Name.ToUpper().Contains("HD");
        }
    }
}
