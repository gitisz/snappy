
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
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using Zeroconf;
    using Snappy.Models;

    public interface ILibrespotService
    {
        public Task<LibrespotInfo> GetInfoAsync(int port);
        public Task<IReadOnlyList<IZeroconfHost>> GetSpotifyZeroConfHostsAsync();
    }

    public class LibrespotService : ILibrespotService
    {
        private readonly ILogger _logger;

        private readonly IOptions<LibrespotConfiguration> _librespotConfiguration;
        private readonly IOptions<SpotifyConfiguration> _spotifyConfiguration;

        private readonly JsonSerializerOptions _options;

        public LibrespotService(
            ILogger logger,
            IOptions<LibrespotConfiguration> librespotConfiguration,
            IOptions<SpotifyConfiguration> spotifyConfiguration
            )
        {
            _logger = logger;
            _librespotConfiguration = librespotConfiguration;
            _spotifyConfiguration = spotifyConfiguration;

             _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<LibrespotInfo> GetInfoAsync(int port)
        {
            LibrespotInfo librespotInfo = null;

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync($"http://{_librespotConfiguration.Value.Url}:{port}/?action=getInfo&version=2.7.1");
                    response.EnsureSuccessStatusCode();
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        librespotInfo = await JsonSerializer.DeserializeAsync<LibrespotInfo>(responseStream, _options);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("GetInfo", ex);

                    throw ex;
                }
            }

            return librespotInfo;
        }

        public async Task<LibrespotInfo> AddUserAsync(LibrespotInfo librespotInfo, string source, string userName, int port)
        {

            var librespotSource = _spotifyConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .FirstOrDefault();

            var blob = "";
            var clientId = _librespotConfiguration.Value.ClientId;
            var loginId = "";
            var deviceName = source;
            var deviceId = librespotSource.DeviceId;

            using (var client = new HttpClient())
            {
                try
                {
                    dynamic content = new
                    {
                        @action = "addUser",
                        @userName = userName,
                        @blob = blob,
                        @clientId = clientId,
                        @loginId = loginId,
                        @deviceName = deviceName,
                        @deviceId = deviceId,
                        @version = _librespotConfiguration.Value.Version,
                    };

                    HttpContent httpContent = new StringContent(JsonSerializer.Serialize(content));

                    var response = await client.PostAsync($"http://{_librespotConfiguration.Value.Url}:{port}/", httpContent);
                    response.EnsureSuccessStatusCode();
                    var responseAsString = response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error("GetInfo", ex);

                    throw ex;
                }
            }

            return librespotInfo;
        }


        public async Task<IReadOnlyList<IZeroconfHost>> GetSpotifyZeroConfHostsAsync()
        {

            IReadOnlyList<IZeroconfHost> results = await
                ZeroconfResolver.ResolveAsync("_spotify-connect._tcp.local.");

            return results;
        }
    }
}