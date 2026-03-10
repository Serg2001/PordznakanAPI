using Newtonsoft.Json.Linq;

namespace PordznakanAPI.Services
{
    public interface ILogTransferService
    {
        /// <summary>
        /// Fetches log entries from the source API for one region and one date.
        /// URL format: {sourceBaseUrl}/{regionId}/{date:yyyy-MM-dd}
        /// Handles both JSON and PHP print_r response formats.
        /// </summary>
        Task<JArray> FetchLogsForRegionAsync(string sourceBaseUrl, int regionId, DateOnly date);

        /// <summary>
        /// Serializes the list as a JSON array and POSTs it to the bulk-update endpoint.
        /// Throws if the remote call fails.
        /// </summary>
        Task SendBulkAsync<T>(IList<T> logs);

        /// <summary>
        /// Iterates regions 1-10 for the given date, maps each raw JObject to a model
        /// via <paramref name="mapper"/>, then sends all accumulated records in a single bulk call.
        /// </summary>
        Task ProcessAllRegionsAsync<T>(
            string sourceBaseUrl,
            string logTypeName,
            DateOnly date,
            Func<JObject, DateTime, T> mapper);
    }
}
