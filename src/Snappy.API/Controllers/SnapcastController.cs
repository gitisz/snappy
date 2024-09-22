using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace Snappy.API.Controllers
{
    using Snappy.Services;
    using Snappy.Models;
    using Snappy.Hubs;
    using System.Linq;

    [ApiController]
    [Route("[controller]")]
    public class SnapcastController : ControllerBase
    {
        private readonly ILogger _logger;

        private readonly ISnapcastService _snapcastService;

        private readonly IHubContext<SnappyHubClient, ISnappyHubClient> _hubContext;

        public SnapcastController(ILogger logger,
            ISnapcastService snapcastService,
            IHubContext<SnappyHubClient, ISnappyHubClient> hubContext
            )
        {
            _logger = logger;
            _snapcastService = snapcastService;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("Server/GetStatus")]
        public async Task<ActionResult<Snapcast>> GetStatusAsync()
        {
            Snapcast snapcast = null;
            try
            {
                snapcast = await _snapcastService.ServerGetStatus();
            }
            catch (System.Exception ex)
            {
                _logger.Error("Server/GetStatus", ex);
                return BadRequest(ex.ToString());
            }

            return Ok(snapcast);
        }

        [HttpPost]
        [Route("Group/SetName/{id}/{name}")]
        public async Task<ActionResult> SetNameAsync(string id, string name)
        {
            try
            {
                await _snapcastService.GroupSetName(id, name);
            }
            catch (System.Exception ex)
            {
                _logger.Error($"Group/SetName/{id}/{name}", ex);
                return BadRequest(ex.ToString());
            }

            return Ok();
        }

        [HttpPost]
        [Route("Group/SetStream/{id}/{name}")]
        public async Task<ActionResult> SetStreamAsync(string id, string name)
        {
            try
            {
                await _snapcastService.GroupSetStream(id, name);
            }
            catch (System.Exception ex)
            {
                _logger.Error($"Group/SetStream/{id}/{name}", ex);
                return BadRequest(ex.ToString());
            }

            return Ok();
        }

        [HttpPost]
        [Route("Group/SetMute/{id}/{muted}")]
        public async Task<ActionResult> SetMuteAsync(string id, bool muted)
        {
            try
            {
                await _snapcastService.GroupSetMute(id, muted);
            }
            catch (System.Exception ex)
            {
                _logger.Error($"Group/SetMute/{id}/{muted}", ex);
                return BadRequest(ex.ToString());
            }

            return Ok();
        }

        [HttpPost]
        [Route("Group/SetClients/{id}")]
        public async Task<ActionResult> SetClientsAsync(string id, string [] clientIds)
        {
            try
            {
                await _snapcastService.GroupSetClients(id, clientIds);

                var serverStatus = await _snapcastService.ServerGetStatus();

                var groups = serverStatus.Result.Server.Groups;

                var group = groups.Where(g => g.Id == id)
                    .FirstOrDefault();

                await _hubContext.Clients.All.GroupChangedAsync(id, group);
            }
            catch (System.Exception ex)
            {
                _logger.Error($"Group/SetClients/{id}", ex);
                return BadRequest(ex.ToString());
            }

            return Ok();
        }


        [HttpPost]
        [Route("Client/SetVolume/{id}/{percent}/{muted}")]
        public async Task<ActionResult> SetVolumeAsync(string id, int percent, bool muted)
        {
            try
            {
                await _snapcastService.ClientSetVolume(id, percent, muted);
            }
            catch (System.Exception ex)
            {
                _logger.Error($"Client/SetVolume/{id}/{percent}/{muted}", ex);
                return BadRequest(ex.ToString());
            }

            return Ok();
        }


        [HttpPost]
        [Route("Client/Power")]
        public async Task<ActionResult> PowerAsync()
        {
            try
            {
                await _hubContext.Clients.All.GlobalMessageAsync(new Models.GlobalMessage { HostName = "localhost", Message = "This is a test..." });
            }
            catch (System.Exception ex)
            {
                _logger.Error($"Client/Power", ex);
                return BadRequest(ex.ToString());
            }

            return Ok();
        }


    }
}
