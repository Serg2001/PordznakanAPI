using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace PordznakanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolEmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SchoolEmployeeController>? _logger;

        private const string SourceBaseUrl = "https://api.emis.am/v1/get_personnel_list";

        public SchoolEmployeeController(AppDbContext context, ILogger<SchoolEmployeeController>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string ComputeMd5(params string?[] fields)
        {
            var raw = string.Join("|", fields.Select(f => f ?? string.Empty));
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// Expands one API person object into one SchoolEmployee row per position entry.
        /// Person with 2 entries in person_positions → 2 rows (with Position, StaffGroup, VacationId).
        /// Person with no person_positions → 1 row with empty position fields.
        /// </summary>
        private static List<SchoolEmployee> ExpandToRows(JObject o, int regionId, DateTime now)
        {
            int.TryParse(o["person_id"]?.ToString(), out var personId);
            int.TryParse(o["school_id"]?.ToString(), out var schoolId);

            var firstName     = o["first_name"]?.ToString()     ?? string.Empty;
            var lastName      = o["last_name"]?.ToString()      ?? string.Empty;
            var fatherName    = o["father_name"]?.ToString()    ?? string.Empty;
            var sex           = o["sex"]?.ToString()            ?? string.Empty;
            var socNumber     = o["soc_number"]?.ToString()     ?? string.Empty;
            var address       = o["address"]?.ToString()        ?? string.Empty;
            var phone         = o["phone"]?.ToString()          ?? string.Empty;
            var mainSubjectId = o["main_subject_id"]?.ToString();

            DateOnly? dateOfBirth = null;
            var dobStr = o["date_of_birth"]?.ToString();
            if (!string.IsNullOrWhiteSpace(dobStr) && DateOnly.TryParse(dobStr, out var dob))
                dateOfBirth = dob;

            // Parse every entry in person_positions, preserving position, staff_group, vacantion_id
            var entries = new List<(string Position, string StaffGroup, int? VacationId)>();
            var rawPositions = o["person_positions"]?.ToString();
            if (!string.IsNullOrWhiteSpace(rawPositions))
            {
                try
                {
                    entries = JArray.Parse(rawPositions)
                        .OfType<JObject>()
                        .Select(p =>
                        {
                            int? vacId = null;
                            if (int.TryParse(p["vacantion_id"]?.ToString(), out var v))
                                vacId = v;
                            return (
                                Position:   p["position"]?.ToString()    ?? string.Empty,
                                StaffGroup: p["staff_group"]?.ToString() ?? string.Empty,
                                VacationId: vacId
                            );
                        })
                        .ToList();
                }
                catch { /* leave empty on malformed JSON */ }
            }

            if (entries.Count == 0)
                entries.Add((string.Empty, string.Empty, null));

            return entries.Select(entry =>
            {
                var md5 = ComputeMd5(
                    personId.ToString(), schoolId.ToString(), regionId.ToString(),
                    firstName, lastName, fatherName, sex, socNumber,
                    dobStr, address, phone, mainSubjectId,
                    entry.Position, entry.StaffGroup, entry.VacationId?.ToString());

                return new SchoolEmployee
                {
                    Id            = Guid.NewGuid(),
                    PersonId      = personId,
                    SchoolId      = schoolId,
                    RegionId      = regionId,
                    FirstName     = firstName,
                    LastName      = lastName,
                    FatherName    = fatherName,
                    Sex           = sex,
                    SocNumber     = socNumber,
                    DateOfBirth   = dateOfBirth,
                    Address       = address,
                    Phone         = phone,
                    MainSubjectId = mainSubjectId,
                    Position      = entry.Position,
                    StaffGroup    = entry.StaffGroup,
                    VacationId    = entry.VacationId,
                    MD5           = md5,
                    CreatedAt     = now,
                    UpdatedAt     = now
                };
            }).ToList();
        }

        // ── Hangfire entry point ──────────────────────────────────────────────

        [NonAction]
        public async Task SyncAllRegions()
        {
            foreach (var regionId in Enumerable.Range(1, 10))
            {
                try
                {
                    await SyncRegionInternal(regionId);
                    _logger?.LogInformation($"[SchoolEmployee] Region {regionId} synced.");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"[SchoolEmployee] Region {regionId} sync failed.");
                }
            }
        }

        // ── Core sync logic ───────────────────────────────────────────────────

        private async Task<(int added, int updated)> SyncRegionInternal(int regionId)
        {
            using var client = new HttpClient();
            var url = $"{SourceBaseUrl}/{regionId}";
            var responseText = await client.GetStringAsync(url);

            var token = JToken.Parse(responseText);

            JArray? array = token as JArray;
            if (array == null && token is JObject obj)
            {
                array = obj["data"] as JArray
                     ?? obj["results"] as JArray
                     ?? obj["items"] as JArray;

                if (array == null)
                {
                    foreach (var prop in obj.Properties())
                    {
                        if (prop.Value.Type == JTokenType.Array)
                        {
                            array = (JArray)prop.Value;
                            break;
                        }
                    }
                }
            }

            if (array == null)
                throw new Exception($"[SchoolEmployee] Cannot parse array from API response for region {regionId}.");

            var now = DateTime.UtcNow;

            // One API person → one row per position entry, deduplicated by (PersonId, VacationId)
            var incoming = array
                .OfType<JObject>()
                .SelectMany(o => ExpandToRows(o, regionId, now))
                .GroupBy(e => (e.PersonId, e.VacationId))
                .Select(g => g.First())
                .ToList();

            // Delete all existing rows for this region and re-insert fresh data.
            // This is simpler and avoids any duplicate-key edge cases.
            await _context.SchoolEmployees
                .Where(e => e.RegionId == regionId)
                .ExecuteDeleteAsync();

            _context.SchoolEmployees.AddRange(incoming);
            await _context.SaveChangesAsync();

            _logger?.LogInformation($"[SchoolEmployee] Region {regionId}: {incoming.Count} records saved.");
            return (incoming.Count, 0);
        }

        // ── HTTP endpoints ────────────────────────────────────────────────────

        /// <summary>
        /// Fetches all personnel from the external API for the given region and saves them.
        /// </summary>
        [HttpPost("sync/{regionId:int}")]
        public async Task<IActionResult> Sync([FromRoute] int regionId)
        {
            try
            {
                var (added, updated) = await SyncRegionInternal(regionId);
                return Ok(new { message = "Sync completed", regionId, added, updated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, regionId });
            }
        }

        /// <summary>
        /// Returns all school employees for the given region.
        /// </summary>
        [HttpGet("by-region/{regionId:int}")]
        public async Task<IActionResult> GetByRegion([FromRoute] int regionId)
        {
            var employees = await _context.SchoolEmployees
                .Where(e => e.RegionId == regionId)
                .Select(e => new
                {
                    e.Id,
                    e.PersonId,
                    e.SchoolId,
                    e.RegionId,
                    e.FirstName,
                    e.LastName,
                    e.FatherName,
                    e.Sex,
                    e.SocNumber,
                    e.DateOfBirth,
                    e.Address,
                    e.Phone,
                    e.MainSubjectId,
                    e.Position,
                    e.StaffGroup,
                    e.VacationId,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .ToListAsync();

            return Ok(employees);
        }

        /// <summary>
        /// Returns all school employees for the given school (by SchoolId).
        /// </summary>
        [HttpGet("by-school/{schoolId:int}")]
        public async Task<IActionResult> GetBySchool([FromRoute] int schoolId)
        {
            var employees = await _context.SchoolEmployees
                .Where(e => e.SchoolId == schoolId)
                .Select(e => new
                {
                    e.Id,
                    e.PersonId,
                    e.SchoolId,
                    e.RegionId,
                    e.FirstName,
                    e.LastName,
                    e.FatherName,
                    e.Sex,
                    e.SocNumber,
                    e.DateOfBirth,
                    e.Address,
                    e.Phone,
                    e.MainSubjectId,
                    e.Position,
                    e.StaffGroup,
                    e.VacationId,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .ToListAsync();

            return Ok(employees);
        }
    }
}
