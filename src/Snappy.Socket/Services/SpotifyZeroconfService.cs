using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Snappy.Socket.Services
{
    using System.Linq;
    using Snappy.Configuration;
    using Zeroconf;

    public class SpotifyZeroconfService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private IOptions<HostConfiguration> _hostConfiguration { get; }
        protected CancellationTokenSource _cancellationTokenSource;


        public SpotifyZeroconfService(
            ILogger logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IOptions<HostConfiguration> hostConfiguration
        )
        {
            _logger = logger;
            _hostConfiguration = hostConfiguration;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public override void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            IReadOnlyList<IZeroconfHost> results = await
                ZeroconfResolver.ResolveAsync("_spotify-connect._tcp.local.");

            ILookup<string, string> domains = await ZeroconfResolver.BrowseDomainsAsync();
                var responses = await ZeroconfResolver.ResolveAsync(domains.Select(g => g.Key));

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

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}