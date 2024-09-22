using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Snappy.API
{
    using Snappy.Hubs;
    using Snappy.Configuration;
    using Snappy.Extensions;
    using Snappy.API.Controllers;
    using Snappy.Services;
    using Snappy.Models.Yahama;

    public class Startup
    {

        protected IConfiguration _configuration { get; }

        protected IConfigurationSection _hostConfiguration
        {
            get
            {
                return _configuration.GetSection("Host");
            }
        }

        protected IConfigurationSection _spotifyConfiguration
        {
            get
            {
                return _configuration.GetSection("Spotify");
            }
        }

        protected IConfigurationSection _yamahaConfiguration
        {
            get
            {
                return _configuration.GetSection("Yamaha");
            }
        }
        protected IConfigurationSection _librespotConfiguration
        {
            get
            {
                return _configuration.GetSection("Librespot");
            }
        }


        public string HubPath
        {
            get
            {
                string hubPath = System.Environment.GetEnvironmentVariable("HUB_PATH");

                var config = _hostConfiguration.Get<HostConfiguration>();

                if (string.IsNullOrEmpty(hubPath))
                    hubPath = config.HubPath;

                return hubPath;
            }
        }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.ConfigureConfigurationServices(services);
            this.ConfigureLoggingServices(services);
            this.ConfigureControllerServices(services);
            this.ConfigureSignalRServices(services);
            this.ConfigureSnapcastDependencies(services);
            this.ConfigureSpotifyDependencies(services);
            this.ConfigureYamahaServices(services);
            this.ConfigureLibrespotServices(services);
        }

        private void ConfigureConfigurationServices(IServiceCollection services)
        {
            services.Configure<HostConfiguration>(_hostConfiguration);
            services.Configure<SpotifyConfiguration>(_spotifyConfiguration);
            services.Configure<YamahaConfiguration>(_yamahaConfiguration);
            services.Configure<LibrespotConfiguration>(_librespotConfiguration);

        }

        private void ConfigureSpotifyDependencies(IServiceCollection services)
        {
        }

        private void ConfigureYamahaServices(IServiceCollection services)
        {
            services.AddSingleton<IYamahaService<YamahaAvMainZone>, YamahaService<YamahaAvMainZone>>();
            services.AddSingleton<IYamahaService<YamahaAvZone2>, YamahaService<YamahaAvZone2>>();
            services.AddSingleton<IYamahaService<YamahaAvZone3>, YamahaService<YamahaAvZone3>>();
        }

        private void ConfigureLibrespotServices(IServiceCollection services)
        {
            services.AddSingleton<ILibrespotService, LibrespotService>();
        }

        private void ConfigureLoggingServices(IServiceCollection services)
        {
            services.AddSingleton<ILogger>(Log.Logger);
        }

        private void ConfigureControllerServices(IServiceCollection services)
        {
            services.AddControllers(opts =>
            {
                // opts.UseCentralRoutePrefix(new RouteAttribute(this.HubPath));
            })
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy =
                    SnakeCaseNamingPolicy.Instance;
            });
        }

        private void ConfigureSignalRServices(IServiceCollection services)
        {
            services.AddSingleton<SnappyHubClient>();
            services.AddCors(opts => opts.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));
            // services.AddSignalR(o => o.EnableDetailedErrors = true)
            //     .AddJsonProtocol(o => {
            //         o.PayloadSerializerOptions.PropertyNamingPolicy = null;
            //     });
            services.AddSignalR(o => o.EnableDetailedErrors = true);
        }

        private void ConfigureSnapcastDependencies(IServiceCollection services)
        {
            services.AddSnapcastDependencies();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            app.UseCors("CorsPolicy");
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSerilogRequestLogging();
            // app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                var path = "/hub";
                if (!string.IsNullOrEmpty(HubPath))
                    path = string.Format("/{0}/hub", HubPath);

                endpoints.MapHub<SnappyHubClient>(path);
            });
        }
    }
}
