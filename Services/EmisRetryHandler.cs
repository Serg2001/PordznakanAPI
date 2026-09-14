using System.Net;

namespace PordznakanAPI.Services
{
    /// <summary>
    /// Names of the EMIS HttpClients registered in Program.cs.
    /// </summary>
    public static class EmisClients
    {
        /// <summary>
        /// Client for data-api.emis.am/v1/getAllData/{regionId} only (pupils, schools,
        /// classrooms). Other EMIS endpoints in this project use their own clients.
        /// </summary>
        public const string GetAllData = "emis-getalldata";
    }

    /// <summary>
    /// Retries getAllData requests that fail with a transient error.
    ///
    /// The endpoint builds each region's payload on demand and is slow: time-to-first-byte
    /// for the smallest region is 25-50 seconds, and the largest regions take considerably
    /// longer. When the server is busy it answers 500 with a maintenance body rather than
    /// waiting, so a failed call is usually worth repeating a minute later instead of
    /// losing the region for the whole night.
    ///
    /// Only 5xx, 408, 429 and transport failures are retried — 401 (bad token) and 404 are
    /// permanent and fail immediately rather than wasting the whole retry budget.
    ///
    /// Tunable via the EmisApi section: AttemptTimeoutMinutes, MaxRetries.
    /// </summary>
    public class EmisRetryHandler : DelegatingHandler
    {
        private readonly ILogger<EmisRetryHandler>? _logger;
        private readonly TimeSpan _attemptTimeout;
        private readonly int _maxRetries;

        public EmisRetryHandler(IConfiguration configuration, ILogger<EmisRetryHandler>? logger = null)
        {
            var emis = configuration.GetSection("EmisApi");

            // Generous by design: a slow answer is still a good answer, and cutting it
            // short only means downloading the whole region again.
            _attemptTimeout = TimeSpan.FromMinutes(emis.GetValue<double?>("AttemptTimeoutMinutes") ?? 10);
            _maxRetries = Math.Max(0, emis.GetValue<int?>("MaxRetries") ?? 3);
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            for (var attempt = 0; ; attempt++)
            {
                // Give every attempt its own budget, so one stalled call cannot consume
                // the whole timeout and leave no room to retry.
                using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                attemptCts.CancelAfter(_attemptTimeout);

                HttpResponseMessage? response = null;
                string failure;

                try
                {
                    response = await base.SendAsync(request, attemptCts.Token);

                    if (!IsTransient(response.StatusCode))
                        return response;

                    failure = $"HTTP {(int)response.StatusCode} {response.StatusCode}";
                }
                catch (Exception ex) when (ex is HttpRequestException or IOException ||
                                           (ex is OperationCanceledException && !cancellationToken.IsCancellationRequested))
                {
                    failure = ex is OperationCanceledException
                        ? $"no response within {_attemptTimeout.TotalMinutes:0} minutes"
                        : ex.Message;
                }

                if (attempt >= _maxRetries)
                {
                    // Out of retries. Hand back the real response when there is one so the
                    // caller can log EMIS's own status and message.
                    if (response != null)
                        return response;

                    throw new HttpRequestException(
                        $"EMIS request to {request.RequestUri} failed after {attempt + 1} attempts: {failure}");
                }

                response?.Dispose();

                var delay = DelayFor(attempt);
                _logger?.LogWarning(
                    $"EMIS request to {request.RequestUri} failed ({failure}). " +
                    $"Retry {attempt + 1}/{_maxRetries} in {delay.TotalSeconds:0}s.");

                await Task.Delay(delay, cancellationToken);
            }
        }

        /// <summary>
        /// Backoff of 5s, 20s, then 60s for every further attempt — a busy server is only
        /// made busier by retrying immediately.
        /// </summary>
        private static TimeSpan DelayFor(int attempt) =>
            TimeSpan.FromSeconds(Math.Min(60, 5 * Math.Pow(4, attempt)));

        private static bool IsTransient(HttpStatusCode status) =>
            (int)status >= 500
            || status == HttpStatusCode.RequestTimeout
            || status == HttpStatusCode.TooManyRequests;
    }
}
