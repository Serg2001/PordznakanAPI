using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using PordznakanAPI.Enums;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    public class NmuhStudentSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int StudentsProcessed { get; set; }
        public int StudentsAdded { get; set; }
        public int StudentsUpdated { get; set; }
        public List<NmuhStudentDto> StudentsUpdatedList { get; set; } = new();
    }

    public class NmuhStudentChangedEntitiesDto
    {
        public List<NmuhStudentDto> StudentsUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class NmuhStudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISyncReportService _syncReportService;
        private readonly ILogger<NmuhStudentController>? _logger;

        public NmuhStudentController(AppDbContext context, ISyncReportService syncReportService, ILogger<NmuhStudentController>? logger = null)
        {
            _context = context;
            _syncReportService = syncReportService;
            _logger = logger;
        }

        private NmuhStudentDto MapToNmuhStudentDto(NmuhStudent student)
        {
            return new NmuhStudentDto
            {
                Id = student.Id,
                NmuhStudentId = student.NmuhStudentId,
                NmuhSchoolId = student.NmuhSchoolId,
                InternalSchoolId = student.InternalSchoolId,
                RegionId = student.RegionId,
                SchoolName = student.SchoolName,
                Marz = student.Marz,
                FirstName = student.FirstName,
                LastName = student.LastName,
                FatherName = student.FatherName,
                DateOfBirth = student.DateOfBirth,
                SocNumber = student.SocNumber,
                Sex = student.Sex,
                Graduated = student.Graduated,
                EduYear = student.EduYear,
                GroupId = student.GroupId,
                ClassroomGrade = student.ClassroomGrade,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };
        }


        [NonAction]
        public async Task SyncAllRegions()
        {
            var regionIds = Enumerable.Range(1, 11).ToArray();
            var results = new List<NmuhStudentSyncResult>();

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Syncing NmuhStudent region {regionId}...");
                    var result = await SyncRegionInternal(regionId);
                    results.Add(result);
                    
                    if (result.Success)
                    {
                        _logger?.LogInformation($"Region {regionId} synced successfully. " +
                            $"Students: {result.StudentsAdded} added, {result.StudentsUpdated} updated.");
                    }
                    else
                    {
                        _logger?.LogError($"Region {regionId} sync failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while syncing NmuhStudent region {regionId}");
                    results.Add(new NmuhStudentSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new NmuhStudentSyncSummaryDto
            {
                SyncCompletedAt = DateTime.UtcNow,
                TotalRegionsProcessed = regionIds.Length,
                SuccessfulRegions = results.Count(r => r.Success),
                FailedRegions = results.Count(r => !r.Success),
                TotalStudentsAdded = results.Sum(r => r.StudentsAdded),
                TotalStudentsUpdated = results.Sum(r => r.StudentsUpdated)
            };

            foreach (var result in results.Where(r => r.Success))
            {
                summary.AllStudentsUpdated.AddRange(result.StudentsUpdatedList);
            }

            _logger?.LogInformation($"NmuhStudent sync completed for all regions. " +
                $"Success: {summary.SuccessfulRegions}/{regionIds.Length}. " +
                $"Total - Students: {summary.TotalStudentsAdded} added, {summary.TotalStudentsUpdated} updated.");
        }

        private async Task<NmuhStudentSyncResult> SyncRegionInternal(int regionId)
        {
            var result = new NmuhStudentSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                using var client = new HttpClient();
                var url = $"https://api.emis.am/v1/get_nmuh_student/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var studentsArray = JArray.Parse(responseText);

                // Clear staging table for this sync
                await _context.NmuhStudentsStaging.ExecuteDeleteAsync();

                var now = DateTime.UtcNow;

                // Process all students from API into staging
                foreach (var studentToken in studentsArray)
                {
                    if (studentToken is not JObject studentObj)
                        continue;

                    if (!int.TryParse(studentObj["student_id"]?.ToString(), out var studentIdStr))
                        continue;

                    if (!int.TryParse(studentObj["school_id"]?.ToString(), out var schoolIdStr))
                        schoolIdStr = 0;
                    var schoolName = studentObj["school_name"]?.ToString() ?? string.Empty;
                    var marz = studentObj["marz"]?.ToString() ?? string.Empty;
                    var firstName = studentObj["first_name"]?.ToString() ?? string.Empty;
                    var lastName = studentObj["last_name"]?.ToString() ?? string.Empty;
                    var fatherName = studentObj["father_name"]?.ToString() ?? string.Empty;
                    var socNumber = studentObj["soc_number"]?.ToString() ?? string.Empty;
                    var sex = studentObj["sex"]?.ToString() ?? string.Empty;
                    var groupId = studentObj["group_id"]?.ToString() ?? string.Empty;
                    var eduYear = studentObj["edu_year"]?.ToString() ?? string.Empty;

                    // Parse date of birth
                    DateOnly dateOfBirth = default;
                    var dobString = studentObj["date_of_birth"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(dobString) && DateOnly.TryParse(dobString, out var parsedDate))
                    {
                        dateOfBirth = parsedDate;
                    }

                    // Parse graduated
                    var graduated = SyncHelpers.MapGraduated(studentObj["graduated"]?.ToString());

                    // Parse classroom_grade
                    var classroomGrade = SyncHelpers.MapGrade(studentObj["classroom_grade"]?.ToString());

                    // Compute MD5
                    var md5 = SyncHelpers.ComputeMd5(
                        studentIdStr.ToString(),
                        schoolIdStr.ToString(),
                        schoolName,
                        marz,
                        firstName,
                        lastName,
                        fatherName,
                        dateOfBirth.ToString("yyyy-MM-dd"),
                        socNumber,
                        sex,
                        graduated ? "1" : "0",
                        eduYear,
                        groupId,
                        classroomGrade.ToString());

                    // Stream directly into staging table
                    _context.NmuhStudentsStaging.Add(new NmuhStudentStaging
                    {
                        Id = Guid.NewGuid(),
                        NmuhStudentId = studentIdStr,
                        NmuhSchoolId = schoolIdStr,
                        InternalSchoolId = null,
                        RegionId = regionId,
                        SchoolName = schoolName,
                        Marz = marz,
                        FirstName = firstName,
                        LastName = lastName,
                        FatherName = fatherName,
                        DateOfBirth = dateOfBirth,
                        SocNumber = socNumber,
                        Sex = sex,
                        Graduated = graduated,
                        EduYear = eduYear,
                        GroupId = groupId,
                        ClassroomGrade = classroomGrade,
                        MD5 = md5,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                // Save all staging data
                await _context.SaveChangesAsync();

                // Load all staged students
                var stagingRows = await _context.NmuhStudentsStaging.ToListAsync();

                // Build NmuhSchoolId → NmuhInstitution.Id lookup for InternalSchoolId resolution
                var schoolIds = stagingRows.Select(s => s.NmuhSchoolId).Distinct().ToList();
                var schoolLookup = await _context.NmuhInstitutions
                    .Where(i => schoolIds.Contains(i.InstId))
                    .ToDictionaryAsync(i => i.InstId, i => i.Id);

                // Get existing students by NmuhStudentId
                var stagedIds = stagingRows.Select(s => s.NmuhStudentId).Distinct().ToList();
                var existingStudents = await _context.NmuhStudents
                    .Where(s => stagedIds.Contains(s.NmuhStudentId))
                    .ToListAsync();

                var existingDict = existingStudents.ToDictionary(s => s.NmuhStudentId);

                var newStudents = new List<NmuhStudent>();
                var updatedCount = 0;

                // Process each staged student
                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.NmuhStudentId, out var existing))
                    {
                        // Compare MD5
                        if (!string.Equals(existing.MD5, staging.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            // MD5 changed → update from staging
                            existing.NmuhSchoolId = staging.NmuhSchoolId;
                            existing.InternalSchoolId = schoolLookup.TryGetValue(staging.NmuhSchoolId, out var updatedSid)
                                ? updatedSid
                                : existing.InternalSchoolId;
                            existing.RegionId = staging.RegionId;
                            existing.SchoolName = staging.SchoolName;
                            existing.Marz = staging.Marz;
                            existing.FirstName = staging.FirstName;
                            existing.LastName = staging.LastName;
                            existing.FatherName = staging.FatherName;
                            existing.DateOfBirth = staging.DateOfBirth;
                            existing.SocNumber = staging.SocNumber;
                            existing.Sex = staging.Sex;
                            existing.Graduated = staging.Graduated;
                            existing.EduYear = staging.EduYear;
                            existing.GroupId = staging.GroupId;
                            existing.ClassroomGrade = staging.ClassroomGrade;
                            existing.MD5 = staging.MD5;
                            existing.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                            result.StudentsUpdatedList.Add(MapToNmuhStudentDto(existing));
                        }
                    }
                    else
                    {
                        // New student
                        var newStudent = new NmuhStudent
                        {
                            Id = Guid.NewGuid(),
                            NmuhStudentId = staging.NmuhStudentId,
                            NmuhSchoolId = staging.NmuhSchoolId,
                            InternalSchoolId = schoolLookup.TryGetValue(staging.NmuhSchoolId, out var newSid)
                                ? newSid
                                : null,
                            RegionId = staging.RegionId,
                            SchoolName = staging.SchoolName,
                            Marz = staging.Marz,
                            FirstName = staging.FirstName,
                            LastName = staging.LastName,
                            FatherName = staging.FatherName,
                            DateOfBirth = staging.DateOfBirth,
                            SocNumber = staging.SocNumber,
                            Sex = staging.Sex,
                            Graduated = staging.Graduated,
                            EduYear = staging.EduYear,
                            GroupId = staging.GroupId,
                            ClassroomGrade = staging.ClassroomGrade,
                            MD5 = staging.MD5,
                            CreatedAt = staging.CreatedAt,
                            UpdatedAt = staging.UpdatedAt
                        };

                        newStudents.Add(newStudent);
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] NmuhStudent MD5 compare done. New={newStudents.Count}, Updated={updatedCount}. Saving...");

                // Add new students
                if (newStudents.Any())
                {
                    _context.NmuhStudents.AddRange(newStudents);
                }

                // Save changes
                if (updatedCount > 0 || newStudents.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Cleanup staging
                await _context.NmuhStudentsStaging.ExecuteDeleteAsync();

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
                _logger?.LogError(ex, $"Error syncing NmuhStudent for region {regionId}");
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
                    message = "NmuhStudent sync completed successfully!",
                    regionId = result.RegionId,
                    studentsProcessed = result.StudentsProcessed,
                    studentsAdded = result.StudentsAdded,
                    studentsUpdated = result.StudentsUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "NmuhStudent sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestStudentSync()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<NmuhStudentSyncSummaryDto>(
                    "NmuhStudentSyncReports", "nmuh-student-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No NmuhStudent sync file found. Run a sync first." });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load NmuhStudent sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestStudentChanges()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<NmuhStudentSyncSummaryDto>(
                    "NmuhStudentSyncReports", "nmuh-student-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No NmuhStudent sync file found. Run a sync first." });

                return Ok(new NmuhStudentChangedEntitiesDto { StudentsUpdated = summary.AllStudentsUpdated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load NmuhStudent changes", message = ex.Message });
            }
        }
    }
}

