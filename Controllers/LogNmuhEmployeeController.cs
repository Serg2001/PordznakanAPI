using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace PordznakanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogNmuhEmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LogNmuhEmployeeController>? _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public LogNmuhEmployeeController(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<LogNmuhEmployeeController>? logger = null)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Parses PHP array format (print_r output) and converts to JArray
        /// </summary>
        private JArray ParsePhpArrayFormat(string phpOutput)
        {
            var logArray = new JArray();
            
            // Remove HTML tags
            var cleanOutput = Regex.Replace(phpOutput, @"<[^>]+>", "");
            
            // Split by object boundaries - look for pattern: [N] => stdClass Object
            var objectPattern = @"\[\d+\]\s*=>\s*stdClass\s+Object\s*\(";
            var objectMatches = Regex.Matches(cleanOutput, objectPattern);
            
            if (objectMatches.Count == 0)
            {
                return logArray;
            }
            
            // Extract each object
            for (int i = 0; i < objectMatches.Count; i++)
            {
                var startIndex = objectMatches[i].Index + objectMatches[i].Length;
                
                // Find the matching closing parenthesis for this object
                var parenCount = 1;
                var actualEnd = startIndex;
                for (int j = startIndex; j < cleanOutput.Length && parenCount > 0; j++)
                {
                    if (cleanOutput[j] == '(') parenCount++;
                    else if (cleanOutput[j] == ')') parenCount--;
                    if (parenCount == 0)
                    {
                        actualEnd = j;
                        break;
                    }
                }
                
                if (actualEnd <= startIndex) continue;
                
                var objectContent = cleanOutput.Substring(startIndex, actualEnd - startIndex);
                var logObj = new JObject();
                
                // Extract id - pattern: [id] => 75380
                var idMatch = Regex.Match(objectContent, @"\[id\]\s*=>\s*(\d+)");
                if (idMatch.Success)
                {
                    logObj["id"] = idMatch.Groups[1].Value;
                }
                
                // Extract school_id - pattern: [school_id] => 926
                var schoolIdMatch = Regex.Match(objectContent, @"\[school_id\]\s*=>\s*(\d+)");
                if (schoolIdMatch.Success)
                {
                    logObj["school_id"] = schoolIdMatch.Groups[1].Value;
                }
                
                // Extract action_date - pattern: [action_date] => 2026-02-13 13:44:57
                var actionDateMatch = Regex.Match(objectContent, @"\[action_date\]\s*=>\s*([\d\s\-:]+)");
                if (actionDateMatch.Success)
                {
                    logObj["action_date"] = actionDateMatch.Groups[1].Value.Trim();
                }
                
                // Extract method - pattern: [method] => ExemptEmployee
                var methodMatch = Regex.Match(objectContent, @"\[method\]\s*=>\s*([^\r\n\(\)]+)");
                if (methodMatch.Success)
                {
                    logObj["method"] = methodMatch.Groups[1].Value.Trim();
                }
                
                // Extract sent - find JSON object by matching braces
                var sentIndex = objectContent.IndexOf("[sent]");
                if (sentIndex >= 0)
                {
                    var sentStart = objectContent.IndexOf('{', sentIndex);
                    if (sentStart >= 0)
                    {
                        var braceCount = 0;
                        var sentEnd = sentStart;
                        for (int j = sentStart; j < objectContent.Length; j++)
                        {
                            if (objectContent[j] == '{') braceCount++;
                            else if (objectContent[j] == '}') braceCount--;
                            if (braceCount == 0)
                            {
                                sentEnd = j + 1;
                                break;
                            }
                        }
                        if (sentEnd > sentStart)
                        {
                            logObj["sent"] = objectContent.Substring(sentStart, sentEnd - sentStart);
                        }
                    }
                }
                
                // Extract received - find JSON object by matching braces
                var receivedIndex = objectContent.IndexOf("[received]");
                if (receivedIndex >= 0)
                {
                    var receivedStart = objectContent.IndexOf('{', receivedIndex);
                    if (receivedStart >= 0)
                    {
                        var braceCount = 0;
                        var receivedEnd = receivedStart;
                        for (int j = receivedStart; j < objectContent.Length; j++)
                        {
                            if (objectContent[j] == '{') braceCount++;
                            else if (objectContent[j] == '}') braceCount--;
                            if (braceCount == 0)
                            {
                                receivedEnd = j + 1;
                                break;
                            }
                        }
                        if (receivedEnd > receivedStart)
                        {
                            logObj["received"] = objectContent.Substring(receivedStart, receivedEnd - receivedStart);
                        }
                    }
                }
                
                // Only add if we have at least id and school_id
                if (logObj["id"] != null && logObj["school_id"] != null)
                {
                    logArray.Add(logObj);
                }
            }
            
            return logArray;
        }

        /// <summary>
        /// Processes all regions. This method is designed to be called by Hangfire.
        /// </summary>
        public async Task ProcessAllRegions()
        {
            var regionIds = Enumerable.Range(1, 10).ToArray();

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Processing LogNmuhEmployee logs for region {regionId}...");
                    
                    // Read data from source API
                    using var sourceClient = new HttpClient();
                    var sourceUrl = $"https://api.emis.am/v1/dshh_log_nmuh_emploee/{regionId}";
                    
                    var response = await sourceClient.GetAsync(sourceUrl);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger?.LogWarning($"API returned status {response.StatusCode} for region {regionId}");
                        continue;
                    }
                    
                    var responseText = await response.Content.ReadAsStringAsync();
                    
                    // Log the raw response for debugging (first 500 chars)
                    _logger?.LogInformation($"API response for region {regionId} (first 500 chars): {responseText.Substring(0, Math.Min(500, responseText.Length))}");
                    
                    // Check if response is valid JSON (not HTML error page)
                    if (string.IsNullOrWhiteSpace(responseText))
                    {
                        _logger?.LogWarning($"API returned empty response for region {regionId}");
                        continue;
                    }
                    
                    var trimmedResponse = responseText.Trim();
                    JArray logArray;
                    
                    // Check if it's PHP array format (print_r output)
                    if (trimmedResponse.Contains("<pre>Array") || trimmedResponse.Contains("stdClass Object"))
                    {
                        _logger?.LogInformation($"API returned PHP array format for region {regionId}, attempting to parse...");
                        try
                        {
                            logArray = ParsePhpArrayFormat(responseText);
                            _logger?.LogInformation($"Successfully parsed PHP array format. Found {logArray.Count} logs for region {regionId}");
                        }
                        catch (Exception parseEx)
                        {
                            _logger?.LogError(parseEx, $"Failed to parse PHP array format for region {regionId}");
                            continue;
                        }
                    }
                    // Check if it's valid JSON
                    else if (trimmedResponse.StartsWith("[") || trimmedResponse.StartsWith("{"))
                    {
                        try
                        {
                            logArray = JArray.Parse(responseText);
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger?.LogError(jsonEx, $"Failed to parse JSON for region {regionId}. Response: {responseText.Substring(0, Math.Min(1000, responseText.Length))}");
                            continue;
                        }
                    }
                    else
                    {
                        _logger?.LogWarning($"API returned unknown format for region {regionId}. Response starts with: {trimmedResponse.Substring(0, Math.Min(100, trimmedResponse.Length))}");
                        continue;
                    }

                    if (logArray.Count == 0)
                    {
                        _logger?.LogInformation($"No logs found for region {regionId}");
                        continue;
                    }

                    var now = DateTime.UtcNow;
                    var addedCount = 0;
                    var updatedCount = 0;
                    var skippedCount = 0;

                    _logger?.LogInformation($"Found {logArray.Count} logs in API response for region {regionId}");

                    // Save each log to the database
                    foreach (var logToken in logArray)
                    {
                        if (logToken is not JObject logObj)
                        {
                            skippedCount++;
                            continue;
                        }

                        var logIdToken = logObj["id"];
                        if (logIdToken == null || !int.TryParse(logIdToken.ToString(), out var logId))
                        {
                            skippedCount++;
                            _logger?.LogWarning($"Skipping log entry - invalid or missing id in region {regionId}");
                            continue;
                        }

                        var schoolIdToken = logObj["school_id"];
                        if (schoolIdToken == null || !int.TryParse(schoolIdToken.ToString(), out var schoolId))
                        {
                            skippedCount++;
                            _logger?.LogWarning($"Skipping log entry {logId} - invalid or missing school_id in region {regionId}");
                            continue;
                        }

                        // Parse action_date
                        DateTime actionDate = now;
                        var actionDateStr = logObj["action_date"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(actionDateStr) && DateTime.TryParse(actionDateStr, out var parsedDate))
                        {
                            actionDate = parsedDate;
                        }

                        var method = logObj["method"]?.ToString() ?? string.Empty;
                        var sent = logObj["sent"]?.ToString() ?? string.Empty;
                        var received = logObj["received"]?.ToString() ?? string.Empty;

                        try
                        {
                            // Check if log already exists
                            var existingLog = await _context.LogNmuhEmployees
                                .FirstOrDefaultAsync(l => l.LogId == logId);

                            if (existingLog != null)
                            {
                                // Update existing log
                                existingLog.SchoolId = schoolId;
                                existingLog.ActionDate = actionDate;
                                existingLog.Method = method;
                                existingLog.Sent = sent;
                                existingLog.Received = received;
                                existingLog.UpdatedAt = now;
                                updatedCount++;
                            }
                            else
                            {
                                // Add new log
                                var newLog = new LogNmuhEmployee
                                {
                                    Id = Guid.NewGuid(),
                                    LogId = logId,
                                    SchoolId = schoolId,
                                    ActionDate = actionDate,
                                    Method = method,
                                    Sent = sent,
                                    Received = received,
                                    Transferred = false,
                                    CreatedAt = now,
                                    UpdatedAt = now
                                };
                                _context.LogNmuhEmployees.Add(newLog);
                                addedCount++;
                            }
                        }
                        catch (Exception dbEx)
                        {
                            _logger?.LogError(dbEx, $"Database error processing log {logId} for region {regionId}");
                            skippedCount++;
                        }
                    }

                    // Save changes with error handling
                    try
                    {
                        if (addedCount > 0 || updatedCount > 0)
                        {
                            var savedCount = await _context.SaveChangesAsync();
                            _logger?.LogInformation($"Region {regionId} processed. Added: {addedCount}, Updated: {updatedCount}, Skipped: {skippedCount}, Saved: {savedCount}");
                        }
                        else
                        {
                            _logger?.LogInformation($"Region {regionId} processed. No changes to save. Skipped: {skippedCount}");
                        }
                    }
                    catch (Exception saveEx)
                    {
                        _logger?.LogError(saveEx, $"Failed to save changes for region {regionId}. Added: {addedCount}, Updated: {updatedCount}");
                        throw; // Re-throw to be caught by outer catch
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while processing LogNmuhEmployee logs for region {regionId}");
                }
            }

            _logger?.LogInformation("LogNmuhEmployee processing completed for all regions.");
        }

        /// <summary>
        /// Reads log nmuh employee data from the API and saves it to the database
        /// Transfer logic is commented out for testing
        /// </summary>
        [HttpPost("transfer/{regionId}")]
        public async Task<IActionResult> TransferLogNmuhEmployees([FromRoute] int regionId)
        {
            try
            {
                // Get transfer API URL from configuration (commented out - not used for now)
                // TODO: Add this to appsettings.json: "TransferApi:LogNmuhEmployeeUrl": "https://your-api-endpoint.com/api/log-nmuh-employee"
                // var transferApiUrl = _configuration?["TransferApi:LogNmuhEmployeeUrl"] 
                //     ?? "https://your-api-endpoint.com/api/log-nmuh-employee"; // Placeholder - replace with actual API URL

                // Read data from source API
                using var sourceClient = new HttpClient();
                var sourceUrl = $"https://api.emis.am/v1/dshh_log_nmuh_emploee/{regionId}";
                
                var response = await sourceClient.GetAsync(sourceUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new
                    {
                        error = "API request failed",
                        statusCode = response.StatusCode,
                        regionId = regionId
                    });
                }
                
                var responseText = await response.Content.ReadAsStringAsync();
                
                // Check if response is valid JSON (not HTML error page)
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    return StatusCode(500, new
                    {
                        error = "API returned empty response",
                        regionId = regionId
                    });
                }
                
                var trimmedResponse = responseText.Trim();
                JArray logArray;
                
                // Check if it's PHP array format (print_r output)
                if (trimmedResponse.Contains("<pre>Array") || trimmedResponse.Contains("stdClass Object"))
                {
                    try
                    {
                        logArray = ParsePhpArrayFormat(responseText);
                    }
                    catch (Exception parseEx)
                    {
                        return StatusCode(500, new
                        {
                            error = "Failed to parse PHP array format",
                            message = parseEx.Message,
                            regionId = regionId
                        });
                    }
                }
                // Check if it's valid JSON
                else if (trimmedResponse.StartsWith("[") || trimmedResponse.StartsWith("{"))
                {
                    try
                    {
                        logArray = JArray.Parse(responseText);
                    }
                    catch (JsonException jsonEx)
                    {
                        return StatusCode(500, new
                        {
                            error = "Failed to parse JSON response",
                            message = jsonEx.Message,
                            regionId = regionId,
                            responsePreview = responseText?.Substring(0, Math.Min(1000, responseText?.Length ?? 0))
                        });
                    }
                }
                else
                {
                    return StatusCode(500, new
                    {
                        error = "API returned unknown format",
                        regionId = regionId,
                        responsePreview = trimmedResponse.Substring(0, Math.Min(200, trimmedResponse.Length))
                    });
                }

                if (logArray.Count == 0)
                {
                    return Ok(new
                    {
                        message = "No logs found",
                        regionId = regionId,
                        saved = 0
                    });
                }

                var now = DateTime.UtcNow;
                var addedCount = 0;
                var updatedCount = 0;

                // Save each log to the database
                foreach (var logToken in logArray)
                {
                    if (logToken is not JObject logObj)
                        continue;

                    var logIdToken = logObj["id"];
                    if (logIdToken == null || !int.TryParse(logIdToken.ToString(), out var logId))
                        continue;

                    var schoolIdToken = logObj["school_id"];
                    if (schoolIdToken == null || !int.TryParse(schoolIdToken.ToString(), out var schoolId))
                        continue;

                    // Parse action_date
                    DateTime actionDate = now;
                    var actionDateStr = logObj["action_date"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(actionDateStr) && DateTime.TryParse(actionDateStr, out var parsedDate))
                    {
                        actionDate = parsedDate;
                    }

                    var method = logObj["method"]?.ToString() ?? string.Empty;
                    var sent = logObj["sent"]?.ToString() ?? string.Empty;
                    var received = logObj["received"]?.ToString() ?? string.Empty;

                    // Check if log already exists
                    var existingLog = await _context.LogNmuhEmployees
                        .FirstOrDefaultAsync(l => l.LogId == logId);

                    if (existingLog != null)
                    {
                        // Update existing log
                        existingLog.SchoolId = schoolId;
                        existingLog.ActionDate = actionDate;
                        existingLog.Method = method;
                        existingLog.Sent = sent;
                        existingLog.Received = received;
                        existingLog.UpdatedAt = now;
                        updatedCount++;
                    }
                    else
                    {
                        // Add new log
                        var newLog = new LogNmuhEmployee
                        {
                            Id = Guid.NewGuid(),
                            LogId = logId,
                            SchoolId = schoolId,
                            ActionDate = actionDate,
                            Method = method,
                            Sent = sent,
                            Received = received,
                            Transferred = false,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        _context.LogNmuhEmployees.Add(newLog);
                        addedCount++;
                    }

                    // ===== TRANSFER LOGIC - COMMENTED OUT FOR TESTING =====
                    /*
                    try
                    {
                        // Prepare the payload from the source data
                        var payload = new
                        {
                            id = logObj["id"]?.ToObject<int>(),
                            school_id = logObj["school_id"]?.ToObject<int>(),
                            action_date = logObj["action_date"]?.ToString(),
                            method = logObj["method"]?.ToString(),
                            sent = logObj["sent"] != null ? JsonConvert.DeserializeObject(logObj["sent"].ToString()) : null,
                            received = logObj["received"] != null ? JsonConvert.DeserializeObject(logObj["received"].ToString()) : null
                        };

                        var jsonPayload = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        // Send POST request to transfer API
                        var response = await _httpClient.PostAsync(transferApiUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            successCount++;
                            _logger?.LogInformation($"Successfully transferred log ID: {logObj["id"]}");
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            var errorMsg = $"Log ID {logObj["id"]}: HTTP {response.StatusCode}: {errorContent}";
                            errors.Add(errorMsg);
                            failCount++;
                            _logger?.LogWarning(errorMsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        var logId = logObj["id"]?.ToString() ?? "unknown";
                        var errorMsg = $"Log ID {logId}: {ex.Message}";
                        errors.Add(errorMsg);
                        failCount++;
                        _logger?.LogError(ex, $"Error transferring log {logId}");
                    }
                    */
                    // ===== END OF COMMENTED TRANSFER LOGIC =====
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Logs saved to database successfully",
                    regionId = regionId,
                    totalProcessed = logArray.Count,
                    logsAdded = addedCount,
                    logsUpdated = updatedCount
                    // Transfer results (commented out)
                    // successful = successCount,
                    // failed = failCount,
                    // errors = errors.Any() ? errors : null
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error processing LogNmuhEmployee for region {regionId}");
                return StatusCode(500, new
                {
                    error = "Processing failed",
                    message = ex.Message,
                    regionId = regionId
                });
            }
        }

        /// <summary>
        /// Reads log nmuh employee data from the API and saves all regions
        /// </summary>
        [HttpPost("transfer-all")]
        public async Task<IActionResult> TransferAllRegions()
        {
            var regionIds = Enumerable.Range(1, 10).ToArray();
            var results = new List<object>();

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Processing logs for region {regionId}...");
                    
                    // Call the transfer endpoint for each region
                    var result = await TransferLogNmuhEmployees(regionId);
                    
                    if (result is OkObjectResult okResult)
                    {
                        results.Add(okResult.Value);
                    }
                    else
                    {
                        results.Add(new
                        {
                            regionId = regionId,
                            error = "Processing failed"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while processing logs for region {regionId}");
                    results.Add(new
                    {
                        regionId = regionId,
                        error = ex.Message
                    });
                }
            }

            var totalAdded = results.OfType<dynamic>()
                .Where(r => r.logsAdded != null)
                .Sum(r => (int)r.logsAdded);

            var totalUpdated = results.OfType<dynamic>()
                .Where(r => r.logsUpdated != null)
                .Sum(r => (int)r.logsUpdated);

            return Ok(new
            {
                message = "Processing completed for all regions",
                totalRegions = regionIds.Length,
                totalAdded = totalAdded,
                totalUpdated = totalUpdated,
                results = results
            });
        }

        /// <summary>
        /// Gets all log nmuh employees from database
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLogNmuhEmployees(
            [FromQuery] int? schoolId = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            try
            {
                var query = _context.LogNmuhEmployees.AsQueryable();

                if (schoolId.HasValue)
                {
                    query = query.Where(l => l.SchoolId == schoolId.Value);
                }

                var totalCount = await query.CountAsync();
                var logs = await query
                    .OrderByDescending(l => l.ActionDate)
                    .Skip(skip)
                    .Take(take)
                    .Select(l => new
                    {
                        l.Id,
                        l.LogId,
                        l.SchoolId,
                        l.ActionDate,
                        l.Method,
                        l.Sent,
                        l.Received,
                        l.Transferred,
                        l.TransferredAt,
                        l.TransferError,
                        l.CreatedAt,
                        l.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    total = totalCount,
                    count = logs.Count,
                    data = logs
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting log nmuh employees");
                return StatusCode(500, new
                {
                    error = "Failed to get log nmuh employees",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Test endpoint to verify database connection and table exists
        /// </summary>
        [HttpGet("test-db")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var count = await _context.LogNmuhEmployees.CountAsync();
                var sample = await _context.LogNmuhEmployees
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(5)
                    .Select(l => new
                    {
                        l.Id,
                        l.LogId,
                        l.SchoolId,
                        l.Method,
                        l.ActionDate,
                        l.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Database connection successful",
                    tableExists = true,
                    totalRecords = count,
                    sampleRecords = sample
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Database error",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Gets a specific log nmuh employee by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLogNmuhEmployee(Guid id)
        {
            try
            {
                var log = await _context.LogNmuhEmployees.FindAsync(id);

                if (log == null)
                {
                    return NotFound(new { message = "Log nmuh employee not found" });
                }

                return Ok(new
                {
                    log.Id,
                    log.LogId,
                    log.SchoolId,
                    log.ActionDate,
                    log.Method,
                    log.Sent,
                    log.Received,
                    log.Transferred,
                    log.TransferredAt,
                    log.TransferError,
                    log.CreatedAt,
                    log.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting log nmuh employee {id}");
                return StatusCode(500, new
                {
                    error = "Failed to get log nmuh employee",
                    message = ex.Message
                });
            }
        }
    }
}

