using System.Net.Http;

namespace Snappy.Models
{
    public class GlobalMessage : HttpResponseMessage
    {
        public string HostName {get ; set; } = System.Net.Dns.GetHostName();
        public string Message {get ; set; }
    }
}