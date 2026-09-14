using System.Net;
using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;

namespace PordznakanAPI.Filters
{
    /// <summary>
    /// Guards the /hangfire dashboard with an optional IP allowlist plus HTTP Basic
    /// authentication.
    ///
    /// Fails closed: if no password is configured the dashboard denies everyone, so a
    /// missing setting can never silently reopen it to the internet.
    ///
    /// Configuration (Hangfire:Dashboard):
    ///   Username    - login name, defaults to "admin"
    ///   Password    - required; set it through the environment variable
    ///                 Hangfire__Dashboard__Password, never in appsettings.json
    ///   AllowedIps  - optional list of client IPs; empty means any IP may attempt login
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private const string Realm = "Pordznakan Hangfire";

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
            var settings = configuration.GetSection("Hangfire:Dashboard");

            // 1. IP allowlist. Checked first so a blocked address never gets a login prompt
            //    and cannot probe for valid credentials.
            // Hangfire turns a false result into 401 itself, so no status is set here.
            if (!IsAllowedAddress(httpContext, settings))
                return false;

            var password = settings["Password"];
            if (string.IsNullOrWhiteSpace(password))
            {
                // No credentials configured — refuse rather than expose the dashboard.
                var logger = httpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger<HangfireDashboardAuthorizationFilter>();

                logger.LogWarning(
                    "Hangfire dashboard request denied: Hangfire:Dashboard:Password is not configured. " +
                    "Set the environment variable Hangfire__Dashboard__Password to enable it.");

                return false;
            }

            var username = settings["Username"];
            if (string.IsNullOrWhiteSpace(username))
                username = "admin";

            // 2. HTTP Basic authentication.
            if (TryReadBasicCredentials(httpContext, out var providedUser, out var providedPassword)
                && FixedTimeEquals(providedUser, username)
                && FixedTimeEquals(providedPassword, password))
            {
                return true;
            }

            Challenge(httpContext);
            return false;
        }

        private static bool IsAllowedAddress(HttpContext httpContext, IConfigurationSection settings)
        {
            var allowedIps = settings.GetSection("AllowedIps").Get<string[]>();
            if (allowedIps is null || allowedIps.Length == 0)
                return true; // No allowlist configured — rely on Basic auth alone.

            var remoteIp = httpContext.Connection.RemoteIpAddress;
            if (remoteIp is null)
                return false;

            if (remoteIp.IsIPv4MappedToIPv6)
                remoteIp = remoteIp.MapToIPv4();

            foreach (var candidate in allowedIps)
            {
                if (IPAddress.TryParse(candidate, out var allowed))
                {
                    if (allowed.IsIPv4MappedToIPv6)
                        allowed = allowed.MapToIPv4();

                    if (allowed.Equals(remoteIp))
                        return true;
                }
            }

            return false;
        }

        private static bool TryReadBasicCredentials(HttpContext httpContext, out string user, out string password)
        {
            user = string.Empty;
            password = string.Empty;

            string? header = httpContext.Request.Headers.Authorization;
            if (string.IsNullOrWhiteSpace(header) ||
                !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header["Basic ".Length..].Trim()));
                var separator = decoded.IndexOf(':');
                if (separator < 0)
                    return false;

                user = decoded[..separator];
                password = decoded[(separator + 1)..];
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static void Challenge(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            httpContext.Response.Headers.WWWAuthenticate = $"Basic realm=\"{Realm}\", charset=\"UTF-8\"";
        }

        /// <summary>
        /// Length-independent comparison, so a wrong password cannot be discovered by
        /// timing how long the check takes.
        /// </summary>
        private static bool FixedTimeEquals(string left, string right) =>
            CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(left),
                Encoding.UTF8.GetBytes(right));
    }
}
