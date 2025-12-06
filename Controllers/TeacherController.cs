using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using PordznakanAPI.Enums;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
                KtakTeacherId = teacher.KtakTeacherId,
                KtakSchoolId = teacher.KtakSchoolId,
                Place = teacher.Place,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                FatherName = teacher.FatherName,
                Gender = teacher.Gender,
                Birthday = teacher.Birthday,
                Phone = teacher.Phone,
                Address = teacher.Address,
                Email = teacher.Email,
                SocNumber = teacher.SocNumber,
                Experience = teacher.Experience,
                AcademicRank = teacher.AcademicRank,
                Education = teacher.Education,
                CommandDate = teacher.CommandDate,
                DigitLevel = teacher.DigitLevel,
                Activated = teacher.Activated,
                WorkType = teacher.WorkType,
                CreatedAt = teacher.CreatedAt,
                UpdatedAt = teacher.UpdatedAt,
                Subjects = teacher.Subjects.Select(s => new TeacherSubjectDto
                {
                    SubjectId = s.SubjectId,
                    Grade = s.Grade,
                    SubGrade = s.SubGrade,
                    Name = s.Name
                }).ToList()
            };
        }

        // === Helper for MD5 generation ===
        private static string ComputeTeacherMd5(
            int ktakTeacherId,
            int ktakSchoolId,
            KtakPlace place,
            string firstName,
            string lastName,
            string fatherName,
            bool gender,
            DateOnly? birthday,
            string phone,
            string address,
            string email,
            string socNumber,
            int experience,
            ERank academicRank,
            EEducation education,
            DateTime? commandDate,
            EDigitLevel digitLevel,
            string activated,
            string workType,
            string mainSubjectId,
            string mainSubject)
        {
            var raw = string.Join('|', new[]
            {
                ktakTeacherId.ToString(),
                ktakSchoolId.ToString(),
                place.ToString(),
                firstName ?? string.Empty,
                lastName ?? string.Empty,
                fatherName ?? string.Empty,
                gender ? "1" : "0",
                birthday.HasValue ? birthday.Value.ToString("yyyy-MM-dd") : string.Empty,
                phone ?? string.Empty,
                address ?? string.Empty,
                email ?? string.Empty,
                socNumber ?? string.Empty,
                experience.ToString(),
                academicRank.ToString(),
                education.ToString(),
                commandDate.HasValue ? commandDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : string.Empty,
                digitLevel.ToString(),
                activated ?? string.Empty,
                workType ?? string.Empty,
                mainSubjectId ?? string.Empty,
                mainSubject ?? string.Empty
            });

            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = md5.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        // === Helper mappers for enums ===
        private static bool MapGender(string? sexCode)
        {
            // Based on your data: "47" seems to be a code. Adjust based on API docs.
            // Common pattern: 46=male, 47=female, or 1=male, 0/2=female
            var v = sexCode?.Trim();
            // Adjust this mapping - for now treating "46" or "1" as male
            return !string.IsNullOrWhiteSpace(v) && (v == "1" || v == "46");
        }

        private static EEducation MapEducation(string? educationCode)
        {
            if (string.IsNullOrWhiteSpace(educationCode))
                return EEducation.Unknown;

            if (int.TryParse(educationCode, out var code))
            {
                return code switch
                {
                    80 => EEducation.Higher,
                    81 => EEducation.Incomplete,
                    82 => EEducation.Professional,
                    83 => EEducation.Unregistered,
                    84 => EEducation.Secondary,
                    _ => EEducation.Unknown
                };
            }

            return EEducation.Unknown;
        }

        private static ERank MapAcademicRank(string? rank, string? rankId)
        {
            if (string.IsNullOrWhiteSpace(rank) && string.IsNullOrWhiteSpace(rankId))
                return ERank.Unknown;

            if (string.IsNullOrWhiteSpace(rank))
                return ERank.Absence;

            var rankLower = rank.Trim().ToLowerInvariant();
            return rankLower switch
            {
                "դոցենտ" or "docent" => ERank.Docent,
                "պրոֆեսոր" or "professor" => ERank.Professor,
                _ => ERank.Unknown
            };
        }

        private static EDigitLevel MapDigitLevel(string? digitLevel)
        {
            if (string.IsNullOrWhiteSpace(digitLevel))
                return EDigitLevel.Unknown;

            if (int.TryParse(digitLevel, out var level))
            {
                return level switch
                {
                    1 => EDigitLevel.C1,
                    2 => EDigitLevel.C2,
                    3 => EDigitLevel.C3,
                    4 => EDigitLevel.C4,
                    _ => EDigitLevel.Unknown
                };
            }

            return EDigitLevel.Unknown;
        }

        private static EGrade MapGrade(int? grade)
        {
            if (grade.HasValue && grade.Value >= 1 && grade.Value <= 12)
            {
                return (EGrade)grade.Value;
            }
            return 0; // default
        }

        private static ESubGrade MapSubGrade(string? classifier)
        {
            if (string.IsNullOrWhiteSpace(classifier))
                return ESubGrade.Unknown;

            return classifier.Trim() switch
            {
                "ա" => ESubGrade.Sg1,
                "բ" => ESubGrade.Sg2,
                "գ" => ESubGrade.Sg3,
                "դ" => ESubGrade.Sg4,
                "ե" => ESubGrade.Sg5,
                "զ" => ESubGrade.Sg6,
                "է" => ESubGrade.Sg7,
                "ը" => ESubGrade.Sg8,
                "թ" => ESubGrade.Sg9,
                "ժ" => ESubGrade.Sg10,
                "ի" => ESubGrade.Sg11,
                "լ" => ESubGrade.Sg12,
                "խ" => ESubGrade.Sg13,
                "ծ" => ESubGrade.Sg14,
                "կ" => ESubGrade.Sg15,
                "հ" => ESubGrade.Sg16,
                "ձ" => ESubGrade.Sg17,
                "ռ" => ESubGrade.Sg18,
                _ => ESubGrade.Unknown
            };
        }

        /// <summary>
        /// Syncs all 10 regions. This method is designed to be called by Hangfire.
        /// </summary>
        public async Task SyncAllRegions()
        {
            var regionIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var results = new List<TeacherSyncResult>();
            var syncStartTime = DateTime.UtcNow;

            _logger?.LogInformation($"Starting teacher sync for all {regionIds.Length} regions at {syncStartTime}");

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Syncing teachers for region {regionId}...");
                    var result = await SyncRegionInternal(regionId);
                    results.Add(result);
                    
                    if (result.Success)
                    {
                        _logger?.LogInformation($"Region {regionId} teacher sync completed successfully. " +
                            $"Teachers: {result.TeachersAdded} added, {result.TeachersUpdated} updated.");
                    }
                    else
                    {
                        _logger?.LogError($"Region {regionId} teacher sync failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while syncing teachers for region {regionId}");
                    results.Add(new TeacherSyncResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        RegionId = regionId
                    });
                }
            }

            var syncEndTime = DateTime.UtcNow;
            var successCount = results.Count(r => r.Success);
            var failedCount = results.Count(r => !r.Success);

            // Create summary of all changes
            var summary = new TeacherSyncSummaryDto
            {
                SyncCompletedAt = syncEndTime,
                TotalRegionsProcessed = regionIds.Length,
                SuccessfulRegions = successCount,
                FailedRegions = failedCount,
                TotalTeachersAdded = results.Sum(r => r.TeachersAdded),
                TotalTeachersUpdated = results.Sum(r => r.TeachersUpdated)
            };

            // Aggregate all changed entities
            foreach (var result in results.Where(r => r.Success))
            {
                summary.AllTeachersUpdated.AddRange(result.TeachersUpdatedList);
            }

            // Generate and save JSON file
            await SaveChangesToJsonFile(summary);

            _logger?.LogInformation($"Teacher sync completed for all regions. " +
                $"Success: {successCount}/{regionIds.Length}. " +
                $"Total - Teachers: {summary.TotalTeachersAdded} added, {summary.TotalTeachersUpdated} updated. " +
                $"Changes saved to JSON file.");

            // Send updated teachers to external API
            if (summary.AllTeachersUpdated.Any())
            {
                await SendUpdatedTeachersToApi(summary.AllTeachersUpdated);
            }
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
                _logger?.LogInformation($"[Region {regionId}] Fetching teachers from: {url}");
                
                var responseText = await client.GetStringAsync(url);
                _logger?.LogDebug($"[Region {regionId}] API response length: {responseText?.Length ?? 0} characters");
                
                // Parse the response - it might be an array or an object containing an array
                JArray? teachersArray = null;
                JToken token;
                
                try
                {
                    token = JToken.Parse(responseText);
                }
                catch (Exception parseEx)
                {
                    var preview = responseText?.Length > 500 ? responseText.Substring(0, 500) : responseText;
                    throw new Exception($"Failed to parse JSON response. Response preview: {preview}. Error: {parseEx.Message}", parseEx);
                }
                
                if (token.Type == JTokenType.Array)
                {
                    // Response is directly an array
                    teachersArray = token as JArray;
                    _logger?.LogInformation($"[Region {regionId}] Response is a direct array with {teachersArray?.Count ?? 0} items");
                }
                else if (token.Type == JTokenType.Object)
                {
                    // Response is an object - try to find an array property
                    var obj = token as JObject;
                    if (obj != null)
                    {
                        _logger?.LogInformation($"[Region {regionId}] Response is an object. Properties: {string.Join(", ", obj.Properties().Select(p => p.Name))}");
                        
                        // Try common property names that might contain the array
                        teachersArray = obj["data"] as JArray 
                                     ?? obj["teachers"] as JArray 
                                     ?? obj["results"] as JArray 
                                     ?? obj["items"] as JArray;
                        
                        if (teachersArray == null)
                        {
                            // Check if there's an error message
                            var errorMessage = obj["error"]?.ToString() 
                                            ?? obj["message"]?.ToString()
                                            ?? obj["ErrorMessage"]?.ToString();
                            
                            if (!string.IsNullOrWhiteSpace(errorMessage))
                            {
                                throw new Exception($"API returned an error: {errorMessage}");
                            }
                            
                            // Try to find any array property
                            foreach (var prop in obj.Properties())
                            {
                                if (prop.Value.Type == JTokenType.Array)
                                {
                                    teachersArray = prop.Value as JArray;
                                    _logger?.LogWarning($"[Region {regionId}] Found teachers array in property '{prop.Name}' with {teachersArray.Count} items");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            _logger?.LogInformation($"[Region {regionId}] Found teachers array in common property with {teachersArray.Count} items");
                        }
                    }
                }
                
                if (teachersArray == null)
                {
                    // Last attempt: check if the response is a single teacher object
                    if (token.Type == JTokenType.Object)
                    {
                        var obj = token as JObject;
                        if (obj != null && obj["person_id"] != null)
                        {
                            // It's a single teacher object - wrap it in an array
                            teachersArray = new JArray { obj };
                            _logger?.LogInformation($"[Region {regionId}] Response is a single teacher object, wrapping in array");
                        }
                    }
                    
                    if (teachersArray == null)
                    {
                        var preview = responseText?.Length > 1000 ? responseText.Substring(0, 1000) : responseText;
                        throw new Exception($"Unable to parse teachers array from API response. Response type: {token.Type}. Response preview: {preview}");
                    }
                }
                
                _logger?.LogInformation($"[Region {regionId}] Successfully parsed {teachersArray.Count} teachers from API response");

                // Clear staging table for this sync
                await _context.TeachersStaging.ExecuteDeleteAsync();

                var now = DateTime.UtcNow;
                var teachersJsonData = new Dictionary<int, JObject>(); // Store JSON for subjects processing later

                // Process all teachers from API into staging
                foreach (var teacherToken in teachersArray)
                {
                    if (teacherToken is not JObject teacherObj)
                        continue;

                    var personIdStr = teacherObj["person_id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(personIdStr) || !int.TryParse(personIdStr, out var ktakTeacherId))
                        continue;

                    var schoolIdStr = teacherObj["school_id"]?.ToString();
                    if (string.IsNullOrWhiteSpace(schoolIdStr) || !int.TryParse(schoolIdStr, out var ktakSchoolId))
                        continue;

                    // Store JSON data for later subject processing
                    teachersJsonData[ktakTeacherId] = teacherObj;

                    // Parse date of birth
                    DateOnly? birthday = null;
                    var dobString = teacherObj["date_of_birth"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(dobString) && DateOnly.TryParse(dobString, out var parsedDate))
                    {
                        birthday = parsedDate;
                    }

                    // Parse command date
                    DateTime? commandDate = ParseNullableDate(teacherObj["command_date"]?.ToString());

                    // Parse experience
                    int experience = 0;
                    var expStr = teacherObj["exp"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(expStr) && int.TryParse(expStr, out var exp))
                    {
                        experience = exp;
                    }

                    // Map enums
                    var gender = MapGender(teacherObj["sex"]?.ToString());
                    var education = MapEducation(teacherObj["education"]?.ToString());
                    var academicRank = MapAcademicRank(
                        teacherObj["academic_rank"]?.ToString(),
                        teacherObj["academic_rank_ID"]?.ToString());
                    var digitLevel = MapDigitLevel(teacherObj["digit_level"]?.ToString());

                    var firstName = teacherObj["first_name"]?.ToString() ?? string.Empty;
                    var lastName = teacherObj["last_name"]?.ToString() ?? string.Empty;
                    var fatherName = teacherObj["father_name"]?.ToString() ?? string.Empty;
                    var phone = teacherObj["phone"]?.ToString() ?? string.Empty;
                    var address = teacherObj["address"]?.ToString() ?? string.Empty;
                    var email = teacherObj["email"]?.ToString() ?? string.Empty;
                    var socNumber = teacherObj["soc_number"]?.ToString() ?? string.Empty;
                    var activated = teacherObj["activated"]?.ToString() ?? string.Empty;
                    var workType = teacherObj["work_type"]?.ToString() ?? string.Empty;
                    var mainSubjectId = teacherObj["subject_id"]?.ToString() ?? string.Empty;
                    var mainSubject = teacherObj["main_subject"]?.ToString() ?? string.Empty;

                    // Compute MD5
                    var md5 = ComputeTeacherMd5(
                        ktakTeacherId,
                        ktakSchoolId,
                        KtakPlace.School,
                        firstName,
                        lastName,
                        fatherName,
                        gender,
                        birthday,
                        phone,
                        address,
                        email,
                        socNumber,
                        experience,
                        academicRank,
                        education,
                        commandDate,
                        digitLevel,
                        activated,
                        workType,
                        mainSubjectId,
                        mainSubject);

                    // Stream directly into staging table
                    _context.TeachersStaging.Add(new TeacherStaging
                    {
                        Id = Guid.NewGuid(),
                        KtakTeacherId = ktakTeacherId,
                        KtakSchoolId = ktakSchoolId,
                        Place = KtakPlace.School,
                        FirstName = firstName,
                        LastName = lastName,
                        FatherName = fatherName,
                        Gender = gender,
                        Birthday = birthday,
                        Phone = phone,
                        Address = address,
                        Email = email,
                        SocNumber = socNumber,
                        Experience = experience,
                        AcademicRank = academicRank,
                        Education = education,
                        CommandDate = commandDate,
                        DigitLevel = digitLevel,
                        Activated = activated,
                        WorkType = workType,
                        MainSubjectId = mainSubjectId,
                        MainSubject = mainSubject,
                        PersonPositions = teacherObj["person_positions"]?.ToString(),
                        MD5 = md5,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                // Save all staging data
                await _context.SaveChangesAsync();

                // Load all staged teachers
                var stagingRows = await _context.TeachersStaging.ToListAsync();

                // Get existing teachers by KtakTeacherId
                var stagedIds = stagingRows.Select(s => s.KtakTeacherId).Distinct().ToList();
                var existingTeachers = await _context.Teachers
                    .Include(t => t.Subjects)
                    .Where(t => stagedIds.Contains(t.KtakTeacherId))
                    .ToListAsync();

                var existingDict = existingTeachers.ToDictionary(t => t.KtakTeacherId);

                var newTeachers = new List<Teacher>();
                var updatedCount = 0;

                // Process each staged teacher
                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.KtakTeacherId, out var existing))
                    {
                        // Compare MD5
                        if (!string.Equals(existing.MD5, staging.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            // MD5 changed → update from staging
                            existing.KtakSchoolId = staging.KtakSchoolId;
                            existing.Place = staging.Place;
                            existing.FirstName = staging.FirstName;
                            existing.LastName = staging.LastName;
                            existing.FatherName = staging.FatherName;
                            existing.Gender = staging.Gender;
                            existing.Birthday = staging.Birthday;
                            existing.Phone = staging.Phone;
                            existing.Address = staging.Address;
                            existing.Email = staging.Email;
                            existing.SocNumber = staging.SocNumber;
                            existing.Experience = staging.Experience;
                            existing.AcademicRank = staging.AcademicRank;
                            existing.Education = staging.Education;
                            existing.CommandDate = staging.CommandDate;
                            existing.DigitLevel = staging.DigitLevel;
                            existing.Activated = staging.Activated;
                            existing.WorkType = staging.WorkType;
                            existing.MainSubjectId = staging.MainSubjectId;
                            existing.MainSubject = staging.MainSubject;
                            existing.MD5 = staging.MD5;
                            existing.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                            result.TeachersUpdatedList.Add(MapToTeacherDto(existing));
                        }
                    }
                    else
                    {
                        // New teacher
                        var newTeacher = new Teacher
                        {
                            Id = Guid.NewGuid(),
                            KtakTeacherId = staging.KtakTeacherId,
                            KtakSchoolId = staging.KtakSchoolId,
                            Place = staging.Place,
                            FirstName = staging.FirstName,
                            LastName = staging.LastName,
                            FatherName = staging.FatherName,
                            Gender = staging.Gender,
                            Birthday = staging.Birthday,
                            Phone = staging.Phone,
                            Address = staging.Address,
                            Email = staging.Email,
                            SocNumber = staging.SocNumber,
                            Experience = staging.Experience,
                            AcademicRank = staging.AcademicRank,
                            Education = staging.Education,
                            CommandDate = staging.CommandDate,
                            DigitLevel = staging.DigitLevel,
                            Activated = staging.Activated,
                            WorkType = staging.WorkType,
                            MainSubjectId = staging.MainSubjectId,
                            MainSubject = staging.MainSubject,
                            MD5 = staging.MD5,
                            CreatedAt = staging.CreatedAt,
                            UpdatedAt = staging.UpdatedAt
                        };

                        newTeachers.Add(newTeacher);
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] Teacher MD5 compare done. New={newTeachers.Count}, Updated={updatedCount}. Saving...");

                // Add new teachers
                if (newTeachers.Any())
                {
                    _context.Teachers.AddRange(newTeachers);
                }

                // Save changes
                if (updatedCount > 0 || newTeachers.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Process subjects for all teachers (both new and existing)
                // Reload all teachers to get their IDs
                _logger?.LogInformation($"[Region {regionId}] Processing subjects for {teachersJsonData.Count} teachers...");
                var allTeachersDict = (await _context.Teachers.ToListAsync())
                    .ToDictionary(t => t.KtakTeacherId);

                var totalSubjectsProcessed = 0;
                var totalSubjectsAdded = 0;
                foreach (var kvp in teachersJsonData)
                {
                    var ktakTeacherId = kvp.Key;
                    var teacherObj = kvp.Value;

                    if (!allTeachersDict.TryGetValue(ktakTeacherId, out var teacher))
                        continue;

                    // Clear existing subjects for this teacher (will be replaced with new ones)
                    var existingSubjects = await _context.TeacherSubjects
                        .Where(ts => ts.TeacherId == teacher.Id)
                        .ToListAsync();
                    if (existingSubjects.Any())
                    {
                        _context.TeacherSubjects.RemoveRange(existingSubjects);
                        totalSubjectsProcessed += existingSubjects.Count;
                    }

                    // Parse and save subjects array
                    var subjectsToken = teacherObj["subjects"];
                    if (subjectsToken is JArray subjectsArray && subjectsArray.Count > 0)
                    {
                        var subjectsAdded = 0;
                        foreach (var subjectToken in subjectsArray)
                        {
                            if (subjectToken is not JObject subjectObj)
                                continue;

                            var subjectIdToken = subjectObj["subject_id"];
                            if (subjectIdToken == null || !int.TryParse(subjectIdToken.ToString(), out var subjectId))
                                continue;

                            var gradeToken = subjectObj["grade"];
                            var grade = gradeToken != null && int.TryParse(gradeToken.ToString(), out var g) 
                                ? MapGrade(g) 
                                : (EGrade)0;

                            var subGrade = MapSubGrade(subjectObj["classifier"]?.ToString());
                            var subjectTitle = subjectObj["subject_title"]?.ToString() ?? string.Empty;
                            var classroomId = subjectObj["classroom_id"]?.ToString() ?? string.Empty;

                            _context.TeacherSubjects.Add(new TeacherSubject
                            {
                                Id = Guid.NewGuid(),
                                TeacherId = teacher.Id,
                                SubjectId = subjectId,
                                Grade = grade,
                                SubGrade = subGrade,
                                Name = subjectTitle,
                                ClassroomId = classroomId
                            });
                            subjectsAdded++;
                            totalSubjectsAdded++;
                        }

                        if (subjectsAdded > 0)
                        {
                            _logger?.LogDebug($"[Region {regionId}] Added {subjectsAdded} subjects for teacher {ktakTeacherId}");
                        }
                    }
                    else if (subjectsToken != null && subjectsToken.Type != JTokenType.Null)
                    {
                        // Subjects field exists but is not a valid array - log warning
                        _logger?.LogWarning($"[Region {regionId}] Invalid subjects format for teacher {ktakTeacherId}. Subjects cleared.");
                    }
                }

                // Save subjects to database
                await _context.SaveChangesAsync();
                
                if (totalSubjectsAdded > 0 || totalSubjectsProcessed > 0)
                {
                    _logger?.LogInformation($"[Region {regionId}] Subject processing completed. Added: {totalSubjectsAdded}, Processed (removed/added): {totalSubjectsProcessed + totalSubjectsAdded} subjects.");
                }
                else
                {
                    _logger?.LogInformation($"[Region {regionId}] No subject changes to save.");
                }

                // Add newly added teachers to the updated list (reload with subjects)
                if (newTeachers.Any())
                {
                    var newTeacherIds = newTeachers.Select(t => t.Id).ToList();
                    var reloadedNewTeachers = await _context.Teachers
                        .Include(t => t.Subjects)
                        .Where(t => newTeacherIds.Contains(t.Id))
                        .ToListAsync();

                    foreach (var newTeacher in reloadedNewTeachers)
                    {
                        result.TeachersUpdatedList.Add(MapToTeacherDto(newTeacher));
                    }
                }

                // Cleanup staging
                await _context.TeachersStaging.ExecuteDeleteAsync();

                result.Success = true;
                result.TeachersProcessed = stagingRows.Count;
                result.TeachersAdded = newTeachers.Count;
                result.TeachersUpdated = updatedCount + newTeachers.Count; // Include newly added as updated

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger?.LogError(ex, $"Error syncing teachers for region {regionId}");
                return result;
            }
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

        /// <summary>
        /// Sends updated teachers to the external API endpoint
        /// </summary>
        private async Task SendUpdatedTeachersToApi(List<TeacherDto> updatedTeachers)
        {
            try
            {
                _logger?.LogInformation($"Sending {updatedTeachers.Count} updated teachers to external API...");

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };

                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromMinutes(5);

                var json = JsonConvert.SerializeObject(updatedTeachers, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "https://crm.dshh.am:1400/api/bulk-update/teachers",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogInformation($"Successfully sent {updatedTeachers.Count} teachers to external API. Response: {responseContent}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogError($"Failed to send teachers to external API. Status: {response.StatusCode}, Response: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Exception while sending updated teachers to external API");
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
    }
}