using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Snappy.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        public HealthController()
        {
        }

        [HttpGet]
        [Route("/")]
        public IActionResult Get()
        {
            return Ok("Snappy.API - All systems go!");
        }

        [HttpGet]
        [Route("/health")]
        public IActionResult GetHealth()
        {
            return Ok("Snappy.API - All systems go!");
        }
    }
}
