using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PordznakanAPI.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string HEADER_NAME = "X-API-KEY";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = config["Integration:IncomingApiKey"];

            if (!context.HttpContext.Request.Headers.TryGetValue(HEADER_NAME, out var extractedKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!apiKey.Equals(extractedKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
