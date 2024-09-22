using System.Collections.Generic;

namespace Snappy.Configuration
{
    public class SpotifyConfiguration
    {
        public List<StreamSource> Sources { get; set; }
    }

    public class StreamSource
    {
        public string Source { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string DeviceId { get; set; }
    }
}