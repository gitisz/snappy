
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Serilog;


namespace Snappy.Services
{
    using Snappy.Models;
    using Snappy.Extensions;
    using Snappy.Configuration;

    public interface ISnapcastService
    {
        Task<Snapcast> ServerGetStatus();
        Task GroupSetName(string id, string name);
        Task GroupSetStream(string id, string streamId);
        Task GroupSetMute(string id, bool mute);
        Task ClientSetVolume(string id, int percent, bool muted);
        Task GroupSetClients(string id, string[] clientIds);
    }

    public class SnapcastService : ISnapcastService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public SnapcastService(
            ILogger logger,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;

            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
            };
        }

        public async Task<Snapcast> ServerGetStatus()
        {
            try
            {
                Snapcast snapcast;

                dynamic content = new
                {
                    @id = 8,
                    @jsonrpc = "2.0",
                    @method = "Server.GetStatus"
                };

                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(content));

                var response = await _httpClient.PostAsync("", httpContent);
                response.EnsureSuccessStatusCode();
                using(var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    snapcast = await JsonSerializer.DeserializeAsync<Snapcast>(responseStream, _options);
                    var groups = snapcast.Result.Server.Groups
                        .Select(g => {
                            if(string.IsNullOrWhiteSpace(g.Name))
                            {
                                g.Name = parseClientName(g.Clients.FirstOrDefault().Host.Name);
                            }
                            g.GroupVol = (int)Math.Round((decimal)g.Clients.Sum(c => c.Config.Volume.Percent) / g.Clients.Count, 0);
                            g.Clients = g.Clients.Select(c => {
                                c.Name = parseClientName(c.Host.Name);
                                return c;
                            }).ToList();
                            return g;
                        })
                        .ToList();
                    snapcast.Result.Server.Groups = groups;
                }

                return snapcast;
            }
            catch (System.Exception ex)
            {
                _logger.Error("ServerGetStatus", ex);

                throw;
            }
        }

        public async Task GroupSetName(string id, string name)
        {
            try
            {
                dynamic content = new
                {
                    @id = 8,
                    @jsonrpc = "2.0",
                    @method = "Group.SetName",
                    @params = new
                    {
                        @id = id,
                        @name = name
                    }
                };

                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(content));

                var response = await _httpClient.PostAsync("", httpContent);
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception ex)
            {
                _logger.Error("GroupSetName", ex);

                throw;
            }
        }


        public async Task GroupSetStream(string id, string streamId)
        {
            try
            {
                dynamic content = new
                {
                    @id = 8,
                    @jsonrpc = "2.0",
                    @method = "Group.SetStream",
                    @params = new
                    {
                        @id = id,
                        @stream_id = streamId
                    }
                };

                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(content));

                var response = await _httpClient.PostAsync("", httpContent);
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception ex)
            {
                _logger.Error("GroupSetStream", ex);

                throw;
            }
        }

        public async Task GroupSetMute(string id, bool muted)
        {
            try
            {
                dynamic content = new
                {
                    @id = 8,
                    @jsonrpc = "2.0",
                    @method = "Group.SetMute",
                    @params = new
                    {
                        @id = id,
                        @mute = muted
                    }
                };

                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(content));

                var response = await _httpClient.PostAsync("", httpContent);
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception ex)
            {
                _logger.Error("GroupSetMute", ex);

                throw;
            }
        }

        public async Task GroupSetClients(string id, string [] clientIds)
        {
            try
            {
                dynamic content = new
                {
                    @id = 8,
                    @jsonrpc = "2.0",
                    @method = "Group.SetClients",
                    @params = new
                    {
                        @id = id,
                        @clients = clientIds
                    }
                };

                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(content));

                var response = await _httpClient.PostAsync("", httpContent);
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception ex)
            {
                _logger.Error("GroupSetClients", ex);

                throw;
            }
        }

        public async Task ClientSetVolume(string id, int percent, bool muted)
        {
            try
            {
                dynamic content = new
                {
                    @id = 8,
                    @jsonrpc = "2.0",
                    @method = "Client.SetVolume",
                    @params = new
                    {
                        @id = id,
                        @volume = new {
                            @muted = muted,
                            @percent = percent
                        }
                    }
                };

                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(content));

                var response = await _httpClient.PostAsync("", httpContent);
                response.EnsureSuccessStatusCode();
            }
            catch (System.Exception ex)
            {
                _logger.Error("ClientSetVolume", ex);

                throw;
            }
        }

        private string parseClientName(string name)
        {
            if(name.StartsWith("snapclient"))
            {
                name = name.Substring(11).ToShoutCase();
            }
            return name;
        }
    }
}