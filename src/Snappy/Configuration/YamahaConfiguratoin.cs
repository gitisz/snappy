using System.Collections.Generic;

namespace Snappy.Configuration
{
    public class YamahaConfiguration
    {
        public List<ReceiverSource> Sources { get; set; }
    }

    public class ReceiverSource
    {
        public string Source { get; set; }
        public string Url { get; set; }
    }

}