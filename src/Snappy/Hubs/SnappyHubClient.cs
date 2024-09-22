using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Snappy.Hubs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using Snappy.Models;
    using Snappy.Models.Yahama;
    using Snappy.Services;

    public class SnappyHubClient : Hub<ISnappyHubClient>
    {
        private readonly ILogger _logger;
        private readonly ISnapcastService _snapcastService;

        public SnappyHubClient(ILogger logger,
            ISnapcastService snapcastService)
        {
            _logger = logger;
            _snapcastService = snapcastService;
        }

        public override Task OnConnectedAsync()
        {
            _logger.Information($"SnappyHubClient. OnConnectedAsync: {base.Context.ConnectionId}");

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception ex)
        {
            _logger.Information($"SnappyHubClient.OnDisconnectedAsync: {base.Context.ConnectionId}");

            return base.OnDisconnectedAsync(ex);
        }

        public async Task AddToGroupAsync(string group)
        {
            _logger.Information($"SnappyHubClient.AddToGroupAsync: {group}");

            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }

        public async Task RemoveFromGroupAsync(string group)
        {
            _logger.Information($"SnappyHubClient.RemoveFromGroupAsync: {group}");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }

        public async Task GlobalMessageAsync(GlobalMessage globalMessaag)
        {
            _logger.Information($"SnappyHubClient.GlobalMessageAsync: {globalMessaag.HostName} - {globalMessaag.Message}");

            await Clients.All.GlobalMessageAsync(globalMessaag);
        }

        public async Task UpdateServerStatusAsync(Snapcast snapcast)
        {
            _logger.Information($"SnappyHubClient.UpdateServerStatusAsync");

            await Clients.All.ServerStatusAsync(snapcast);
        }

        public async Task<List<Group>> GetGroupsAsync()
        {
            _logger.Information($"SnappyHubClient.GetGroupsAsync");

            var snapcast = await _snapcastService.ServerGetStatus();

            var groups = snapcast.Result.Server.Groups;

            return groups;
        }


        public async Task<List<Stream>> GetStreamsAsync()
        {
            _logger.Information($"SnappyHubClient.GetStreamsAsync");

            var snapcast = await _snapcastService.ServerGetStatus();

            return snapcast.Result.Server.Streams;
        }

        public async Task GroupChangedAsync(string id)
        {
            _logger.Information($"SnappyHubClient.GetServerStatusAsync");

            var snapcast = await _snapcastService.ServerGetStatus();

            var group = snapcast.Result.Server.Groups
                .Where(g => g.Id == id)
                .FirstOrDefault();

            await Clients.All.GroupChangedAsync(id, group);
            // await Clients.All.GroupChangedAsync(id, group);

        }

        public async Task NotifyYamahaMainZoneBasicStatusChangedAsync(string group, YamahaAvMainZone yamahaAvMainZone)
        {
            _logger.Information($"SnappyHubClient.NotifyYamahaMainZoneBasicStatusChangedAsync: {group} - {JsonSerializer.Serialize(yamahaAvMainZone)}");

            await Clients.Group(group).NotifyYamahaMainZoneBasicStatusChangedAsync(yamahaAvMainZone);
        }

        public async Task NotifyYamahaZone2BasicStatusChangedAsync(string group, YamahaAvZone2 yamahaAvZone2)
        {
            _logger.Information($"SnappyHubClient.NotifyYamahaZone2BasicStatusChangedAsync: {group} - {JsonSerializer.Serialize(yamahaAvZone2)}");

            await Clients.Group(group).NotifyYamahaZone2BasicStatusChangedAsync(yamahaAvZone2);
        }

        public async Task NotifyYamahaZone3BasicStatusChangedAsync(string group, YamahaAvZone3 yamahaAvZone3)
        {
            _logger.Information($"SnappyHubClient.NotifyYamahaZone3BasicStatusChangedAsync: {group} - {JsonSerializer.Serialize(yamahaAvZone3)}");

            await Clients.Group(group).NotifyYamahaZone3BasicStatusChangedAsync(yamahaAvZone3);
        }
    }
}