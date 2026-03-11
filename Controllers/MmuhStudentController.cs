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
    public class MmuhStudentSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int StudentsProcessed { get; set; }
        public int StudentsAdded { get; set; }
        public int StudentsUpdated { get; set; }
        public List<MmuhStudentDto> StudentsUpdatedList { get; set; } = new();
    }

    public class MmuhStudentChangedEntitiesDto
    {
        public List<MmuhStudentDto> StudentsUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MmuhStudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISyncReportService _syncReportService;
        private readonly ILogger<MmuhStudentController>? _logger;

        public MmuhStudentController(AppDbContext context, ISyncReportService syncReportService, ILogger<MmuhStudentController>? logger = null)
        {
            _context = context;
            _syncReportService = syncReportService;
            _logger = logger;
        }

        private MmuhStudentDto MapToMmuhStudentDto(MmuhStudent student)
        {
            return new MmuhStudentDto
            {
                Id = student.Id,
                MmuhStudentId = student.MmuhStudentId,
                MmuhSchoolId = student.MmuhSchoolId,
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
                GroupId = student.GroupId,
                ClassroomGrade = student.ClassroomGrade,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };
        }


        [NonAction]
        public async Task SyncAllRegions()
        {
            var regionIds = Enumerable.Range(1, 10).ToArray();
            var results = new List<MmuhStudentSyncResult>();

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Syncing MmuhStudent region {regionId}...");
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
                    _logger?.LogError(ex, $"Exception while syncing MmuhStudent region {regionId}");
                    results.Add(new MmuhStudentSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new MmuhStudentSyncSummaryDto
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

            _logger?.LogInformation($"MmuhStudent sync completed for all regions. " +
                $"Success: {summary.SuccessfulRegions}/{regionIds.Length}. " +
                $"Total - Students: {summary.TotalStudentsAdded} added, {summary.TotalStudentsUpdated} updated.");
        }

        private async Task<MmuhStudentSyncResult> SyncRegionInternal(int regionId)
        {
            var result = new MmuhStudentSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                using var client = new HttpClient();
                var url = $"https://api.emis.am/v1/get_mmuh_student/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var studentsArray = JArray.Parse(responseText);

                // Clear staging table for this sync
                await _context.MmuhStudentsStaging.ExecuteDeleteAsync();

                var now = DateTime.UtcNow;

                // Process all students from API into staging
                foreach (var studentToken in studentsArray)
                {
                    if (studentToken is not JObject studentObj)
                        continue;

                    var studentIdStr = studentObj["student_id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(studentIdStr))
                        continue;

                    var schoolIdStr = studentObj["school_id"]?.ToString() ?? string.Empty;
                    var schoolName = studentObj["school_name"]?.ToString() ?? string.Empty;
                    var marz = studentObj["marz"]?.ToString() ?? string.Empty;
                    var firstName = studentObj["first_name"]?.ToString() ?? string.Empty;
                    var lastName = studentObj["last_name"]?.ToString() ?? string.Empty;
                    var fatherName = studentObj["father_name"]?.ToString() ?? string.Empty;
                    var socNumber = studentObj["soc_number"]?.ToString() ?? string.Empty;
                    var sex = studentObj["sex"]?.ToString() ?? string.Empty;
                    var groupId = studentObj["group_id"]?.ToString() ?? string.Empty;

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
                        studentIdStr,
                        schoolIdStr,
                        schoolName,
                        marz,
                        firstName,
                        lastName,
                        fatherName,
                        dateOfBirth.ToString("yyyy-MM-dd"),
                        socNumber,
                        sex,
                        graduated ? "1" : "0",
                        groupId,
                        classroomGrade.ToString());

                    // Stream directly into staging table
                    _context.MmuhStudentsStaging.Add(new MmuhStudentStaging
                    {
                        Id = Guid.NewGuid(),
                        MmuhStudentId = studentIdStr,
                        MmuhSchoolId = schoolIdStr,
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
                var stagingRows = await _context.MmuhStudentsStaging.ToListAsync();

                // Get existing students by MmuhStudentId
                var stagedIds = stagingRows.Select(s => s.MmuhStudentId).Distinct().ToList();
                var existingStudents = await _context.MmuhStudents
                    .Where(s => stagedIds.Contains(s.MmuhStudentId))
                    .ToListAsync();

                var existingDict = existingStudents.ToDictionary(s => s.MmuhStudentId);

                var newStudents = new List<MmuhStudent>();
                var updatedCount = 0;

                // Process each staged student
                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.MmuhStudentId, out var existing))
                    {
                        // Compare MD5
                        if (!string.Equals(existing.MD5, staging.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            // MD5 changed → update from staging
                            existing.MmuhSchoolId = staging.MmuhSchoolId;
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
                            existing.GroupId = staging.GroupId;
                            existing.ClassroomGrade = staging.ClassroomGrade;
                            existing.MD5 = staging.MD5;
                            existing.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                            result.StudentsUpdatedList.Add(MapToMmuhStudentDto(existing));
                        }
                    }
                    else
                    {
                        // New student
                        var newStudent = new MmuhStudent
                        {
                            Id = Guid.NewGuid(),
                            MmuhStudentId = staging.MmuhStudentId,
                            MmuhSchoolId = staging.MmuhSchoolId,
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
                            GroupId = staging.GroupId,
                            ClassroomGrade = staging.ClassroomGrade,
                            MD5 = staging.MD5,
                            CreatedAt = staging.CreatedAt,
                            UpdatedAt = staging.UpdatedAt
                        };

                        newStudents.Add(newStudent);
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] MmuhStudent MD5 compare done. New={newStudents.Count}, Updated={updatedCount}. Saving...");

                // Add new students
                if (newStudents.Any())
                {
                    _context.MmuhStudents.AddRange(newStudents);
                }

                // Save changes
                if (updatedCount > 0 || newStudents.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Cleanup staging
                await _context.MmuhStudentsStaging.ExecuteDeleteAsync();

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
                _logger?.LogError(ex, $"Error syncing MmuhStudent for region {regionId}");
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
                    message = "MmuhStudent sync completed successfully!",
                    regionId = result.RegionId,
                    studentsProcessed = result.StudentsProcessed,
                    studentsAdded = result.StudentsAdded,
                    studentsUpdated = result.StudentsUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "MmuhStudent sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestStudentSync()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MmuhStudentSyncSummaryDto>(
                    "MmuhStudentSyncReports", "mmuh-student-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MmuhStudent sync file found. Run a sync first." });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MmuhStudent sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestStudentChanges()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MmuhStudentSyncSummaryDto>(
                    "MmuhStudentSyncReports", "mmuh-student-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MmuhStudent sync file found. Run a sync first." });

                return Ok(new MmuhStudentChangedEntitiesDto { StudentsUpdated = summary.AllStudentsUpdated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MmuhStudent changes", message = ex.Message });
            }
        }
    }
}

