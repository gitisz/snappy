using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using StreamJsonRpc;

namespace Snappy.Socket.Services
{
    using Snappy.Configuration;
    using Snappy.Models;
    using Snappy.Services;

    public class SnappySocketService : BackgroundService
    {
        private readonly ILogger _logger;

        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        private IOptions<HostConfiguration> _hostConfiguration { get; }

        private readonly ISnapcastService _snapcastService;

        protected HubConnection _backChannelHub;

        protected ClientWebSocket _webSocket;

        protected CancellationTokenSource _cancellationTokenSource;

        protected string _hubProtocol
        {
            get
            {
                string hproto = System.Environment.GetEnvironmentVariable("HUB_PROTOCOL");

                if (string.IsNullOrEmpty(hproto))
                    hproto = _hostConfiguration.Value.HubProtocol;

                return hproto;
            }
        }

        protected string _hubTarget
        {
            get
            {
                string htarget = System.Environment.GetEnvironmentVariable("HUB_TARGET");

                if (string.IsNullOrEmpty(htarget))
                    htarget = _hostConfiguration.Value.HubTarget;

                return htarget;
            }
        }

        protected string _hubPort
        {
            get
            {
                string hport = System.Environment.GetEnvironmentVariable("HUB_PORT");

                if (string.IsNullOrEmpty(hport))
                    hport = _hostConfiguration.Value.HubPort;

                return hport;
            }
        }

        protected string _hubPath
        {
            get
            {
                string hpath = System.Environment.GetEnvironmentVariable("HUB_PATH");

                if (string.IsNullOrEmpty(hpath))
                    hpath = _hostConfiguration.Value.HubPath;

                return hpath;
            }
        }

        public SnappySocketService(
            ILogger logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IOptions<HostConfiguration> hostConfiguration,
            ISnapcastService snapcastService
        )
        {
            _logger = logger;
            _hostConfiguration = hostConfiguration;
            _hostApplicationLifetime = hostApplicationLifetime;
            _snapcastService = snapcastService;

            initializeSignalRHub();
            initializeJsonRpcHub();
        }

        private void initializeSignalRHub()
        {
            var hubPath = "http://localhost:5001/snappy/hub";
            if (!string.IsNullOrEmpty(_hubPath))
                hubPath = $"{_hubProtocol}://{_hubTarget}:{_hubPort}/{_hubPath}/hub";

            _backChannelHub = new HubConnectionBuilder()
                .WithUrl(hubPath, options =>
                {
                    options.UseDefaultCredentials = true;
                    options.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        return message;
                    };
                })
                .WithAutomaticReconnect()
                .Build();
        }

        private void initializeJsonRpcHub()
        {

            _webSocket = new ClientWebSocket();
        }

