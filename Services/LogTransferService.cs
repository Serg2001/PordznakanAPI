using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PordznakanAPI.Services
{
    public class LogTransferService : ILogTransferService
    {
        private readonly ILogger<LogTransferService> _logger;
        private const string BulkUpdateUrl = "https://demo.dshh.am:1400/api/bulk-update";

        public LogTransferService(ILogger<LogTransferService> logger)
        {
            _logger = logger;
        }

        public async Task<JArray> FetchLogsForRegionAsync(string sourceBaseUrl, int regionId, DateOnly date)
        {
            using var client = new HttpClient();
            var url = $"{sourceBaseUrl}/{regionId}/{date:yyyy-MM-dd}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Source API returned {StatusCode} for region {RegionId}", response.StatusCode, regionId);
                return new JArray();
            }

            var responseText = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseText)) return new JArray();

            var trimmed = responseText.Trim();

            if (trimmed.Contains("<pre>Array") || trimmed.Contains("stdClass Object"))
                return ParsePhpArrayFormat(responseText);

            if (trimmed.StartsWith("[") || trimmed.StartsWith("{"))
                return JArray.Parse(responseText);

            _logger.LogWarning("Unknown response format from {SourceBaseUrl} for region {RegionId}", sourceBaseUrl, regionId);
            return new JArray();
        }

        public async Task SendBulkAsync<T>(IList<T> logs)
        {
            using var client = new HttpClient();
            var json = JsonConvert.SerializeObject(logs);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(BulkUpdateUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent {Count} records to bulk-update API", logs.Count);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Bulk-update API returned {StatusCode}: {Error}", response.StatusCode, error);
                throw new Exception($"Bulk-update API error {response.StatusCode}: {error}");
            }
        }

        public async Task ProcessAllRegionsAsync<T>(
            string sourceBaseUrl,
            string logTypeName,
            DateOnly date,
            Func<JObject, DateTime, T> mapper)
        {
            var allLogs = new List<T>();

            foreach (var regionId in Enumerable.Range(1, 10))
            {
                try
                {
                    _logger.LogInformation("Fetching {LogType} logs for region {RegionId} date {Date}...", logTypeName, regionId, date);
                    var logArray = await FetchLogsForRegionAsync(sourceBaseUrl, regionId, date);
                    var now = DateTime.UtcNow;

                    foreach (var token in logArray)
                    {
                        if (token is JObject logObj)
                            allLogs.Add(mapper(logObj, now));
                    }

                    _logger.LogInformation("Region {RegionId}: collected {Count} {LogType} entries", regionId, logArray.Count, logTypeName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching {LogType} logs for region {RegionId}", logTypeName, regionId);
                }
            }

            if (allLogs.Count == 0)
            {
                _logger.LogInformation("No {LogType} logs collected. Skipping bulk send.", logTypeName);
                return;
            }

            await SendBulkAsync(allLogs);
        }

        private JArray ParsePhpArrayFormat(string phpOutput)
        {
            var logArray = new JArray();
            var cleanOutput = Regex.Replace(phpOutput, @"<[^>]+>", "");
            var objectPattern = @"\[\d+\]\s*=>\s*stdClass\s+Object\s*\(";
            var objectMatches = Regex.Matches(cleanOutput, objectPattern);

            if (objectMatches.Count == 0) return logArray;

            for (int i = 0; i < objectMatches.Count; i++)
            {
                var startIndex = objectMatches[i].Index + objectMatches[i].Length;
                var parenCount = 1;
                var actualEnd = startIndex;
                for (int j = startIndex; j < cleanOutput.Length && parenCount > 0; j++)
                {
                    if (cleanOutput[j] == '(') parenCount++;
                    else if (cleanOutput[j] == ')') parenCount--;
                    if (parenCount == 0) { actualEnd = j; break; }
                }
                if (actualEnd <= startIndex) continue;

                var objectContent = cleanOutput.Substring(startIndex, actualEnd - startIndex);
                var logObj = new JObject();

                var idMatch = Regex.Match(objectContent, @"\[id\]\s*=>\s*(\d+)");
                if (idMatch.Success) logObj["id"] = idMatch.Groups[1].Value;

                var schoolIdMatch = Regex.Match(objectContent, @"\[school_id\]\s*=>\s*(\d+)");
                if (schoolIdMatch.Success) logObj["school_id"] = schoolIdMatch.Groups[1].Value;

                var actionDateMatch = Regex.Match(objectContent, @"\[action_date\]\s*=>\s*([\d\s\-:]+)");
                if (actionDateMatch.Success) logObj["action_date"] = actionDateMatch.Groups[1].Value.Trim();

                var methodMatch = Regex.Match(objectContent, @"\[method\]\s*=>\s*([^\r\n\(\)]+)");
                if (methodMatch.Success) logObj["method"] = methodMatch.Groups[1].Value.Trim();

                ExtractJsonBlock(objectContent, "[sent]", logObj, "sent");
                ExtractJsonBlock(objectContent, "[received]", logObj, "received");

                if (logObj["id"] != null && logObj["school_id"] != null)
                    logArray.Add(logObj);
            }

            return logArray;
        }

        private static void ExtractJsonBlock(string content, string marker, JObject target, string key)
        {
            var markerIndex = content.IndexOf(marker);
            if (markerIndex < 0) return;

            var blockStart = content.IndexOf('{', markerIndex);
            if (blockStart < 0) return;

            var braceCount = 0;
            var blockEnd = blockStart;
            for (int j = blockStart; j < content.Length; j++)
            {
                if (content[j] == '{') braceCount++;
                else if (content[j] == '}') braceCount--;
                if (braceCount == 0) { blockEnd = j + 1; break; }
            }

            if (blockEnd > blockStart)
                target[key] = content.Substring(blockStart, blockEnd - blockStart);
        }
    }
}
