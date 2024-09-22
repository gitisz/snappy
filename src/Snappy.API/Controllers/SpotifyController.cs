using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using SpotifyAPI.Web;

namespace Snappy.API.Controllers
{
    using Snappy.Services;
    using Snappy.Models;
    using Snappy.Hubs;
    using Snappy.Configuration;
    using static SpotifyAPI.Web.PlayerCurrentlyPlayingRequest;
    using System;
    using System.Collections.Generic;

    [ApiController]
    [Route("[controller]")]
    public class SpotifyController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IOptions<SpotifyConfiguration> _spotifyConfiguration;

        public SpotifyController(ILogger logger,
            IOptions<SpotifyConfiguration> spotifyConfiguration
            )
        {
            _logger = logger;
            _spotifyConfiguration = spotifyConfiguration;
        }

        [HttpGet]
        [Route("Login/Request/{source}")]
        public string SpotifyLoginRequest(string source)
        {
            var s = _spotifyConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .FirstOrDefault();

            var loginRequest = new LoginRequest(new Uri($"http://localhost:4200/pages/spotify/login/callback/{source}"), s.ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = new[] {
                        Scopes.UserReadCurrentlyPlaying,
                        Scopes.UserReadPlaybackPosition,
                        Scopes.UserReadPlaybackState,
                        Scopes.UserModifyPlaybackState,
                        Scopes.UserReadRecentlyPlayed,
                    }
            };
            var uri = loginRequest.ToUri();

            return uri.ToString();
        }


        [HttpPost]
        [Route("AccessToken/{source}/{spotifyAuthType}")]
        public async Task<ActionResult<string>> AccessTokenAsync([FromBody] SpotifyAuthorizationCode spotifyAuthorizationCode, string source, SpotifyAuthType spotifyAuthType)
        {
            var s = _spotifyConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .FirstOrDefault();

            AuthorizationCodeTokenResponse authorizationCodeTokenResponse;

            try
            {
                // This guy can only be used once after sign-in.
                authorizationCodeTokenResponse = await new OAuthClient().RequestToken(
                    new AuthorizationCodeTokenRequest(
                        s.ClientId,
                        s.ClientSecret,
                        spotifyAuthorizationCode.Code,
                        new Uri($"http://localhost:4200/pages/spotify/login/callback/{source}"))

                );
            }
            catch (System.Exception ex)
            {
                _logger.Error("GetAccessTokenAsync", ex);

                return BadRequest(ex.ToString());
            }

            return Ok(authorizationCodeTokenResponse);
        }

