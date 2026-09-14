using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PordznakanAPI.Filters
{
    /// <summary>
    /// Requires the X-API-KEY header to match Integration:IncomingApiKey.
    ///
    /// Can be applied to a single controller or action with [ApiKeyAuth], or registered
    /// globally in Program.cs when Integration:RequireApiKey is true.
    ///
    /// Fails closed: if no key is configured every request is rejected, so a missing
    /// setting cannot silently leave the API open.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string HEADER_NAME = "X-API-KEY";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Endpoints marked [AllowAnonymousApiKey] stay reachable without a key.
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousApiKeyAttribute>().Any())
            {
                await next();
                return;
            }

            var services = context.HttpContext.RequestServices;
            var configuredKey = services.GetRequiredService<IConfiguration>()["Integration:IncomingApiKey"];

            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                services.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<ApiKeyAuthAttribute>()
                    .LogError("Request rejected: Integration:IncomingApiKey is not configured. " +
                              "Set the environment variable Integration__IncomingApiKey.");

                context.Result = new UnauthorizedResult();
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HEADER_NAME, out var extractedKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Length-independent comparison, so the key cannot be recovered by timing.
            var provided = extractedKey.ToString();
            var matches = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(provided),
                Encoding.UTF8.GetBytes(configuredKey));

            if (!matches)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }

    /// <summary>
    /// Exempts an action or controller from the global API key requirement — for health
    /// checks or anything that must stay reachable without a key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AllowAnonymousApiKeyAttribute : Attribute
    {
    }
}
