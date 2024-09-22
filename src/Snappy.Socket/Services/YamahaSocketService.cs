using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Snappy.Socket.Services
{
    using KellermanSoftware.CompareNetObjects;
    using Snappy.Configuration;
    using Snappy.Extensions;
    using Snappy.Models.Yahama;
    using Snappy.Services;

    public class YamahaSocketService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private IOptions<HostConfiguration> _hostConfiguration { get; }
        private IOptions<YamahaConfiguration> _yamahaConfiguration { get; }
        protected HubConnection _backChannelHub;
        protected ClientWebSocket _webSocket;
        protected CancellationTokenSource _cancellationTokenSource;
        protected List<YamahaComparer> _yamahaComparerList;

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

        private readonly IYamahaService<YamahaAvMainZone> _yamahaServiceMainZone;
        private readonly IYamahaService<YamahaAvZone2> _yamahaServiceZone2;
        private readonly IYamahaService<YamahaAvZone3> _yamahaServiceZone3;

        public YamahaSocketService(
            ILogger logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IOptions<HostConfiguration> hostConfiguration,
            IOptions<YamahaConfiguration> yamahaConfiguration,
            IYamahaService<YamahaAvMainZone> yamahaAvMainZoneService,
            IYamahaService<YamahaAvZone2> yamahaAvZone2Service,
            IYamahaService<YamahaAvZone3> yamahaAvZone3Service
        )
        {
            _logger = logger;
            _hostConfiguration = hostConfiguration;
            _yamahaConfiguration = yamahaConfiguration;
            _hostApplicationLifetime = hostApplicationLifetime;
            _yamahaServiceMainZone = yamahaAvMainZoneService;
            _yamahaServiceZone2 = yamahaAvZone2Service;
            _yamahaServiceZone3 = yamahaAvZone3Service;

            initializeSignalRHub();
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

            _yamahaComparerList = _yamahaConfiguration.Value.Sources.Select(s => new YamahaComparer
            {
                Source = s.Source,
                Url = s.Url,
                PreviousMainZone = null,
                PreviousZone2 = null,
                PreviousZone3 = null,
            }).ToList();

            await ExecuteAsync(_cancellationTokenSource.Token);

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Information("Worker running at: {time}", DateTimeOffset.Now);

                await _yamahaComparerList.ForEachAsync(async y =>
                {
                    await trackYamahaServiceMainZoneAsync(y);
                    await trackYamahaServiceZone2Async(y);
                    await trackYamahaServiceZone3Async(y);
                }, new SnappyExtensions.AsyncParallelOptions { MaxDegreeOfParallelism = 5 });

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task trackYamahaServiceMainZoneAsync(YamahaComparer yamahaComparer)
        {
            var mainZone = await _yamahaServiceMainZone.BasicStatusAsync(yamahaComparer.Source, yamahaComparer.Url);

            if (yamahaComparer.PreviousMainZone == null)
            {
                yamahaComparer.PreviousMainZone = mainZone;
            }
            else
            {
                CompareLogic compareLogic = new CompareLogic();

                ComparisonResult result = compareLogic.Compare(yamahaComparer.PreviousMainZone, mainZone);

                if (!result.AreEqual)
                {
                    yamahaComparer.PreviousMainZone = mainZone;

                    try
                    {
                        await this._backChannelHub.InvokeAsync("notifyYamahaMainZoneBasicStatusChangedAsync", yamahaComparer.Source, mainZone);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.Error(ex.ToString());
                    }
                }
            }
        }

        private async Task trackYamahaServiceZone2Async(YamahaComparer yamahaComparer)
        {
            var zone2 = await _yamahaServiceZone2.BasicStatusAsync(yamahaComparer.Source, yamahaComparer.Url);

            if (yamahaComparer.PreviousZone2 == null)
            {
                yamahaComparer.PreviousZone2 = zone2;
            }
            else
            {
                CompareLogic compareLogic = new CompareLogic();

                ComparisonResult result = compareLogic.Compare(yamahaComparer.PreviousZone2, zone2);

                if (!result.AreEqual)
                {
                    yamahaComparer.PreviousZone2 = zone2;

                    try
                    {
                        await this._backChannelHub.InvokeAsync("notifyYamahaZone2BasicStatusChangedAsync", yamahaComparer.Source, zone2);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.Error(ex.ToString());
                    }
                }
            }
        }

        private async Task trackYamahaServiceZone3Async(YamahaComparer yamahaComparer)
        {
            var zone3 = await _yamahaServiceZone3.BasicStatusAsync(yamahaComparer.Source, yamahaComparer.Url);

            if (yamahaComparer.PreviousZone3 == null)
            {
                yamahaComparer.PreviousZone3 = zone3;
            }
            else
            {
                CompareLogic compareLogic = new CompareLogic();

                ComparisonResult result = compareLogic.Compare(yamahaComparer.PreviousZone3, zone3);

                if (!result.AreEqual)
                {
                    yamahaComparer.PreviousZone3 = zone3;

                    try
                    {
                        await this._backChannelHub.InvokeAsync("notifyYamahaZone3BasicStatusChangedAsync", yamahaComparer.Source, zone3);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.Error(ex.ToString());
                    }
                }
            }
        }
    }

    public class YamahaComparer : ReceiverSource
    {
        public YamahaAvMainZone PreviousMainZone { get; set; }
        public YamahaAvZone2 PreviousZone2 { get; set; }
        public YamahaAvZone3 PreviousZone3 { get; set; }
    }
}