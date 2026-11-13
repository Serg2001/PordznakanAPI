using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    public class MuhStudentSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int StudentsProcessed { get; set; }
        public int StudentsAdded { get; set; }
        public int StudentsUpdated { get; set; }
        public List<MuhStudentDto> StudentsUpdatedList { get; set; } = new();
    }

    public class MuhStudentChangedEntitiesDto
    {
        public List<MuhStudentDto> StudentsUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MuhStudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISyncReportService _syncReportService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MuhStudentController>? _logger;

        public MuhStudentController(
            AppDbContext context,
            ISyncReportService syncReportService,
            IHttpClientFactory httpClientFactory,
            ILogger<MuhStudentController>? logger = null)
        {
            _context = context;
            _syncReportService = syncReportService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private static MuhStudentDto MapToDto(MuhStudent student)
        {
            return new MuhStudentDto
            {
                Id = student.Id,
                MuhStudentId = student.MuhStudentId,
                InstitutionId = student.InstitutionId,
                InternalSchoolId = student.InternalSchoolId,
                RegionId = student.RegionId,
                InstitutionName = student.InstitutionName,
                GroupId = student.GroupId,
                GroupName = student.GroupName,
                InstitutionEmail = student.InstitutionEmail,
                InstitutionType = student.InstitutionType,
                InstitutionTypeName = student.InstitutionTypeName,
                Marz = student.Marz,
                Region = student.Region,
                Community = student.Community,
                Address = student.Address,
                EduProgramCode = student.EduProgramCode,
                EduProgramTitle = student.EduProgramTitle,
                EduProgramProfCode = student.EduProgramProfCode,
                EduProgramProfTitle = student.EduProgramProfTitle,
                EduYear = student.EduYear,
                Base = student.Base,
                VeGroupTypes = student.VeGroupTypes,
                GroupType = student.GroupType,
                Duration = student.Duration,
                AnnualFee = student.AnnualFee,
                FirstName = student.FirstName,
                LastName = student.LastName,
                FatherName = student.FatherName,
                IdentDocument = student.IdentDocument,
                IdentDocumentNumber = student.IdentDocumentNumber,
                FromCountry = student.FromCountry,
                SocNumber = student.SocNumber,
                DateOfBirth = student.DateOfBirth,
                Sex = student.Sex,
                FreeEdu = student.FreeEdu,
                HasBook = student.HasBook,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };
        }

        [NonAction]
        public async Task SyncAllRegions()
        {
            // Regions plus the manually maintained (regionId, subId) links shared with MuhInstitutionController.
            var targets = Enumerable.Range(1, 11)
                .Select(r => (RegionId: r, SubId: (int?)null))
                .Concat(MuhInstitutionController.ExtraGeographyLinks.Select(l => (l.RegionId, SubId: (int?)l.SubId)))
                .Append((RegionId: 0, SubId: (int?)null)) // colleges/getAllData with no region: everything
                .ToArray();
            var results = new List<MuhStudentSyncResult>();

            foreach (var (regionId, subId) in targets)
            {
                var label = subId.HasValue ? $"{regionId}/{subId}" : regionId.ToString();
                try
                {
                    _logger?.LogInformation($"Syncing MuhStudent region {label}...");
                    var result = await SyncRegionInternal(regionId, subId);
                    results.Add(result);

                    if (result.Success)
                    {
                        _logger?.LogInformation($"Region {label} synced successfully. " +
                            $"Students: {result.StudentsAdded} added, {result.StudentsUpdated} updated.");
                    }
                    else
                    {
                        _logger?.LogError($"Region {label} sync failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while syncing MuhStudent region {label}");
                    results.Add(new MuhStudentSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new MuhStudentSyncSummaryDto
            {
                SyncCompletedAt = DateTime.UtcNow,
                TotalRegionsProcessed = targets.Length,
                SuccessfulRegions = results.Count(r => r.Success),
                FailedRegions = results.Count(r => !r.Success),
                TotalStudentsAdded = results.Sum(r => r.StudentsAdded),
                TotalStudentsUpdated = results.Sum(r => r.StudentsUpdated)
            };

            foreach (var result in results.Where(r => r.Success))
            {
                summary.AllStudentsUpdated.AddRange(result.StudentsUpdatedList);
            }

            _logger?.LogInformation($"MuhStudent sync completed for all regions. " +
                $"Success: {summary.SuccessfulRegions}/{targets.Length}. " +
                $"Total - Students: {summary.TotalStudentsAdded} added, {summary.TotalStudentsUpdated} updated.");
        }

        /// <summary>
        /// Walks the getAllData response and flattens it into one row per student:
        /// root dictionary keyed by institution_id → "group" dictionary keyed by group id →
        /// "students" dictionary keyed by group id → array of student objects.
        /// Keys are used when the inner id fields are missing, and empty levels
        /// (which the API may send as [] instead of {}) are skipped. A student that shows
        /// up more than once is kept once (first occurrence).
        /// </summary>
        private static List<MuhStudentStaging> ParseStudents(string json, int regionId, DateTime now)
        {
            var rows = new List<MuhStudentStaging>();
            var seen = new HashSet<int>();

            if (JToken.Parse(json) is not JObject institutions)
                return rows;

            foreach (var institutionProp in institutions.Properties())
            {
                if (institutionProp.Value is not JObject institution)
                    continue;

                if (!int.TryParse(institution["institution_id"]?.ToString(), out var institutionId) &&
                    !int.TryParse(institutionProp.Name, out institutionId))
                    continue;

                var institutionName = institution["institution_name"]?.ToString() ?? string.Empty;
                var institutionEmail = institution["institution_email"]?.ToString() ?? string.Empty;
                int.TryParse(institution["institution_type"]?.ToString(), out var institutionType);
                var institutionTypeName = institution["institution_type_name"]?.ToString() ?? string.Empty;
                var marz = institution["marz"]?.ToString() ?? string.Empty;
                var region = institution["region"]?.ToString() ?? string.Empty;
                var community = institution["community"]?.ToString() ?? string.Empty;
                var address = institution["address"]?.ToString() ?? string.Empty;

                if (institution["group"] is not JObject groups)
                    continue;

                foreach (var groupProp in groups.Properties())
                {
                    if (groupProp.Value is not JObject group)
                        continue;

                    if (!int.TryParse(group["id"]?.ToString(), out var groupId) &&
                        !int.TryParse(groupProp.Name, out groupId))
                        continue;

                    var groupName = group["name"]?.ToString() ?? string.Empty;
                    var eduProgramCode = group["edu_program_code"]?.ToString() ?? string.Empty;
                    var eduProgramTitle = group["edu_program_title"]?.ToString() ?? string.Empty;
                    var eduProgramProfCode = group["edu_program_prof_code"]?.ToString() ?? string.Empty;
                    var eduProgramProfTitle = group["edu_program_prof_title"]?.ToString() ?? string.Empty;
                    var eduYear = group["edu_year"]?.ToString() ?? string.Empty;
                    int.TryParse(group["base"]?.ToString(), out var baseValue);
                    int.TryParse(group["ve_group_types"]?.ToString(), out var veGroupTypes);
                    var groupType = group["group_type"]?.ToString() ?? string.Empty;
                    int.TryParse(group["duration"]?.ToString(), out var duration);
                    decimal.TryParse(group["annual_fee"]?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var annualFee);

                    foreach (var studentObj in EnumerateStudentObjects(group["students"]))
                    {
                        if (!int.TryParse(studentObj["id"]?.ToString(), out var studentId))
                            continue;

                        if (!seen.Add(studentId))
                            continue;

                        var firstName = studentObj["first_name"]?.ToString() ?? string.Empty;
                        var lastName = studentObj["last_name"]?.ToString() ?? string.Empty;
                        var fatherName = studentObj["father_name"]?.ToString() ?? string.Empty;
                        var identDocument = studentObj["ident_document"]?.ToString() ?? string.Empty;
                        var identDocumentNumber = studentObj["ident_document_number"]?.ToString() ?? string.Empty;
                        var fromCountry = studentObj["from_country"]?.ToString() ?? string.Empty;
                        var socNumber = studentObj["soc_number"]?.ToString() ?? string.Empty;
                        var sex = studentObj["sex"]?.ToString() ?? string.Empty;

                        DateOnly dateOfBirth = default;
                        var dobString = studentObj["date_of_birth"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(dobString) && DateOnly.TryParse(dobString, out var parsedDate))
                            dateOfBirth = parsedDate;

                        // free_edu comes as 0/1 (or "0"/"1"/true/false)
                        var freeEduText = studentObj["free_edu"]?.ToString();
                        var freeEdu = freeEduText == "1" || string.Equals(freeEduText, "true", StringComparison.OrdinalIgnoreCase);

                        var md5 = SyncHelpers.ComputeMd5(
                            studentId.ToString(),
                            institutionId.ToString(),
                            institutionName,
                            groupId.ToString(),
                            groupName,
                            firstName,
                            lastName,
                            fatherName,
                            identDocument,
                            identDocumentNumber,
                            fromCountry,
                            socNumber,
                            dateOfBirth.ToString("yyyy-MM-dd"),
                            sex,
                            freeEdu ? "1" : "0",
                            institutionEmail,
                            institutionType.ToString(),
                            institutionTypeName,
                            marz,
                            region,
                            community,
                            address,
                            eduProgramCode,
                            eduProgramTitle,
                            eduProgramProfCode,
                            eduProgramProfTitle,
                            eduYear,
                            baseValue.ToString(),
                            veGroupTypes.ToString(),
                            groupType,
                            duration.ToString(),
                            annualFee.ToString(System.Globalization.CultureInfo.InvariantCulture));

                        rows.Add(new MuhStudentStaging
                        {
                            Id = Guid.NewGuid(),
                            MuhStudentId = studentId,
                            InstitutionId = institutionId,
                            InternalSchoolId = null,
                            RegionId = regionId,
                            InstitutionName = institutionName,
                            GroupId = groupId,
                            GroupName = groupName,
                            InstitutionEmail = institutionEmail,
                            InstitutionType = institutionType,
                            InstitutionTypeName = institutionTypeName,
                            Marz = marz,
                            Region = region,
                            Community = community,
                            Address = address,
                            EduProgramCode = eduProgramCode,
                            EduProgramTitle = eduProgramTitle,
                            EduProgramProfCode = eduProgramProfCode,
                            EduProgramProfTitle = eduProgramProfTitle,
                            EduYear = eduYear,
                            Base = baseValue,
                            VeGroupTypes = veGroupTypes,
                            GroupType = groupType,
                            Duration = duration,
                            AnnualFee = annualFee,
                            FirstName = firstName,
                            LastName = lastName,
                            FatherName = fatherName,
                            IdentDocument = identDocument,
                            IdentDocumentNumber = identDocumentNumber,
                            FromCountry = fromCountry,
                            SocNumber = socNumber,
                            DateOfBirth = dateOfBirth,
                            Sex = sex,
                            FreeEdu = freeEdu,
                            MD5 = md5,
                            CreatedAt = now,
                            UpdatedAt = now
                        });
                    }
                }
            }

            return rows;
        }

        /// <summary>
        /// "students" is an object keyed by group id whose values are arrays of students
        /// ("1166": [ {...}, {...} ]); a plain array of students is accepted too.
        /// </summary>
        private static IEnumerable<JObject> EnumerateStudentObjects(JToken? students)
        {
            switch (students)
            {
                case JObject byGroup:
                    foreach (var prop in byGroup.Properties())
                    {
                        if (prop.Value is JArray arr)
                        {
                            foreach (var item in arr.OfType<JObject>())
                                yield return item;
                        }
                        else if (prop.Value is JObject single)
                        {
                            yield return single;
                        }
                    }
                    break;
                case JArray array:
                    foreach (var item in array.OfType<JObject>())
                        yield return item;
                    break;
            }
        }

        private async Task<MuhStudentSyncResult> SyncRegionInternal(int regionId, int? subId = null)
        {
            var result = new MuhStudentSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                // Shared EMIS client: base URL, X-Api-Token, retries and long timeout come from Program.cs.
                var client = _httpClientFactory.CreateClient(EmisClients.GetAllData);
                // regionId 0 = the plain colleges/getAllData call (no region in the URL)
                var path = regionId == 0
                    ? "colleges/getAllData"
                    : subId.HasValue
                        ? $"colleges/getAllData/{regionId}/{subId}"
                        : $"colleges/getAllData/{regionId}";
                var responseText = await client.GetStringAsync(path);

                var now = DateTime.UtcNow;
                var parsed = ParseStudents(responseText, regionId, now);

                // Clear staging table for this sync
                await _context.MuhStudentsStaging.ExecuteDeleteAsync();

                _context.MuhStudentsStaging.AddRange(parsed);

                // Save all staging data
                await _context.SaveChangesAsync();

                // Load all staged students
                var stagingRows = await _context.MuhStudentsStaging.ToListAsync();

                // Build InstitutionId → MuhInstitution.Id lookup for InternalSchoolId resolution
                var institutionIds = stagingRows.Select(s => s.InstitutionId).Distinct().ToList();
                var schoolLookup = await _context.MuhInstitutions
                    .Where(i => institutionIds.Contains(i.InstitutionId))
                    .ToDictionaryAsync(i => i.InstitutionId, i => i.Id);

                // Get existing students by MuhStudentId
                var stagedIds = stagingRows.Select(s => s.MuhStudentId).Distinct().ToList();
                var existingStudents = await _context.MuhStudents
                    .Where(s => stagedIds.Contains(s.MuhStudentId))
                    .ToListAsync();

                var existingDict = existingStudents.ToDictionary(s => s.MuhStudentId);

                var newStudents = new List<MuhStudent>();
                var updatedCount = 0;

                // Process each staged student
                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.MuhStudentId, out var existing))
                    {
                        // Compare MD5
                        if (!string.Equals(existing.MD5, staging.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            // MD5 changed → update from staging (HasBook is never touched by the sync)
                            existing.InstitutionId = staging.InstitutionId;
                            existing.InternalSchoolId = schoolLookup.TryGetValue(staging.InstitutionId, out var updatedSid)
                                ? updatedSid
                                : existing.InternalSchoolId;
                            // The no-region call can't tell the region, so keep the one already known.
                            if (staging.RegionId != 0)
                                existing.RegionId = staging.RegionId;
                            existing.InstitutionName = staging.InstitutionName;
                            existing.GroupId = staging.GroupId;
                            existing.GroupName = staging.GroupName;
                            existing.InstitutionEmail = staging.InstitutionEmail;
                            existing.InstitutionType = staging.InstitutionType;
                            existing.InstitutionTypeName = staging.InstitutionTypeName;
                            existing.Marz = staging.Marz;
                            existing.Region = staging.Region;
                            existing.Community = staging.Community;
                            existing.Address = staging.Address;
                            existing.EduProgramCode = staging.EduProgramCode;
                            existing.EduProgramTitle = staging.EduProgramTitle;
                            existing.EduProgramProfCode = staging.EduProgramProfCode;
                            existing.EduProgramProfTitle = staging.EduProgramProfTitle;
                            existing.EduYear = staging.EduYear;
                            existing.Base = staging.Base;
                            existing.VeGroupTypes = staging.VeGroupTypes;
                            existing.GroupType = staging.GroupType;
                            existing.Duration = staging.Duration;
                            existing.AnnualFee = staging.AnnualFee;
                            existing.FirstName = staging.FirstName;
                            existing.LastName = staging.LastName;
                            existing.FatherName = staging.FatherName;
                            existing.IdentDocument = staging.IdentDocument;
                            existing.IdentDocumentNumber = staging.IdentDocumentNumber;
                            existing.FromCountry = staging.FromCountry;
                            existing.SocNumber = staging.SocNumber;
                            existing.DateOfBirth = staging.DateOfBirth;
                            existing.Sex = staging.Sex;
                            existing.FreeEdu = staging.FreeEdu;
                            existing.MD5 = staging.MD5;
                            existing.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                            result.StudentsUpdatedList.Add(MapToDto(existing));
                        }
                    }
                    else
                    {
                        // New student
                        newStudents.Add(new MuhStudent
                        {
                            Id = Guid.NewGuid(),
                            MuhStudentId = staging.MuhStudentId,
                            InstitutionId = staging.InstitutionId,
                            InternalSchoolId = schoolLookup.TryGetValue(staging.InstitutionId, out var newSid)
                                ? newSid
                                : null,
                            RegionId = staging.RegionId,
                            InstitutionName = staging.InstitutionName,
                            GroupId = staging.GroupId,
                            GroupName = staging.GroupName,
                            InstitutionEmail = staging.InstitutionEmail,
                            InstitutionType = staging.InstitutionType,
                            InstitutionTypeName = staging.InstitutionTypeName,
                            Marz = staging.Marz,
                            Region = staging.Region,
                            Community = staging.Community,
                            Address = staging.Address,
                            EduProgramCode = staging.EduProgramCode,
                            EduProgramTitle = staging.EduProgramTitle,
                            EduProgramProfCode = staging.EduProgramProfCode,
                            EduProgramProfTitle = staging.EduProgramProfTitle,
                            EduYear = staging.EduYear,
                            Base = staging.Base,
                            VeGroupTypes = staging.VeGroupTypes,
                            GroupType = staging.GroupType,
                            Duration = staging.Duration,
                            AnnualFee = staging.AnnualFee,
                            FirstName = staging.FirstName,
                            LastName = staging.LastName,
                            FatherName = staging.FatherName,
                            IdentDocument = staging.IdentDocument,
                            IdentDocumentNumber = staging.IdentDocumentNumber,
                            FromCountry = staging.FromCountry,
                            SocNumber = staging.SocNumber,
                            DateOfBirth = staging.DateOfBirth,
                            Sex = staging.Sex,
                            FreeEdu = staging.FreeEdu,
                            MD5 = staging.MD5,
                            CreatedAt = staging.CreatedAt,
                            UpdatedAt = staging.UpdatedAt
                        });
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] MuhStudent MD5 compare done. New={newStudents.Count}, Updated={updatedCount}. Saving...");

                // Add new students
                if (newStudents.Any())
                {
                    _context.MuhStudents.AddRange(newStudents);
                }

                // Save changes
                if (updatedCount > 0 || newStudents.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Cleanup staging
                await _context.MuhStudentsStaging.ExecuteDeleteAsync();

                result.Success = true;
                result.StudentsProcessed = stagingRows.Count;
                result.StudentsAdded = newStudents.Count;
                result.StudentsUpdated = updatedCount;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger?.LogError(ex, $"Error syncing MuhStudent for region {regionId}");
                return result;
            }
        }

        [HttpPost("sync/{regionId?}")]
        public async Task<IActionResult> SyncStudents([FromRoute] int regionId = 1)
        {
            var result = await SyncRegionInternal(regionId);

            if (result.Success)
            {
                return Ok(new
                {
                    message = "MuhStudent sync completed successfully!",
                    regionId = result.RegionId,
                    studentsProcessed = result.StudentsProcessed,
                    studentsAdded = result.StudentsAdded,
                    studentsUpdated = result.StudentsUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "MuhStudent sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestStudentSync()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MuhStudentSyncSummaryDto>(
                    "MuhStudentSyncReports", "muh-student-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MuhStudent sync file found. Run a sync first." });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MuhStudent sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestStudentChanges()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MuhStudentSyncSummaryDto>(
                    "MuhStudentSyncReports", "muh-student-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MuhStudent sync file found. Run a sync first." });

                return Ok(new MuhStudentChangedEntitiesDto { StudentsUpdated = summary.AllStudentsUpdated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MuhStudent changes", message = ex.Message });
            }
        }
    }
}
