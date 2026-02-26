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
    public class MmuhStaffSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int StaffProcessed { get; set; }
        public int StaffAdded { get; set; }
        public int StaffUpdated { get; set; }
        public List<MmuhStaffDto> StaffUpdatedList { get; set; } = new();
    }

    public class MmuhStaffChangedEntitiesDto
    {
        public List<MmuhStaffDto> StaffUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MmuhStaffController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISyncReportService _syncReportService;
        private readonly ILogger<MmuhStaffController>? _logger;

        public MmuhStaffController(AppDbContext context, ISyncReportService syncReportService, ILogger<MmuhStaffController>? logger = null)
        {
            _context = context;
            _syncReportService = syncReportService;
            _logger = logger;
        }

        private MmuhStaffDto MapToMmuhStaffDto(MmuhStaff staff)
        {
            return new MmuhStaffDto
            {
                Id = staff.Id,
                MmuhStaffId = staff.MmuhStaffId,
                InstId = staff.InstId,
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
                GroupId = staff.GroupId,
                GroupsJson = staff.GroupsJson,
                CreatedAt = staff.CreatedAt,
                UpdatedAt = staff.UpdatedAt
            };
        }


        public async Task SyncAllRegions()
        {
            var regionIds = Enumerable.Range(1, 10).ToArray();
            var results = new List<MmuhStaffSyncResult>();

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Syncing MmuhStaff region {regionId}...");
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
                    _logger?.LogError(ex, $"Exception while syncing MmuhStaff region {regionId}");
                    results.Add(new MmuhStaffSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new MmuhStaffSyncSummaryDto
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

            _logger?.LogInformation($"MmuhStaff sync completed for all regions. " +
                $"Success: {summary.SuccessfulRegions}/{regionIds.Length}. " +
                $"Total - Staff: {summary.TotalStaffAdded} added, {summary.TotalStaffUpdated} updated.");
        }

        private async Task<MmuhStaffSyncResult> SyncRegionInternal(int regionId)
        {
            var result = new MmuhStaffSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                using var client = new HttpClient();
                var url = $"https://api.emis.am/v1/get_mmuh_staff_by_marz/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var staffArray = JArray.Parse(responseText);

                // Clear staging table for this sync
                await _context.MmuhStaffStaging.ExecuteDeleteAsync();

                var now = DateTime.UtcNow;

                // Process all staff from API into staging
                foreach (var staffToken in staffArray)
                {
                    if (staffToken is not JObject staffObj)
                        continue;

                    var staffIdStr = staffObj["staff_id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(staffIdStr))
                        continue;

                    var instId = staffObj["inst_id"]?.ToString() ?? string.Empty;
                    var instName = staffObj["inst_name"]?.ToString() ?? string.Empty;
                    var firstName = staffObj["first_name"]?.ToString() ?? string.Empty;
                    var lastName = staffObj["last_name"]?.ToString() ?? string.Empty;
                    var fatherName = staffObj["father_name"]?.ToString() ?? string.Empty;
                    var socNumber = staffObj["soc_number"]?.ToString() ?? string.Empty;
                    var sex = staffObj["sex"]?.ToString() ?? string.Empty;
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
                    var groupId = staffObj["group_id"]?.ToString() ?? string.Empty;

                    // Parse date of birth
                    DateOnly dateOfBirth = default;
                    var dobString = staffObj["date_of_birth"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(dobString) && DateOnly.TryParse(dobString, out var parsedDate))
                    {
                        dateOfBirth = parsedDate;
                    }

                    // Serialize groups array to JSON string
                    var groupsJson = string.Empty;
                    var groupsToken = staffObj["groups"];
                    if (groupsToken != null && groupsToken.Type == JTokenType.Array)
                    {
                        groupsJson = groupsToken.ToString(Formatting.None);
                    }

                    // Compute MD5
                    var md5 = SyncHelpers.ComputeMd5(
                        staffIdStr,
                        instId,
                        instName,
                        firstName,
                        lastName,
                        fatherName,
                        dateOfBirth.ToString("yyyy-MM-dd"),
                        socNumber,
                        sex,
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
                        groupId,
                        groupsJson);

                    // Stream directly into staging table
                    _context.MmuhStaffStaging.Add(new MmuhStaffStaging
                    {
                        Id = Guid.NewGuid(),
                        MmuhStaffId = staffIdStr,
                        InstId = instId,
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
                        GroupId = groupId,
                        GroupsJson = groupsJson,
                        MD5 = md5,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                // Save all staging data
                await _context.SaveChangesAsync();

                // Load all staged staff
                var stagingRows = await _context.MmuhStaffStaging.ToListAsync();

                // Get existing staff by MmuhStaffId
                var stagedIds = stagingRows.Select(s => s.MmuhStaffId).Distinct().ToList();
                var existingStaff = await _context.MmuhStaff
                    .Where(s => stagedIds.Contains(s.MmuhStaffId))
                    .ToListAsync();

                var existingDict = existingStaff.ToDictionary(s => s.MmuhStaffId);

                var newStaff = new List<MmuhStaff>();
                var updatedCount = 0;

                // Process each staged staff
                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.MmuhStaffId, out var existing))
                    {
                        // Compare MD5
                        if (!string.Equals(existing.MD5, staging.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            // MD5 changed → update from staging
                            existing.InstId = staging.InstId;
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
                            existing.GroupId = staging.GroupId;
                            existing.GroupsJson = staging.GroupsJson;
                            existing.MD5 = staging.MD5;
                            existing.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                            result.StaffUpdatedList.Add(MapToMmuhStaffDto(existing));
                        }
                    }
                    else
                    {
                        // New staff
                        var newStaffMember = new MmuhStaff
                        {
                            Id = Guid.NewGuid(),
                            MmuhStaffId = staging.MmuhStaffId,
                            InstId = staging.InstId,
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
                            GroupId = staging.GroupId,
                            GroupsJson = staging.GroupsJson,
                            MD5 = staging.MD5,
                            CreatedAt = staging.CreatedAt,
                            UpdatedAt = staging.UpdatedAt
                        };

                        newStaff.Add(newStaffMember);
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] MmuhStaff MD5 compare done. New={newStaff.Count}, Updated={updatedCount}. Saving...");

                // Add new staff
                if (newStaff.Any())
                {
                    _context.MmuhStaff.AddRange(newStaff);
                }

                // Save changes
                if (updatedCount > 0 || newStaff.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Cleanup staging
                await _context.MmuhStaffStaging.ExecuteDeleteAsync();

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
                _logger?.LogError(ex, $"Error syncing MmuhStaff for region {regionId}");
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
                    message = "MmuhStaff sync completed successfully!",
                    regionId = result.RegionId,
                    staffProcessed = result.StaffProcessed,
                    staffAdded = result.StaffAdded,
                    staffUpdated = result.StaffUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "MmuhStaff sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestStaffSync()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MmuhStaffSyncSummaryDto>(
                    "MmuhStaffSyncReports", "mmuh-staff-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MmuhStaff sync file found. Run a sync first." });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MmuhStaff sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestStaffChanges()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MmuhStaffSyncSummaryDto>(
                    "MmuhStaffSyncReports", "mmuh-staff-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MmuhStaff sync file found. Run a sync first." });

                return Ok(new MmuhStaffChangedEntitiesDto { StaffUpdated = summary.AllStaffUpdated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MmuhStaff changes", message = ex.Message });
            }
        }
    }
}

