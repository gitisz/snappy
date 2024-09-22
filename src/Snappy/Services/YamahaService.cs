
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Xml.Serialization;
using Serilog;


namespace Snappy.Services
{
    using Snappy.Extensions;
    using Snappy.Configuration;
    using Snappy.Models.Yahama;
    using System.IO;
    using System.Xml;

    public interface IYamahaService<T>
    {
        Task<T> BasicStatusAsync(string source, string yamahaUrl);
        Task<T> ConfigAsync(string source, string yamahaUrl);
        Task<T> PowerOnAsync(string source, string yamahaUrl);
        Task<T> PowerOffAsync(string source, string yamahaUrl);
        Task<T> ConfigNameZoneAsync(string source, string yamahaUrl, string name);
        Task<T> VolumeLvlValAsync(string source, string yamahaUrl, decimal volume);
    }

    public class YamahaService<T> : IYamahaService<T> where T : new()
    {
        private readonly ILogger _logger;

        private readonly string _zone;

        public YamahaService(
            ILogger logger)
        {
            _logger = logger;

            switch (this)
            {
                case YamahaService<YamahaAvMainZone> mainZone:
                    _zone = "Main_Zone";
                    break;
                case YamahaService<YamahaAvZone2> zone2:
                    _zone = "Zone_2";
                    break;
                case YamahaService<YamahaAvZone3> zone3:
                    _zone = "Zone_3";
                    break;
            }
        }

        public async Task<T> BasicStatusAsync(string source, string yamahaUrl)
        {
            T yamahaAvZone;

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri($"http://{yamahaUrl}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent($"<YAMAHA_AV cmd=\"GET\"><{_zone}><Basic_Status>GetParam</Basic_Status></{_zone}></YAMAHA_AV>"));
                    var contentAsString = await response.Content.ReadAsStringAsync();
                    yamahaAvZone = contentAsString.XmlDeserializeFromString<T>();
                }
                catch (Exception ex)
                {
                    _logger.Error("BasicStatusAsync", ex);

                    throw ex;
                }
            }

            return yamahaAvZone;
        }

        public async Task<T> ConfigAsync(string source, string yamahaUrl)
        {
            T yamahaAvZone;

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri($"http://{yamahaUrl}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent($"<YAMAHA_AV cmd=\"GET\"><{_zone}><Config>GetParam</Config></{_zone}></YAMAHA_AV>"));
                    var contentAsString = await response.Content.ReadAsStringAsync();
                    yamahaAvZone = contentAsString.XmlDeserializeFromString<T>();
                }
                catch (Exception ex)
                {
                    _logger.Error("ConfigAsync", ex);

                    throw ex;
                }
            }

            return yamahaAvZone;
        }

        public async Task<T> PowerOnAsync(string source, string yamahaUrl)
        {
            T yamahaAvZone;

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri($"http://{yamahaUrl}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent($"<YAMAHA_AV cmd=\"PUT\"><{_zone}><Power_Control><Power>On</Power></Power_Control></{_zone}></YAMAHA_AV>"));
                    var contentAsString = await response.Content.ReadAsStringAsync();
                    yamahaAvZone = contentAsString.XmlDeserializeFromString<T>();
                }
                catch (Exception ex)
                {
                    _logger.Error("PowerOnAsync", ex);

                    throw ex;
                }
            }

            return yamahaAvZone;
        }

        public async Task<T> PowerOffAsync(string source, string yamahaUrl)
        {
            T yamahaAvZone;

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri($"http://{yamahaUrl}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent($"<YAMAHA_AV cmd=\"PUT\"><{_zone}><Power_Control><Power>Standby</Power></Power_Control></{_zone}></YAMAHA_AV>"));
                    var contentAsString = await response.Content.ReadAsStringAsync();
                    yamahaAvZone = contentAsString.XmlDeserializeFromString<T>();
                }
                catch (Exception ex)
                {
                    _logger.Error("PowerOffAsync", ex);

                    throw ex;
                }
            }

            return yamahaAvZone;
        }

        public async Task<T> ConfigNameZoneAsync(string source, string yamahaUrl, string name)
        {
            T yamahaAvZone;

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri($"http://{yamahaUrl}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent($"<YAMAHA_AV cmd=\"PUT\"><{_zone}><Config><Name><Zone>{name}</Zone></Name></Config></{_zone}></YAMAHA_AV>"));
                    var contentAsString = await response.Content.ReadAsStringAsync();
                    yamahaAvZone = contentAsString.XmlDeserializeFromString<T>();
                }
                catch (Exception ex)
                {
                    _logger.Error("ConfigNameZoneAsync", ex);

                    throw ex;
                }
            }

            return yamahaAvZone;
        }

        public async Task<T> VolumeLvlValAsync(string source, string yamahaUrl, decimal volume)
        {
            T yamahaAvZone = new T();

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri($"http://{yamahaUrl}/");
                    var response = await client.PostAsync("YamahaRemoteControl/ctrl", new StringContent($"<YAMAHA_AV cmd=\"PUT\"><{_zone}><Volume><Lvl><Val>{volume.Round(5m)}</Val><Exp>1</Exp><Unit>dB</Unit></Lvl></Volume></{_zone}></YAMAHA_AV>"));
                    var contentAsString = await response.Content.ReadAsStringAsync();
                    yamahaAvZone = contentAsString.XmlDeserializeFromString<T>();
                }
                catch (Exception ex)
                {
                    _logger.Error("VolumeLvlValAsync", ex);

                    throw ex;
                }
            }

            return yamahaAvZone;
        }


    }
}