        [HttpPost]
        [Route("GetCurrentlyPlaying/{source}")]
        public async Task<ActionResult<IPlayableItem>> GetCurrentlyPlayingAsync([FromBody] AuthorizationCodeTokenResponse authorizationCodeTokenResponse, string source)
        {
            IPlayableItem playableItem = null;

            try
            {
                var spotify = GetSpotifyClient(authorizationCodeTokenResponse, source);

                var current = new PlayerCurrentlyPlayingRequest(AdditionalTypes.All);

                CurrentlyPlaying currentlyPlaying = await spotify.Player.GetCurrentlyPlaying(current);

                if(currentlyPlaying != null)
                {
                    playableItem = currentlyPlaying.Item;
                }
                else
                {
                    return Ok(@"It's quiet around here :\");
                }

            }
            catch (SpotifyAPI.Web.APIUnauthorizedException ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

            }
            catch (System.Exception ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

                return BadRequest(ex.ToString());
            }

            return Ok(playableItem);
        }

        [HttpPost]
        [Route("GetAvailableDevices/{source}")]
        public async Task<ActionResult<IPlayableItem>> GetAvailableDevicesAsync([FromBody] AuthorizationCodeTokenResponse authorizationCodeTokenResponse, string source)
        {
            IList<Device> devices = null;

            try
            {
                var spotify = GetSpotifyClient(authorizationCodeTokenResponse, source);

                DeviceResponse deviceResponse = await spotify.Player.GetAvailableDevices();

                devices = deviceResponse.Devices;
            }
            catch (SpotifyAPI.Web.APIUnauthorizedException ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

            }
            catch (System.Exception ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

                return BadRequest(ex.ToString());
            }

            return Ok(devices);
        }

        [HttpPost]
        [Route("TransferPlayback/{source}/{deviceId}")]
        public async Task<ActionResult<bool>>TransferPlaybackAsync([FromBody] AuthorizationCodeTokenResponse authorizationCodeTokenResponse, string source, string deviceId)
        {
            bool playbackTrasferred = true;

            try
            {
                var spotify = GetSpotifyClient(authorizationCodeTokenResponse, source);

                PlayerTransferPlaybackRequest playerTransferPlaybackRequest = new PlayerTransferPlaybackRequest(new List<string> { deviceId });
                playerTransferPlaybackRequest.Play = true;
                playbackTrasferred = await spotify.Player.TransferPlayback(playerTransferPlaybackRequest);
            }
            catch (SpotifyAPI.Web.APIUnauthorizedException ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

            }
            catch (System.Exception ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

                return BadRequest(ex.ToString());
            }

            return Ok(playbackTrasferred);
        }

        [HttpPost]
        [Route("CurrentPlayback/{source}")]
        public async Task<ActionResult<IPlayableItem>> GetCurrentPlaybackAsync([FromBody] AuthorizationCodeTokenResponse authorizationCodeTokenResponse, string source)
        {
            CurrentlyPlayingContext currentlyPlayingContext = null;

            try
            {
                var spotify = GetSpotifyClient(authorizationCodeTokenResponse, source);

                currentlyPlayingContext = await spotify.Player.GetCurrentPlayback();

                if(currentlyPlayingContext == null)
                {
                    return Ok(@"It's quiet around here :\");
                }
                else
                {
                }

            }
            catch (SpotifyAPI.Web.APIUnauthorizedException ex)
            {
                _logger.Error($"CurrentPlayback {ex}");

            }
            catch (System.Exception ex)
            {
                _logger.Error($"CurrentPlayback {ex}");

                return BadRequest(ex.ToString());
            }

            return Ok(currentlyPlayingContext);
        }

        [HttpPost]
        [Route("PausePlayback/{source}")]
        public async Task<ActionResult> PausePlaybackAsync([FromBody] AuthorizationCodeTokenResponse authorizationCodeTokenResponse, string source)
        {
            try
            {
                var spotify = GetSpotifyClient(authorizationCodeTokenResponse, source);

                await spotify.Player.PausePlayback();

            }
            catch (SpotifyAPI.Web.APIUnauthorizedException ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

            }
            catch (System.Exception ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

                return BadRequest(ex.ToString());
            }

            return Ok();
        }


        [HttpPost]
        [Route("ResumePlayback/{source}")]
        public async Task<ActionResult> ResumePlaybackAsync([FromBody] AuthorizationCodeTokenResponse authorizationCodeTokenResponse, string source)
        {
            try
            {
                var spotify = GetSpotifyClient(authorizationCodeTokenResponse, source);

                await spotify.Player.ResumePlayback();

            }
            catch (SpotifyAPI.Web.APIUnauthorizedException ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

            }
            catch (System.Exception ex)
            {
                _logger.Error($"GetCurrentlyPlayingAsync {ex}");

                return BadRequest(ex.ToString());
            }

            return Ok();
        }

        private SpotifyClient GetSpotifyClient(AuthorizationCodeTokenResponse authorizationCodeTokenResponse, string source)
        {
            var s = _spotifyConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .FirstOrDefault();

            var spotifyClientConfig = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new AuthorizationCodeAuthenticator(s.ClientId, s.ClientSecret, authorizationCodeTokenResponse));

            return new SpotifyClient(spotifyClientConfig);;
        }
    }
}