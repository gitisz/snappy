
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace Snappy.API.Controllers
{
    using Snappy.Configuration;
    using Snappy.Models.Yahama;
    using Snappy.Services;

    [ApiController]
    [CamelCaseJsonOutput]
    [Route("[controller]")]
    public class YamahaController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IOptions<YamahaConfiguration> _yamahaConfiguration;
        private readonly IYamahaService<YamahaAvMainZone> _yamahaServiceMainZone;
        private readonly IYamahaService<YamahaAvZone2> _yamahaServiceZone2;
        private readonly IYamahaService<YamahaAvZone3> _yamahaServiceZone3;

        public YamahaController(ILogger logger,
            IOptions<YamahaConfiguration> yamahaConfiguration,
            IYamahaService<YamahaAvMainZone> yamahaServiceMainZone,
            IYamahaService<YamahaAvZone2> yamahaServiceZone2,
            IYamahaService<YamahaAvZone3> yamahaServiceZone3
            )
        {
            _logger = logger;
            _yamahaConfiguration = yamahaConfiguration;
            _yamahaServiceMainZone = yamahaServiceMainZone;
            _yamahaServiceZone2 = yamahaServiceZone2;
            _yamahaServiceZone3 = yamahaServiceZone3;
        }

        [HttpGet]
        [Route("Main_Zone/BasicStatus/{source}")]
        public async Task<IActionResult> MainZoneBasicStatusAsync(string source)
        {
            YamahaAvMainZone mainZone;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                mainZone = await _yamahaServiceMainZone.BasicStatusAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(mainZone);
        }

        [HttpGet]
        [Route("Main_Zone/Config/{source}")]
        public async Task<IActionResult> MainZoneConfigAsync(string source)
        {
            YamahaAvMainZone mainZone;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                mainZone = await _yamahaServiceMainZone.ConfigAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(mainZone);
        }

        [HttpPut]
        [Route("Main_Zone/PowerOn/{source}")]
        public async Task<IActionResult> Main_ZonePowerOnAsync(string source)
        {
            YamahaAvMainZone mainZone;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                mainZone = await _yamahaServiceMainZone.PowerOnAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(mainZone);

        }

        [HttpPut]
        [Route("Main_Zone/PowerOff/{source}")]
        public async Task<IActionResult> Main_ZonePowerOffAsync(string source)
        {
            YamahaAvMainZone mainZone;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                mainZone = await _yamahaServiceMainZone.PowerOffAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(mainZone);
        }

        [HttpPut]
        [Route("Main_Zone/Volume/Lvl/Val/{source}")]
        public async Task<IActionResult> Main_ZoneVolumeLvlValAsync([FromBody] Lvl lvl, string source)
        {
            YamahaAvMainZone mainZone;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                mainZone = await _yamahaServiceMainZone.VolumeLvlValAsync(source, yamahaUrl, lvl.Val);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(mainZone);
        }

        [HttpPut]
        [Route("Main_Zone/Config/Name/Zone/{source}/{name}")]
        public async Task<IActionResult> Main_ZoneConfigNameZoneAsync(string source, string name)
        {
            YamahaAvMainZone mainZone;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                mainZone = await _yamahaServiceMainZone.ConfigNameZoneAsync(source, yamahaUrl, name);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(mainZone);
        }

        [HttpGet]
        [Route("Zone_2/BasicStatus/{source}")]
        public async Task<IActionResult> Zone_2BasicStatusAsync(string source)
        {
            YamahaAvZone2 zone2;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone2 = await _yamahaServiceZone2.BasicStatusAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone2);
        }

        [HttpGet]
        [Route("Zone_2/Config/{source}")]
        public async Task<IActionResult> Zone2ConfigAsync(string source)
        {
            YamahaAvZone2 zone2;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone2 = await _yamahaServiceZone2.ConfigAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone2);
        }

        [HttpPut]
        [Route("Zone_2/Config/Name/Zone/{source}/{name}")]
        public async Task<IActionResult> Zone_2ConfigNameZoneAsync(string source, string name)
        {
            YamahaAvZone2 zone2;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone2 = await _yamahaServiceZone2.ConfigNameZoneAsync(source, yamahaUrl, name);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone2);
        }

        [HttpPut]
        [Route("Zone_2/PowerOn/{source}")]
        public async Task<IActionResult> Zone_2PowerOnAsync(string source)
        {
            YamahaAvZone2 zone2;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone2 = await _yamahaServiceZone2.PowerOnAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone2);
        }

        [HttpPut]
        [Route("Zone_2/PowerOff/{source}")]
        public async Task<IActionResult> Zone_2PowerOffAsync(string source)
        {
            YamahaAvZone2 zone2;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone2 = await _yamahaServiceZone2.PowerOffAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone2);
        }

        [HttpPut]
        [Route("Zone_2/Volume/Lvl/Val/{source}")]
        public async Task<IActionResult> Zone_2VolumeLvlValAsync([FromBody] Lvl lvl, string source)
        {
            YamahaAvZone2 zone2;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone2 = await _yamahaServiceZone2.VolumeLvlValAsync(source, yamahaUrl, lvl.Val);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone2);
        }


        [HttpGet]
        [Route("Zone_3/BasicStatus/{source}")]
        public async Task<IActionResult> Zone_3BasicStatusAsync(string source)
        {
            YamahaAvZone3 zone3;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone3 = await _yamahaServiceZone3.BasicStatusAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone3);
        }

        [HttpGet]
        [Route("Zone_3/Config/{source}")]
        public async Task<IActionResult> Zone3ConfigAsync(string source)
        {
            YamahaAvZone3 zone3;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone3 = await _yamahaServiceZone3.ConfigAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone3);
        }

        [HttpPut]
        [Route("Zone_3/Config/Name/Zone/{source}/{name}")]
        public async Task<IActionResult> Zone_3ConfigNameZoneAsync(string source, string name)
        {
            YamahaAvZone3 zone3;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone3 = await _yamahaServiceZone3.ConfigNameZoneAsync(source, yamahaUrl, name);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone3);
        }

        [HttpPut]
        [Route("Zone_3/PowerOn/{source}")]
        public async Task<IActionResult> Zone_3PowerOnAsync(string source)
        {
            YamahaAvZone3 zone3;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone3 = await _yamahaServiceZone3.PowerOnAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone3);
        }

        [HttpPut]
        [Route("Zone_3/PowerOff/{source}")]
        public async Task<IActionResult> Zone_3PowerOffAsync(string source)
        {
            YamahaAvZone3 zone3;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone3 = await _yamahaServiceZone3.PowerOffAsync(source, yamahaUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone3);
        }

        [HttpPut]
        [Route("Zone_3/Volume/Lvl/Val/{source}")]
        public async Task<IActionResult> Zone_3VolumeLvlValAsync([FromBody] Lvl lvl, string source)
        {
            YamahaAvZone3 zone3;

            var yamahaUrl = _yamahaConfiguration.Value.Sources
                .Where(s => s.Source == source)
                .Select(s => s.Url)
                .FirstOrDefault();

            try
            {
                zone3 = await _yamahaServiceZone3.VolumeLvlValAsync(source, yamahaUrl, lvl.Val);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }

            return Ok(zone3);
        }

    }
}