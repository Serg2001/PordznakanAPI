using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using System.IO;

namespace PordznakanAPI.Controllers
{
    public class TeacherSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int TeachersProcessed { get; set; }
        public int TeachersAdded { get; set; }
        public int TeachersUpdated { get; set; }
        public List<TeacherDto> TeachersUpdatedList { get; set; } = new();
    }

    public class TeacherChangedEntitiesDto
    {
        public List<TeacherDto> TeachersUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TeacherController>? _logger;

        public TeacherController(AppDbContext context, ILogger<TeacherController>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        private TeacherDto MapToTeacherDto(Teacher teacher)
        {
            return new TeacherDto
            {
                Id = teacher.Id,
                PersonId = teacher.PersonId,
                SchoolId = teacher.SchoolId,
                SchoolName = teacher.SchoolName,
                Email = teacher.Email,
                Activated = teacher.Activated,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                FatherName = teacher.FatherName,
                Sex = teacher.Sex,
                WorkType = teacher.WorkType,
                SocNumber = teacher.SocNumber,
                DateOfBirth = teacher.DateOfBirth,
                Address = teacher.Address,
                Phone = teacher.Phone,
                Education = teacher.Education,
                CommandDate = teacher.CommandDate,
                SubjectId = teacher.SubjectId,
                MainSubject = teacher.MainSubject,
                PersonPositions = teacher.PersonPositions,
                SubjectsJson = teacher.SubjectsJson,
                DigitLevel = teacher.DigitLevel,
                Experience = teacher.Experience,
                AcademicRank = teacher.AcademicRank,
                AcademicRankId = teacher.AcademicRankId,
                CreatedAt = teacher.CreatedAt,
                UpdatedAt = teacher.UpdatedAt
            };
        }

        public async Task SyncAllRegions()
        {
            var regionIds = Enumerable.Range(1, 10).ToArray();
            var results = new List<TeacherSyncResult>();

            foreach (var regionId in regionIds)
            {
                try
                {
                    var result = await SyncRegionInternal(regionId);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Teacher sync failed for region {regionId}");
                    results.Add(new TeacherSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new TeacherSyncSummaryDto
            {
                SyncCompletedAt = DateTime.UtcNow,
                TotalRegionsProcessed = regionIds.Length,
                SuccessfulRegions = results.Count(r => r.Success),
                FailedRegions = results.Count(r => !r.Success),
                TotalTeachersAdded = results.Sum(r => r.TeachersAdded),
                TotalTeachersUpdated = results.Sum(r => r.TeachersUpdated)
            };

            foreach (var result in results.Where(r => r.Success))
            {
                summary.AllTeachersUpdated.AddRange(result.TeachersUpdatedList);
            }

            await SaveChangesToJsonFile(summary);
        }

        public TeacherChangedEntitiesDto GetChangedTeachers(List<TeacherSyncResult> results)
        {
            var dto = new TeacherChangedEntitiesDto();

            foreach (var result in results.Where(r => r.Success))
            {
                dto.TeachersUpdated.AddRange(result.TeachersUpdatedList);
            }

            return dto;
        }

        private async Task SaveChangesToJsonFile(TeacherSyncSummaryDto summary)
        {
            try
            {
                var json = JsonConvert.SerializeObject(summary, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
                });

                var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TeacherSyncReports");
                if (!Directory.Exists(reportsDirectory))
                {
                    Directory.CreateDirectory(reportsDirectory);
                }

                var fileName = $"teacher-sync-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
                var filePath = Path.Combine(reportsDirectory, fileName);
                await System.IO.File.WriteAllTextAsync(filePath, json);

                var latestFilePath = Path.Combine(reportsDirectory, "teacher-sync-latest.json");
                await System.IO.File.WriteAllTextAsync(latestFilePath, json);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save teacher sync summary");
            }
        }

        private async Task<TeacherSyncResult> SyncRegionInternal(int regionId)
        {
            var result = new TeacherSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                using var client = new HttpClient();
                var url = $"https://api.emis.am/v1/get_personnel/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var teachersArray = JArray.Parse(responseText);

                var teachersToProcess = new List<Teacher>();
                foreach (var teacherToken in teachersArray)
                {
                    if (teacherToken is not JObject teacherObj)
                        continue;

                    var personId = teacherObj["person_id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(personId))
                        continue;

                    var now = DateTime.UtcNow;
                    DateTime? dateOfBirth = ParseNullableDate(teacherObj["date_of_birth"]?.ToString());
                    DateTime? commandDate = ParseNullableDate(teacherObj["command_date"]?.ToString());

                    teachersToProcess.Add(new Teacher
                    {
                        Id = Guid.NewGuid(),
                        PersonId = personId,
                        SchoolId = teacherObj["school_id"]?.ToString() ?? string.Empty,
                        SchoolName = teacherObj["school_name"]?.ToString() ?? string.Empty,
                        Email = teacherObj["email"]?.ToString() ?? string.Empty,
                        Activated = teacherObj["activated"]?.ToString() ?? string.Empty,
                        FirstName = teacherObj["first_name"]?.ToString() ?? string.Empty,
                        LastName = teacherObj["last_name"]?.ToString() ?? string.Empty,
                        FatherName = teacherObj["father_name"]?.ToString() ?? string.Empty,
                        Sex = teacherObj["sex"]?.ToString() ?? string.Empty,
                        WorkType = teacherObj["work_type"]?.ToString() ?? string.Empty,
                        SocNumber = teacherObj["soc_number"]?.ToString() ?? string.Empty,
                        DateOfBirth = dateOfBirth,
                        Address = teacherObj["address"]?.ToString() ?? string.Empty,
                        Phone = teacherObj["phone"]?.ToString() ?? string.Empty,
                        Education = teacherObj["education"]?.ToString() ?? string.Empty,
                        CommandDate = commandDate,
                        SubjectId = teacherObj["subject_id"]?.ToString() ?? string.Empty,
                        MainSubject = teacherObj["main_subject"]?.ToString() ?? string.Empty,
                        PersonPositions = teacherObj["person_positions"]?.ToString(),
                        SubjectsJson = teacherObj["subjects"]?.ToString(),
                        DigitLevel = teacherObj["digit_level"]?.ToString(),
                        Experience = teacherObj["exp"]?.ToString(),
                        AcademicRank = teacherObj["academic_rank"]?.ToString(),
                        AcademicRankId = teacherObj["academic_rank_ID"]?.ToString(),
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                var personIds = teachersToProcess.Select(t => t.PersonId).Distinct().ToList();
                var existingTeachers = await _context.Teachers
                    .Where(t => personIds.Contains(t.PersonId))
                    .ToListAsync();
                var existingDict = existingTeachers.ToDictionary(t => t.PersonId);

                var newTeachers = new List<Teacher>();
                var updatedCount = 0;

                foreach (var teacher in teachersToProcess)
                {
                    if (existingDict.TryGetValue(teacher.PersonId, out var existing))
                    {
                        bool hasChanges = false;

                        hasChanges |= UpdateIfChanged(existing, t => t.SchoolId, (t, v) => t.SchoolId = v, teacher.SchoolId);
                        hasChanges |= UpdateIfChanged(existing, t => t.SchoolName, (t, v) => t.SchoolName = v, teacher.SchoolName);
                        hasChanges |= UpdateIfChanged(existing, t => t.Email, (t, v) => t.Email = v, teacher.Email);
                        hasChanges |= UpdateIfChanged(existing, t => t.Activated, (t, v) => t.Activated = v, teacher.Activated);
                        hasChanges |= UpdateIfChanged(existing, t => t.FirstName, (t, v) => t.FirstName = v, teacher.FirstName);
                        hasChanges |= UpdateIfChanged(existing, t => t.LastName, (t, v) => t.LastName = v, teacher.LastName);
                        hasChanges |= UpdateIfChanged(existing, t => t.FatherName, (t, v) => t.FatherName = v, teacher.FatherName);
                        hasChanges |= UpdateIfChanged(existing, t => t.Sex, (t, v) => t.Sex = v, teacher.Sex);
                        hasChanges |= UpdateIfChanged(existing, t => t.WorkType, (t, v) => t.WorkType = v, teacher.WorkType);
                        hasChanges |= UpdateIfChanged(existing, t => t.SocNumber, (t, v) => t.SocNumber = v, teacher.SocNumber);
                        hasChanges |= UpdateIfChanged(existing, t => t.DateOfBirth, (t, v) => t.DateOfBirth = v, teacher.DateOfBirth);
                        hasChanges |= UpdateIfChanged(existing, t => t.Address, (t, v) => t.Address = v, teacher.Address);
                        hasChanges |= UpdateIfChanged(existing, t => t.Phone, (t, v) => t.Phone = v, teacher.Phone);
                        hasChanges |= UpdateIfChanged(existing, t => t.Education, (t, v) => t.Education = v, teacher.Education);
                        hasChanges |= UpdateIfChanged(existing, t => t.CommandDate, (t, v) => t.CommandDate = v, teacher.CommandDate);
                        hasChanges |= UpdateIfChanged(existing, t => t.SubjectId, (t, v) => t.SubjectId = v, teacher.SubjectId);
                        hasChanges |= UpdateIfChanged(existing, t => t.MainSubject, (t, v) => t.MainSubject = v, teacher.MainSubject);
                        hasChanges |= UpdateIfChanged(existing, t => t.PersonPositions, (t, v) => t.PersonPositions = v, teacher.PersonPositions);
                        hasChanges |= UpdateIfChanged(existing, t => t.SubjectsJson, (t, v) => t.SubjectsJson = v, teacher.SubjectsJson);
                        hasChanges |= UpdateIfChanged(existing, t => t.DigitLevel, (t, v) => t.DigitLevel = v, teacher.DigitLevel);
                        hasChanges |= UpdateIfChanged(existing, t => t.Experience, (t, v) => t.Experience = v, teacher.Experience);
                        hasChanges |= UpdateIfChanged(existing, t => t.AcademicRank, (t, v) => t.AcademicRank = v, teacher.AcademicRank);
                        hasChanges |= UpdateIfChanged(existing, t => t.AcademicRankId, (t, v) => t.AcademicRankId = v, teacher.AcademicRankId);

                        if (hasChanges)
                        {
                            existing.UpdatedAt = DateTime.UtcNow;
                            updatedCount++;
                            result.TeachersUpdatedList.Add(MapToTeacherDto(existing));
                        }
                    }
                    else
                    {
                        newTeachers.Add(teacher);
                    }
                }

                if (newTeachers.Any())
                {
                    _context.Teachers.AddRange(newTeachers);
                }

                if (updatedCount > 0 || newTeachers.Any())
                {
                    await _context.SaveChangesAsync();
                }

                result.Success = true;
                result.TeachersProcessed = teachersToProcess.Count;
                result.TeachersAdded = newTeachers.Count;
                result.TeachersUpdated = updatedCount;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        [HttpPost("sync/{regionId?}")]
        public async Task<IActionResult> SyncTeachers([FromRoute] int regionId = 1)
        {
            var result = await SyncRegionInternal(regionId);

            if (result.Success)
            {
                return Ok(new
                {
                    message = "Teacher sync completed successfully!",
                    regionId = result.RegionId,
                    teachersProcessed = result.TeachersProcessed,
                    teachersAdded = result.TeachersAdded,
                    teachersUpdated = result.TeachersUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "Teacher sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestTeacherSync()
        {
            try
            {
                var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TeacherSyncReports");
                var latestFilePath = Path.Combine(reportsDirectory, "teacher-sync-latest.json");

                if (!System.IO.File.Exists(latestFilePath))
                {
                    return NotFound(new { message = "No teacher sync file found. Run a sync first." });
                }

                var jsonContent = System.IO.File.ReadAllText(latestFilePath);
                var summary = JsonConvert.DeserializeObject<TeacherSyncSummaryDto>(jsonContent);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load teacher sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestTeacherChanges()
        {
            try
            {
                var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TeacherSyncReports");
                var latestFilePath = Path.Combine(reportsDirectory, "teacher-sync-latest.json");

                if (!System.IO.File.Exists(latestFilePath))
                {
                    return NotFound(new { message = "No teacher sync file found. Run a sync first." });
                }

                var jsonContent = System.IO.File.ReadAllText(latestFilePath);
                var summary = JsonConvert.DeserializeObject<TeacherSyncSummaryDto>(jsonContent);

                if (summary == null)
                {
                    return NotFound(new { message = "Could not parse teacher sync file." });
                }

                var dto = new TeacherChangedEntitiesDto
                {
                    TeachersUpdated = summary.AllTeachersUpdated
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load teacher changes", message = ex.Message });
            }
        }

        private static bool UpdateIfChanged<T>(
            Teacher target,
            Func<Teacher, T> getter,
            Action<Teacher, T> setter,
            T newValue)
        {
            var currentValue = getter(target);
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
            {
                return false;
            }

            setter(target, newValue);
            return true;
        }

        private static bool UpdateIfChanged(
            Teacher target,
            Func<Teacher, DateTime?> getter,
            Action<Teacher, DateTime?> setter,
            DateTime? newValue)
        {
            var currentValue = getter(target);

            if (currentValue.HasValue && newValue.HasValue && currentValue.Value == newValue.Value)
            {
                return false;
            }

            if (!currentValue.HasValue && !newValue.HasValue)
            {
                return false;
            }

            setter(target, newValue);
            return true;
        }

        private static DateTime? ParseNullableDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value, out var parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}

