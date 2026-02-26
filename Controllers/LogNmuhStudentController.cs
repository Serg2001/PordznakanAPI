using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Models;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogNmuhStudentController : ControllerBase
    {
        private readonly ILogTransferService _logTransferService;
        private const string SourceBaseUrl = "https://api.emis.am/v1/dshh_log_nmuh_students";

        public LogNmuhStudentController(ILogTransferService logTransferService)
        {
            _logTransferService = logTransferService;
        }

        private static LogNmuhStudent MapToModel(JObject o, DateTime fallbackDate)
        {
            int.TryParse(o["id"]?.ToString(), out var logId);
            int.TryParse(o["school_id"]?.ToString(), out var schoolId);
            DateTime.TryParse(o["action_date"]?.ToString(), out var actionDate);

            return new LogNmuhStudent
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
            _logTransferService.ProcessAllRegionsAsync(SourceBaseUrl, nameof(LogNmuhStudent), MapToModel);

        [HttpPost("process/{regionId}")]
        public async Task<IActionResult> ProcessRegion([FromRoute] int regionId)
        {
            try
            {
                var logArray = await _logTransferService.FetchLogsForRegionAsync(SourceBaseUrl, regionId);
                var now = DateTime.UtcNow;
                var logs = logArray.OfType<JObject>().Select(o => MapToModel(o, now)).ToList();

                if (logs.Count == 0)
                    return Ok(new { message = "No logs found", regionId });

                await _logTransferService.SendBulkAsync(logs);
                return Ok(new { message = "Logs sent to bulk-update API", regionId, count = logs.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, regionId });
            }
        }

        [HttpPost("process-all")]
        public async Task<IActionResult> ProcessAll()
        {
            try
            {
                await ProcessAllRegions();
                return Ok(new { message = "All regions processed and sent to bulk-update API" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
