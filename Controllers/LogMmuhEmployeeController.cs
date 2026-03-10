using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Models;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogMmuhEmployeeController : ControllerBase
    {
        private readonly ILogTransferService _logTransferService;
        private const string SourceBaseUrl = "https://api.emis.am/v1/dshh_log_mmuh_emploee";

        public LogMmuhEmployeeController(ILogTransferService logTransferService)
        {
            _logTransferService = logTransferService;
        }

        private static LogMmuhEmployee MapToModel(JObject o, DateTime fallbackDate)
        {
            int.TryParse(o["id"]?.ToString(), out var logId);
            int.TryParse(o["school_id"]?.ToString(), out var schoolId);
            DateTime.TryParse(o["action_date"]?.ToString(), out var actionDate);

            return new LogMmuhEmployee
            {
                LogId = logId,
                SchoolId = schoolId,
                ActionDate = actionDate == default ? fallbackDate : actionDate,
                Method = o["method"]?.ToString() ?? string.Empty,
                Sent = o["sent"]?.ToString() ?? string.Empty,
                Received = o["received"]?.ToString() ?? string.Empty,
            };
        }

        public Task ProcessAllRegions() =>
            ProcessAllRegions(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        public Task ProcessAllRegions(DateOnly date) =>
            _logTransferService.ProcessAllRegionsAsync(SourceBaseUrl, nameof(LogMmuhEmployee), date, MapToModel);

        [HttpPost("process/{regionId}")]
        public async Task<IActionResult> ProcessRegion(
            [FromRoute] int regionId,
            [FromQuery] DateOnly? date = null)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            try
            {
                var logArray = await _logTransferService.FetchLogsForRegionAsync(SourceBaseUrl, regionId, targetDate);
                var now = DateTime.UtcNow;
                var logs = logArray.OfType<JObject>().Select(o => MapToModel(o, now)).ToList();

                if (logs.Count == 0)
                    return Ok(new { message = "No logs found", regionId, date = targetDate });

                await _logTransferService.SendBulkAsync(logs);
                return Ok(new { message = "Logs sent to bulk-update API", regionId, date = targetDate, count = logs.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, regionId, date = targetDate });
            }
        }

        [HttpPost("process-all")]
        public async Task<IActionResult> ProcessAll([FromQuery] DateOnly? date = null)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            try
            {
                await ProcessAllRegions(targetDate);
                return Ok(new { message = "All regions processed and sent to bulk-update API", date = targetDate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, date = targetDate });
            }
        }
    }
}