        public override void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                try
                {
                    _logger.Information($"Starting SignalR connection: https://localhost:5001/snappy/hub");

                    await _backChannelHub.StartAsync(_cancellationTokenSource.Token);

                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to start Hub connection: {ex}");
                    await Task.Delay(1000);
                }
            }

            while (true)
            {
                try
                {
                    _logger.Information($"Starting JsonRpc connection: ws://snapserver-direct.iszland.com:1780/jsonrpc");

                    var traceSource = new TraceSource("Snappy.Tracing", SourceLevels.All | SourceLevels.ActivityTracing);

                    await _webSocket.ConnectAsync(new Uri("ws://snapserver-direct.iszland.com:1780/jsonrpc"), _cancellationTokenSource.Token);

                    IJsonRpcMessageHandler jsonRpcMessageHandler = new WebSocketMessageHandler(_webSocket);

                    JsonRpc rpc = new JsonRpc(jsonRpcMessageHandler);

                    rpc.TraceSource = traceSource;

                    rpc.AddLocalRpcMethod("Client.OnConnect", new Func<string, Client, Task>((id, client) => OnClientOnConnectAsync(id, client)));
                    rpc.AddLocalRpcMethod("Client.OnDisconnect", new Func<string, Client, Task>((id, client) => OnClientOnDisconnectAsync(id, client)));
                    rpc.AddLocalRpcMethod("Client.OnVolumeChanged", new Func<string, Volume, Task>((id, volume) => OnClientOnVolumeChangedAsync(id, volume)));
                    rpc.AddLocalRpcMethod("Client.SnapserverClientOnLatencyChanged", new Func<string, Latency, Task>((id, latency) => OnClientOnLatencyChangedAsync(id, latency)));
                    rpc.AddLocalRpcMethod("Group.OnMute", new Func<string, bool, Task>((id, mute) => OnGroupOnMuteAsync(id, mute)));
                    rpc.AddLocalRpcMethod("Group.OnStreamChanged", new Func<string, string, Task>((id, stream_id) => OnGroupOnStreamChangedAsync(id, stream_id)));
                    rpc.AddLocalRpcMethod("Group.OnNameChanged", new Func<string, string, Task>((id, name) => OnGroupOnNameChangedAsync(id, name)));
                    rpc.AddLocalRpcMethod("Stream.OnUpdate", new Func<string, Stream, Task>((id, stream) => OnStreamOnUpdateAsync(id, stream)));
                    rpc.AddLocalRpcMethod("Stream.OnProperties", new Func<string, Metadata, Task>((id, metaData) => OnStreamOnPropertiesAsync(id, metaData)));
                    rpc.AddLocalRpcMethod("Server.OnUpdate", new Func<ParentServer, Task>((server) => OnServerOnUpdateAsync(server)));
                    rpc.StartListening();

                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to start JsonRpc connection: {ex}");
                    await Task.Delay(1000);
                }
            }
        }

        private async Task OnClientOnConnectAsync(string id, Client client)
        {
            _logger.Information($"Client.OnConnect: id = {id}, client = {JsonSerializer.Serialize(client)}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Client.OnConnect" });
        }

        private async Task OnClientOnDisconnectAsync(string id, Client client)
        {
            _logger.Information($"Client.OnDisconnect: id = {id}, client = {JsonSerializer.Serialize(client)}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Client.OnDisconnect" });
        }

        private async Task OnClientOnVolumeChangedAsync(string id, Volume volume)
        {
            _logger.Information($"Client.OnVolumeChanged: volume = {JsonSerializer.Serialize(volume)}");

            var serverStatus = await _snapcastService.ServerGetStatus();

            var group = serverStatus.Result.Server.Groups.Where(g =>
            {
                var client = g.Clients.Where(c => c.Id == id).FirstOrDefault();
                return client != null;
            }).FirstOrDefault();

            // await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Client.OnVolumeChanged" });

            await _backChannelHub.InvokeAsync("GroupChangedAsync", group.Id);

        }

        private async Task OnClientOnLatencyChangedAsync(string id, Latency latency)
        {
            _logger.Information($"Client.OnLatencyChanged: id = {id}, latency = {JsonSerializer.Serialize(latency)}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Client.OnLatencyChanged" });
        }

        private async Task OnGroupOnMuteAsync(string id, bool mute)
        {
            _logger.Information($"Group.OnMute: id = {id}, mute = {mute}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Group.OnMute" });

            await _backChannelHub.InvokeAsync("GroupChangedAsync", id);
        }

        private async Task OnGroupOnStreamChangedAsync(string id, string stream_id)
        {
            _logger.Information($"Group.OnStreamChanged: id = {id}, streamId = {stream_id}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Group.OnStreamChanged" });

            await _backChannelHub.InvokeAsync("GroupChangedAsync", id);
        }

        private async Task OnGroupOnNameChangedAsync(string id, string name)
        {
            _logger.Information($"Group.OnNameChanged: id = {id}, name = {name}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Group.OnNameChanged" });

            await _backChannelHub.InvokeAsync("GroupChangedAsync", id);

        }

        private async Task OnStreamOnUpdateAsync(string id, Stream stream)
        {
            _logger.Information($"Stream.OnUpdate: id = {id}, stream = {JsonSerializer.Serialize(stream)}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Stream.OnUpdate" });

        }

        private async Task OnStreamOnPropertiesAsync(string id, Metadata metadata)
        {
            _logger.Information($"Stream.OnProperties: id = {id}, metadata = {JsonSerializer.Serialize(metadata)}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Stream.OnProperties" });
        }

        private async Task OnServerOnUpdateAsync(ParentServer parentServer)
        {
            _logger.Information($"Server.OnUpdate: parentServer = {JsonSerializer.Serialize(parentServer)}");

            await _backChannelHub.InvokeAsync("GlobalMessageAsync", new GlobalMessage { Message = "Server.OnUpdate" });
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Information("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}