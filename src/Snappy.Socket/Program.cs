using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Snappy.Socket
{
    using Serilog.Core;
    using Snappy.Configuration;
    using Snappy.Extensions;
    using Snappy.Hubs;
    using Snappy.Models.Yahama;
    using Snappy.Services;
    using Snappy.Socket.Services;

    public class Program
    {
        public static void Main(string[] args)
        {
            var switchMappings = new Dictionary<string, string>()
            {
                { "--host", "HostPort "}
            };

            if (args.Length == 0)
            {
                args = new string[] { "--host", "5001" };
            }

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{environment}.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args, switchMappings)
                .Build()
                ;

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger()
                ;

            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, config) => {
                    var env = context.HostingEnvironment;
                    config.ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console();
                }, writeToProviders: true)
                .ConfigureServices((h, s) =>
                {
                    s.Configure<HostConfiguration>(configuration.GetSection("Host"));
                    s.Configure<YamahaConfiguration>(configuration.GetSection("Yamaha"));
                    s.AddSingleton<ILogger>(logger);
                    s.AddSingleton<IYamahaService<YamahaAvMainZone>, YamahaService<YamahaAvMainZone>>();
                    s.AddSingleton<IYamahaService<YamahaAvZone2>, YamahaService<YamahaAvZone2>>();
                    s.AddSingleton<IYamahaService<YamahaAvZone3>, YamahaService<YamahaAvZone3>>();
                    // s.AddHostedService<SnappySocketService>();
                    // s.AddHostedService<YamahaSocketService>();
                    s.AddHostedService<SpotifyZeroconfService>();
                    s.AddSingleton<SnappyHubClient>();
                    s.AddSnapcastDependencies();

                })
                .Build()
                .Run();

        }
    }
}
