
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Snappy.API.Controllers
{
    public class CamelCaseJsonOutputAttribute : ActionFilterAttribute
    {
        private static readonly SystemTextJsonOutputFormatter Formatter = new SystemTextJsonOutputFormatter(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult)
                objectResult.Formatters.Add(Formatter);
        }
    }

}