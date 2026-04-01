using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    public class NmuhStaffSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int StaffProcessed { get; set; }
        public int StaffAdded { get; set; }
        public int StaffUpdated { get; set; }
        public List<NmuhStaffDto> StaffUpdatedList { get; set; } = new();
    }

    public class NmuhStaffChangedEntitiesDto
    {
        public List<NmuhStaffDto> StaffUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class NmuhStaffController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISyncReportService _syncReportService;
        private readonly ILogger<NmuhStaffController>? _logger;

        public NmuhStaffController(AppDbContext context, ISyncReportService syncReportService, ILogger<NmuhStaffController>? logger = null)
        {
            _context = context;
            _syncReportService = syncReportService;
            _logger = logger;
        }

        private NmuhStaffDto MapToNmuhStaffDto(NmuhStaff staff)
        {
            return new NmuhStaffDto
            {
                Id = staff.Id,
                NmuhStaffId = staff.NmuhStaffId,
                InstId = staff.InstId,
                InternalSchoolId = staff.InternalSchoolId,
                RegionId = staff.RegionId,
                InstName = staff.InstName,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                FatherName = staff.FatherName,
                DateOfBirth = staff.DateOfBirth,
                SocNumber = staff.SocNumber,
                Sex = staff.Sex,
                Address = staff.Address,
                Phone = staff.Phone,
                Citizenship = staff.Citizenship,
                Nationality = staff.Nationality,
                IdentDocument = staff.IdentDocument,
                IdentDocumentNumber = staff.IdentDocumentNumber,
                FromCountry = staff.FromCountry,
                InFiz = staff.InFiz,
                Druyq = staff.Druyq,
                PartlyIds = staff.PartlyIds,
                PartlyInstNames = staff.PartlyInstNames,
                PositionName = staff.PositionName,
                PositionId = staff.PositionId,
                PositionDetailId = staff.PositionDetailId,
                PositionDetailName = staff.PositionDetailName,
                GroupIds = staff.GroupIds,
                Groups = staff.Groups.Select(g => new NmuhStaffGroupDto
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    Subjects = g.Subjects.Select(s => new NmuhSubjectDto
                    {
                        SubjectId = s.SubjectId,
                        SubjectName = s.SubjectName,
                        SubjectType = s.SubjectType,
                        SubjectTypeId = s.SubjectTypeId
                    }).ToList()
                }).ToList(),
                CreatedAt = staff.CreatedAt,
                UpdatedAt = staff.UpdatedAt
            };
        }

        private static bool ParseSex(string? raw) => raw == "1";

        private static List<int> ParseGroupIds(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<int>();
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id != 0)
                .ToList();
        }

        private static List<NmuhStaffGroup> ParseGroups(string? groupsJson)
        {
            if (string.IsNullOrWhiteSpace(groupsJson)) return new List<NmuhStaffGroup>();
            try
            {
                var arr = JArray.Parse(groupsJson);
                return arr.OfType<JObject>().Select(g => new NmuhStaffGroup
                {
                    Id = Guid.NewGuid(),
                    GroupId = int.TryParse(g["group_id"]?.ToString(), out var gid) ? gid : 0,
                    GroupName = g["group"]?.ToString() ?? string.Empty,
                    Subjects = (g["subjects"] as JArray ?? new JArray())
                        .OfType<JObject>()
                        .Select(s => new NmuhSubject
                        {
                            Id = Guid.NewGuid(),
                            SubjectId = int.TryParse(s["subject_id"]?.ToString(), out var sid) ? sid : 0,
                            SubjectName = s["subject"]?.ToString() ?? string.Empty,
                            SubjectType = s["subject_type"]?.ToString() ?? string.Empty,
                            SubjectTypeId = int.TryParse(s["subject_type_id"]?.ToString(), out var stid) ? stid : 0,
                            Grade = int.TryParse(s["grade"]?.ToString(), out var gr) ? gr : 0,
                            SubGrade = int.TryParse(s["sub_grade"]?.ToString(), out var sgr) ? sgr : 0
                        }).ToList()
                }).ToList();
            }
            catch { return new List<NmuhStaffGroup>(); }
        }


        [NonAction]
        public async Task SyncAllRegions()
        {
            var regionIds = Enumerable.Range(1, 11).ToArray();
            var results = new List<NmuhStaffSyncResult>();

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Syncing NmuhStaff region {regionId}...");
                    var result = await SyncRegionInternal(regionId);
                    results.Add(result);
                    
                    if (result.Success)
                    {
                        _logger?.LogInformation($"Region {regionId} synced successfully. " +
                            $"Staff: {result.StaffAdded} added, {result.StaffUpdated} updated.");
                    }
                    else
                    {
                        _logger?.LogError($"Region {regionId} sync failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while syncing NmuhStaff region {regionId}");
                    results.Add(new NmuhStaffSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new NmuhStaffSyncSummaryDto
            {
                SyncCompletedAt = DateTime.UtcNow,
                TotalRegionsProcessed = regionIds.Length,
                SuccessfulRegions = results.Count(r => r.Success),
                FailedRegions = results.Count(r => !r.Success),
                TotalStaffAdded = results.Sum(r => r.StaffAdded),
                TotalStaffUpdated = results.Sum(r => r.StaffUpdated)
            };

            foreach (var result in results.Where(r => r.Success))
            {
                summary.AllStaffUpdated.AddRange(result.StaffUpdatedList);
            }

            _logger?.LogInformation($"NmuhStaff sync completed for all regions. " +
                $"Success: {summary.SuccessfulRegions}/{regionIds.Length}. " +
                $"Total - Staff: {summary.TotalStaffAdded} added, {summary.TotalStaffUpdated} updated.");
        }

        private async Task<NmuhStaffSyncResult> SyncRegionInternal(int regionId)
        {
            var result = new NmuhStaffSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                using var client = new HttpClient();
                var url = $"https://api.emis.am/v1/get_nmuh_staff_by_marz/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var staffArray = JArray.Parse(responseText);

                // Clear staging table for this sync
                await _context.NmuhStaffStaging.ExecuteDeleteAsync();

                var now = DateTime.UtcNow;

                // Process all staff from API into staging
                foreach (var staffToken in staffArray)
                {
                    if (staffToken is not JObject staffObj)
                        continue;

                    if (!int.TryParse(staffObj["staff_id"]?.ToString(), out var staffIdStr))
                        continue;

                    if (!int.TryParse(staffObj["inst_id"]?.ToString(), out var instId))
                        instId = 0;
                    var instName = staffObj["inst_name"]?.ToString() ?? string.Empty;
                    var firstName = staffObj["first_name"]?.ToString() ?? string.Empty;
                    var lastName = staffObj["last_name"]?.ToString() ?? string.Empty;
                    var fatherName = staffObj["father_name"]?.ToString() ?? string.Empty;
                    var socNumber = staffObj["soc_number"]?.ToString() ?? string.Empty;
                    var sexRaw = staffObj["sex"]?.ToString() ?? string.Empty;
                    var sex = ParseSex(sexRaw);
                    var address = staffObj["address"]?.ToString() ?? string.Empty;
                    var phone = staffObj["phone"]?.ToString() ?? string.Empty;
                    var citizenship = staffObj["citizenship"]?.ToString() ?? string.Empty;
                    var nationality = staffObj["nationality"]?.ToString() ?? string.Empty;
                    var identDocument = staffObj["ident_document"]?.ToString() ?? string.Empty;
                    var identDocumentNumber = staffObj["ident_document_number"]?.ToString() ?? string.Empty;
                    var fromCountry = staffObj["from_country"]?.ToString() ?? string.Empty;
                    var inFiz = staffObj["in_fiz"]?.ToString() ?? string.Empty;
                    var druyq = staffObj["druyq"]?.ToString() ?? string.Empty;
                    var partlyIds = staffObj["partly_ids"]?.ToString();
                    var partlyInstNames = staffObj["partly_inst_names"]?.ToString();
                    var positionName = staffObj["position_name"]?.ToString() ?? string.Empty;
                    var positionId = staffObj["position_id"]?.ToString() ?? string.Empty;
                    var positionDetailId = staffObj["position_detail_id"]?.ToString() ?? string.Empty;
                    var positionDetailName = staffObj["position_detail_name"]?.ToString() ?? string.Empty;
                    var groupIdRaw = staffObj["group_id"]?.ToString(); // raw comma-separated string kept for MD5 + staging

                    // Parse date of birth
                    DateOnly dateOfBirth = default;
                    var dobString = staffObj["date_of_birth"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(dobString) && DateOnly.TryParse(dobString, out var parsedDate))
                    {
                        dateOfBirth = parsedDate;
                    }

                    // Serialize groups array to JSON string (if present)
                    var groupsJson = string.Empty;
                    var groupsToken = staffObj["groups"];
                    if (groupsToken != null && groupsToken.Type == JTokenType.Array)
                    {
                        groupsJson = groupsToken.ToString(Formatting.None);
                    }

                    // Compute MD5
                    var md5 = SyncHelpers.ComputeMd5(
                        staffIdStr.ToString(),
                        instId.ToString(),
                        instName,
                        firstName,
                        lastName,
                        fatherName,
                        dateOfBirth.ToString("yyyy-MM-dd"),
                        socNumber,
                        sexRaw,
                        address,
                        phone,
                        citizenship,
                        nationality,
                        identDocument,
                        identDocumentNumber,
                        fromCountry,
                        inFiz,
                        druyq,
                        partlyIds,
                        partlyInstNames,
                        positionName,
                        positionId,
                        positionDetailId,
                        positionDetailName,
                        groupIdRaw,
                        groupsJson);

                    // Stream directly into staging table
                    _context.NmuhStaffStaging.Add(new NmuhStaffStaging
                    {
                        Id = Guid.NewGuid(),
                        NmuhStaffId = staffIdStr,
                        InstId = instId,
                        InternalSchoolId = null,
                        RegionId = regionId,
                        InstName = instName,
                        FirstName = firstName,
                        LastName = lastName,
                        FatherName = fatherName,
                        DateOfBirth = dateOfBirth,
                        SocNumber = socNumber,
                        Sex = sex,
                        Address = address,
                        Phone = phone,
                        Citizenship = citizenship,
                        Nationality = nationality,
                        IdentDocument = identDocument,
                        IdentDocumentNumber = identDocumentNumber,
                        FromCountry = fromCountry,
                        InFiz = inFiz,
                        Druyq = druyq,
                        PartlyIds = partlyIds,
                        PartlyInstNames = partlyInstNames,
                        PositionName = positionName,
                        PositionId = positionId,
                        PositionDetailId = positionDetailId,
                        PositionDetailName = positionDetailName,
                        GroupId = groupIdRaw,       // raw string kept in staging
                        GroupsJson = groupsJson,    // raw JSON kept in staging
                        MD5 = md5,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                // Save all staging data
                await _context.SaveChangesAsync();

                // Load all staged staff
                var stagingRows = await _context.NmuhStaffStaging.ToListAsync();

                // Build InstId → NmuhInstitution.Id lookup for InternalSchoolId resolution
                var instIds = stagingRows.Select(s => s.InstId).Distinct().ToList();
                var schoolLookup = await _context.NmuhInstitutions
                    .Where(i => instIds.Contains(i.InstId))
                    .ToDictionaryAsync(i => i.InstId, i => i.Id);

                // Get existing staff by NmuhStaffId
                var stagedIds = stagingRows.Select(s => s.NmuhStaffId).Distinct().ToList();
                var existingStaff = await _context.NmuhStaff
                    .Where(s => stagedIds.Contains(s.NmuhStaffId))
                    .ToListAsync();

                var existingDict = existingStaff.ToDictionary(s => s.NmuhStaffId);

                var newStaff = new List<NmuhStaff>();
                var updatedCount = 0;

                // Process each staged staff
                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.NmuhStaffId, out var existing))
                    {
                        // Compare MD5
                        if (!string.Equals(existing.MD5, staging.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            // MD5 changed → update from staging
                            existing.InstId = staging.InstId;
                            existing.InternalSchoolId = schoolLookup.TryGetValue(staging.InstId, out var updatedSid)
                                ? updatedSid
                                : existing.InternalSchoolId;
                            existing.RegionId = staging.RegionId;
                            existing.InstName = staging.InstName;
                            existing.FirstName = staging.FirstName;
                            existing.LastName = staging.LastName;
                            existing.FatherName = staging.FatherName;
                            existing.DateOfBirth = staging.DateOfBirth;
                            existing.SocNumber = staging.SocNumber;
                            existing.Sex = staging.Sex;
                            existing.Address = staging.Address;
                            existing.Phone = staging.Phone;
                            existing.Citizenship = staging.Citizenship;
                            existing.Nationality = staging.Nationality;
                            existing.IdentDocument = staging.IdentDocument;
                            existing.IdentDocumentNumber = staging.IdentDocumentNumber;
                            existing.FromCountry = staging.FromCountry;
                            existing.InFiz = staging.InFiz;
                            existing.Druyq = staging.Druyq;
                            existing.PartlyIds = staging.PartlyIds;
                            existing.PartlyInstNames = staging.PartlyInstNames;
                            existing.PositionName = staging.PositionName;
                            existing.PositionId = staging.PositionId;
                            existing.PositionDetailId = staging.PositionDetailId;
                            existing.PositionDetailName = staging.PositionDetailName;
                            existing.GroupIds = ParseGroupIds(staging.GroupId);
                            existing.MD5 = staging.MD5;
                            existing.UpdatedAt = DateTime.UtcNow;

                            // Replace groups: delete old (cascade removes subjects), insert new
                            await _context.NmuhStaffGroups
                                .Where(g => g.NmuhStaffId == existing.Id)
                                .ExecuteDeleteAsync();
                            var updatedGroups = ParseGroups(staging.GroupsJson);
                            foreach (var g in updatedGroups) g.NmuhStaffId = existing.Id;
                            _context.NmuhStaffGroups.AddRange(updatedGroups);

                            updatedCount++;
                            result.StaffUpdatedList.Add(MapToNmuhStaffDto(existing));
                        }
                    }
                    else
                    {
                        // New staff
                        var newStaffMember = new NmuhStaff
                        {
                            Id = Guid.NewGuid(),
                            NmuhStaffId = staging.NmuhStaffId,
                            InstId = staging.InstId,
                            InternalSchoolId = schoolLookup.TryGetValue(staging.InstId, out var newSid)
                                ? newSid
                                : null,
                            RegionId = staging.RegionId,
                            InstName = staging.InstName,
                            FirstName = staging.FirstName,
                            LastName = staging.LastName,
                            FatherName = staging.FatherName,
                            DateOfBirth = staging.DateOfBirth,
                            SocNumber = staging.SocNumber,
                            Sex = staging.Sex,
                            Address = staging.Address,
                            Phone = staging.Phone,
                            Citizenship = staging.Citizenship,
                            Nationality = staging.Nationality,
                            IdentDocument = staging.IdentDocument,
                            IdentDocumentNumber = staging.IdentDocumentNumber,
                            FromCountry = staging.FromCountry,
                            InFiz = staging.InFiz,
                            Druyq = staging.Druyq,
                            PartlyIds = staging.PartlyIds,
                            PartlyInstNames = staging.PartlyInstNames,
                            PositionName = staging.PositionName,
                            PositionId = staging.PositionId,
                            PositionDetailId = staging.PositionDetailId,
                            PositionDetailName = staging.PositionDetailName,
                            GroupIds = ParseGroupIds(staging.GroupId),
                            Groups = ParseGroups(staging.GroupsJson),
                            MD5 = staging.MD5,
                            CreatedAt = staging.CreatedAt,
                            UpdatedAt = staging.UpdatedAt
                        };

                        newStaff.Add(newStaffMember);
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] NmuhStaff MD5 compare done. New={newStaff.Count}, Updated={updatedCount}. Saving...");

                // Add new staff
                if (newStaff.Any())
                {
                    _context.NmuhStaff.AddRange(newStaff);
                }

                // Save changes
                if (updatedCount > 0 || newStaff.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Cleanup staging
                await _context.NmuhStaffStaging.ExecuteDeleteAsync();

                result.Success = true;
                result.StaffProcessed = stagingRows.Count;
                result.StaffAdded = newStaff.Count;
                result.StaffUpdated = updatedCount;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger?.LogError(ex, $"Error syncing NmuhStaff for region {regionId}");
                return result;
            }
        }

        [HttpPost("sync/{regionId?}")]
        public async Task<IActionResult> SyncStaff([FromRoute] int regionId = 1)
        {
            var result = await SyncRegionInternal(regionId);

            if (result.Success)
            {
                return Ok(new
                {
                    message = "NmuhStaff sync completed successfully!",
                    regionId = result.RegionId,
                    staffProcessed = result.StaffProcessed,
                    staffAdded = result.StaffAdded,
                    staffUpdated = result.StaffUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "NmuhStaff sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestStaffSync()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<NmuhStaffSyncSummaryDto>(
                    "NmuhStaffSyncReports", "nmuh-staff-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No NmuhStaff sync file found. Run a sync first." });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load NmuhStaff sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestStaffChanges()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<NmuhStaffSyncSummaryDto>(
                    "NmuhStaffSyncReports", "nmuh-staff-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No NmuhStaff sync file found. Run a sync first." });

                return Ok(new NmuhStaffChangedEntitiesDto { StaffUpdated = summary.AllStaffUpdated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load NmuhStaff changes", message = ex.Message });
            }
        }
    }
}

