using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using PordznakanAPI.Enums;
using PordznakanAPI.Services;
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
                KtakTeacherId = teacher.KtakTeacherId,
                KtakSchoolId = teacher.KtakSchoolId,
                RegionId = teacher.RegionId,
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
                Subjects = new List<TeacherSubjectDto>() // Will be populated separately
            };
        }

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
            return SyncHelpers.ComputeMd5(
                ktakTeacherId.ToString(),
                ktakSchoolId.ToString(),
                place.ToString(),
                firstName,
                lastName,
                fatherName,
                gender ? "1" : "0",
                birthday.HasValue ? birthday.Value.ToString("yyyy-MM-dd") : string.Empty,
                phone,
                address,
                email,
                socNumber,
                experience.ToString(),
                academicRank.ToString(),
                education.ToString(),
                commandDate.HasValue ? commandDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : string.Empty,
                digitLevel.ToString(),
                activated,
                workType,
                mainSubjectId,
                mainSubject);
        }

        // === Helper mappers for enums ===
        private static bool MapGender(string? sexCode)
        {
            // Based on your data: "47" seems to be a code. Adjust based on API docs.
            // Common pattern: 46=male, 47=female, or 1=male, 0/2=female
            var v = sexCode?.Trim();
            // Adjust this mapping - for now treating "48" or "1" as male
            return !string.IsNullOrWhiteSpace(v) && (v == "1" || v == "48");
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
        [NonAction]
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

            _logger?.LogInformation($"Teacher sync completed for all regions. " +
                $"Success: {successCount}/{regionIds.Length}. " +
                $"Total - Teachers: {summary.TotalTeachersAdded} added, {summary.TotalTeachersUpdated} updated.");
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
                var url = $"https://crmapi.dshh.am/api/Integration/SendRequest?myUrl=v1/get_personnel/{regionId}";
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

                // Log sample teacher structure to debug
                if (teachersArray.Count > 0 && teachersArray[0] is JObject firstTeacher)
                {
                    var sampleFields = string.Join(", ", firstTeacher.Properties().Select(p => p.Name));
                    _logger?.LogInformation($"[Region {regionId}] Sample teacher fields: {sampleFields}");
                    
                    if (firstTeacher["subjects"] != null)
                    {
                        var subjectsValue = firstTeacher["subjects"];
                        _logger?.LogInformation($"[Region {regionId}] Sample teacher 'subjects' field type: {subjectsValue?.Type}, Value: {subjectsValue?.ToString(Formatting.None)?.Substring(0, Math.Min(200, subjectsValue?.ToString(Formatting.None)?.Length ?? 0))}");
                    }
                    else
                    {
                        _logger?.LogWarning($"[Region {regionId}] ⚠️ Sample teacher does NOT have 'subjects' field!");
                    }
                }

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
                        RegionId = regionId,
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

                // ============================================================================
                // STEP 1: PROCESS AND SAVE TEACHERS FIRST (so we have Teacher IDs for FK)
                // ============================================================================
                _logger?.LogInformation($"[Region {regionId}] Step 1: Processing and saving teachers...");
                
                // Get existing teachers by KtakTeacherId
                var stagedIds = stagingRows.Select(s => s.KtakTeacherId).Distinct().ToList();
                var existingTeachers = await _context.Teachers
                    .Where(t => stagedIds.Contains(t.KtakTeacherId))
                    .ToListAsync();

                var existingDict = existingTeachers.ToDictionary(t => t.KtakTeacherId);

                var newTeachers = new List<Teacher>();
                var updatedCount = 0;
                var updatedTeacherIds = new HashSet<int>(); // Track which teachers were updated (by KtakTeacherId)

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
                            existing.RegionId = staging.RegionId;
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
                            updatedTeacherIds.Add(staging.KtakTeacherId);
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
                            RegionId = staging.RegionId,
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

                _logger?.LogInformation($"[Region {regionId}] Teacher MD5 compare done. New={newTeachers.Count}, Updated={updatedCount}. Saving teachers...");

                // Save teachers to database FIRST (so we have their IDs for subjects FK)
                if (newTeachers.Any())
                {
                    _context.Teachers.AddRange(newTeachers);
                }

                if (updatedCount > 0 || newTeachers.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation($"[Region {regionId}] ✅ Teachers saved successfully. New: {newTeachers.Count}, Updated: {updatedCount}");
                }

                // Reload all teachers (including new ones) to get their IDs
                var allTeachers = await _context.Teachers
                    .Where(t => stagedIds.Contains(t.KtakTeacherId))
                    .ToListAsync();
                var teacherIdByKtakId = allTeachers.ToDictionary(t => t.KtakTeacherId, t => t.Id);

                // ============================================================================
                // STEP 2: PROCESS AND SAVE SUBJECTS (using TeacherId FK)
                // After saving teachers, insert all subjects into the database.
                // If subjects are null/empty, continue processing (no error).
                // ============================================================================
                _logger?.LogInformation($"[Region {regionId}] Step 2: Processing subjects for {teachersJsonData.Count} teachers...");
                
                // Log a sample teacher's JSON structure for debugging
                if (teachersJsonData.Any())
                {
                    var firstKvp = teachersJsonData.First();
                    var sampleTeacherId = firstKvp.Key;
                    var sampleTeacher = firstKvp.Value;
                    var sampleFields = string.Join(", ", sampleTeacher.Properties().Select(p => p.Name));
                    _logger?.LogInformation($"[Region {regionId}] 📋 Sample teacher {sampleTeacherId} fields: {sampleFields}");
                    
                    var sampleSubjects = sampleTeacher["subjects"];
                    if (sampleSubjects != null)
                    {
                        _logger?.LogInformation($"[Region {regionId}] 📚 Sample teacher {sampleTeacherId} 'subjects' field exists. Type: {sampleSubjects.Type}, Value (first 500 chars): {sampleSubjects.ToString(Formatting.None)?.Substring(0, Math.Min(500, sampleSubjects.ToString(Formatting.None)?.Length ?? 0))}");
                    }
                    else
                    {
                        _logger?.LogWarning($"[Region {regionId}] ⚠️ Sample teacher {sampleTeacherId} does NOT have 'subjects' field!");
                    }
                }
                
                var totalSubjectsProcessed = 0;
                var totalSubjectsAdded = 0;
                var totalSubjectsSkipped = 0;
                var teachersWithSubjectChanges = new HashSet<int>(); // Track teachers whose subjects were updated (by KtakTeacherId)
                var teachersWithNoSubjects = 0;
                
                // Clear existing subjects for all staged teachers (will be replaced with new ones)
                var teacherIdsToClear = allTeachers.Select(t => t.Id).ToList();
                
                var existingSubjects = await _context.TeacherSubjects
                    .Where(ts => teacherIdsToClear.Contains(ts.TeacherId))
                    .ToListAsync();
                
                if (existingSubjects.Any())
                {
                    _context.TeacherSubjects.RemoveRange(existingSubjects);
                    totalSubjectsProcessed = existingSubjects.Count;
                    _logger?.LogInformation($"[Region {regionId}] Cleared {existingSubjects.Count} existing subjects for staged teachers");
                }
                
                // Process subjects from JSON for each teacher
                foreach (var kvp in teachersJsonData)
                {
                    var ktakTeacherId = kvp.Key;
                    var teacherObj = kvp.Value;

                    // Get the Teacher's Guid ID (must exist since we just saved teachers)
                    if (!teacherIdByKtakId.TryGetValue(ktakTeacherId, out var teacherId))
                    {
                        _logger?.LogWarning($"[Region {regionId}] Teacher with KtakTeacherId {ktakTeacherId} not found, skipping subjects");
                        continue;
                    }

                    // Parse and save subjects array - if null/empty, just continue to next teacher
                    var subjectsToken = teacherObj["subjects"];
                    JArray? subjectsArray = null;
                    
                    // Handle different formats: direct array, string containing JSON, or null
                    if (subjectsToken != null && subjectsToken.Type != JTokenType.Null)
                    {
                        if (subjectsToken is JArray directArray)
                        {
                            subjectsArray = directArray;
                        }
                        else if (subjectsToken.Type == JTokenType.String)
                        {
                            // Parse string containing JSON array
                            var subjectsJsonString = subjectsToken.Value<string>();
                            if (!string.IsNullOrWhiteSpace(subjectsJsonString))
                            {
                                try
                                {
                                    var parsedToken = JToken.Parse(subjectsJsonString);
                                    if (parsedToken is JArray parsedArray)
                                    {
                                        subjectsArray = parsedArray;
                                    }
                                }
                                catch (Exception parseEx)
                                {
                                    _logger?.LogWarning($"[Region {regionId}] Failed to parse subjects JSON string for teacher {ktakTeacherId}: {parseEx.Message}");
                                }
                            }
                        }
                    }
                    
                    // Check if subjects exist and is a valid array
                    if (subjectsArray != null && subjectsArray.Count > 0)
                    {
                        _logger?.LogDebug($"[Region {regionId}] Processing {subjectsArray.Count} subjects for teacher {ktakTeacherId}");
                        
                        var subjectsAdded = 0;
                        var subjectsSkipped = 0;
                        var seenSubjects = new HashSet<string>(); // Track duplicates: "subjectId-grade-subGrade"
                        
                        foreach (var subjectToken in subjectsArray)
                        {
                            if (subjectToken is not JObject subjectObj)
                            {
                                subjectsSkipped++;
                                totalSubjectsSkipped++;
                                continue;
                            }

                            // Parse subject_id (can be number or string)
                            int subjectId;
                            var subjectIdToken = subjectObj["subject_id"];
                            if (subjectIdToken == null)
                            {
                                subjectsSkipped++;
                                totalSubjectsSkipped++;
                                continue;
                            }
                            
                            // Handle both number and string types
                            if (subjectIdToken.Type == JTokenType.Integer)
                            {
                                subjectId = subjectIdToken.Value<int>();
                            }
                            else if (!int.TryParse(subjectIdToken.ToString(), out subjectId))
                            {
                                subjectsSkipped++;
                                totalSubjectsSkipped++;
                                continue;
                            }

                            // Parse grade (can be number or string)
                            EGrade grade = (EGrade)0;
                            var gradeToken = subjectObj["grade"];
                            if (gradeToken != null)
                            {
                                if (gradeToken.Type == JTokenType.Integer)
                                {
                                    var gradeValue = gradeToken.Value<int>();
                                    grade = MapGrade(gradeValue);
                                }
                                else if (int.TryParse(gradeToken.ToString(), out var parsedGrade))
                                {
                                    grade = MapGrade(parsedGrade);
                                }
                            }

                            // Parse classifier (subGrade) - should be string like "ա", "բ"
                            var classifier = subjectObj["classifier"]?.ToString();
                            var subGrade = MapSubGrade(classifier);
                            
                            // Parse subject_title (Name)
                            var subjectTitle = subjectObj["subject_title"]?.ToString() ?? string.Empty;

                            // Check for duplicates within the same API response
                            var subjectKey = $"{subjectId}-{grade}-{subGrade}";
                            if (seenSubjects.Contains(subjectKey))
                            {
                                subjectsSkipped++;
                                totalSubjectsSkipped++;
                                continue;
                            }
                            
                            seenSubjects.Add(subjectKey);

                            var newSubject = new TeacherSubject
                            {
                                Id = Guid.NewGuid(),
                                TeacherId = teacherId,
                                SubjectId = subjectId,
                                Grade = grade,
                                SubGrade = subGrade,
                                Name = subjectTitle
                            };
                            
                            _context.TeacherSubjects.Add(newSubject);
                            subjectsAdded++;
                            totalSubjectsAdded++;
                        }

                        if (subjectsAdded > 0)
                        {
                            teachersWithSubjectChanges.Add(ktakTeacherId);
                            _logger?.LogDebug($"[Region {regionId}] Added {subjectsAdded} subjects for teacher {ktakTeacherId}");
                        }
                    }
                    else
                    {
                        // Subjects are null, empty, or invalid - just continue (don't log as error)
                        teachersWithNoSubjects++;
                    }
                }
                
                _logger?.LogInformation($"[Region {regionId}] 📊 Subject processing summary: TotalAdded={totalSubjectsAdded}, TotalSkipped={totalSubjectsSkipped}, TeachersWithSubjects={teachersWithSubjectChanges.Count}, TeachersWithNoSubjects={teachersWithNoSubjects}, Removed={totalSubjectsProcessed}");
                
                // Save subjects to database - always try to save, even if null/empty (just continues if nothing to save)
                try
                {
                    if (totalSubjectsAdded > 0 || totalSubjectsProcessed > 0)
                    {
                        _logger?.LogInformation($"[Region {regionId}] 💾 Saving {totalSubjectsAdded} new subjects and removing {totalSubjectsProcessed} old subjects to database...");
                        
                        var savedCount = await _context.SaveChangesAsync();
                        _logger?.LogInformation($"[Region {regionId}] ✅ SaveChangesAsync completed. Entities saved: {savedCount}");
                        
                        // Verify subjects were actually saved
                        var savedSubjectsCount = await _context.TeacherSubjects
                            .Where(ts => teacherIdsToClear.Contains(ts.TeacherId))
                            .CountAsync();
                        _logger?.LogInformation($"[Region {regionId}] ✅ Verification: Found {savedSubjectsCount} subjects in database for {teacherIdsToClear.Count} teachers after save.");
                    }
                    else
                    {
                        _logger?.LogInformation($"[Region {regionId}] No subjects to save (all teachers have null/empty subjects). Continuing...");
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    _logger?.LogError(dbEx, $"[Region {regionId}] ❌ DATABASE ERROR saving subjects! Message: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        _logger?.LogError($"[Region {regionId}] Inner exception: {dbEx.InnerException.Message}");
                    }
                    // Continue processing even if subject save fails
                    _logger?.LogWarning($"[Region {regionId}] ⚠️ Continuing despite subject save error...");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"[Region {regionId}] ❌ ERROR saving subjects to database! Exception: {ex.Message}");
                    // Continue processing even if subject save fails
                    _logger?.LogWarning($"[Region {regionId}] ⚠️ Continuing despite subject save error...");
                }

                // ============================================================================
                // STEP 3: RELOAD ALL CHANGED TEACHERS WITH SUBJECTS AND MAP TO DTOs
                // ============================================================================
                var allChangedKtakTeacherIds = updatedTeacherIds
                    .Union(newTeachers.Select(t => t.KtakTeacherId))
                    .Union(teachersWithSubjectChanges)
                    .Distinct()
                    .ToList();
                
                if (allChangedKtakTeacherIds.Any())
                {
                    _logger?.LogInformation($"[Region {regionId}] Step 3: Reloading {allChangedKtakTeacherIds.Count} changed teachers (updated: {updatedTeacherIds.Count}, new: {newTeachers.Count}, subjects changed: {teachersWithSubjectChanges.Count}) with subjects for DTO mapping...");
                    
                    // Load teachers
                    var changedTeachers = await _context.Teachers
                        .Where(t => allChangedKtakTeacherIds.Contains(t.KtakTeacherId))
                        .ToListAsync();

                    _logger?.LogInformation($"[Region {regionId}] Loaded {changedTeachers.Count} teachers from database for DTO mapping");

                    // Load subjects directly from database (more reliable than navigation property)
                    var teacherIds = changedTeachers.Select(t => t.Id).ToList();
                    var allSubjects = await _context.TeacherSubjects
                        .Where(ts => teacherIds.Contains(ts.TeacherId))
                        .ToListAsync();
                    
                    var subjectsByTeacherId = allSubjects.GroupBy(s => s.TeacherId)
                        .ToDictionary(g => g.Key, g => g.ToList());
                    
                    _logger?.LogInformation($"[Region {regionId}] Direct database query: Found {allSubjects.Count} subjects for {subjectsByTeacherId.Count} teachers (out of {teacherIds.Count} total teachers)");

                    foreach (var teacher in changedTeachers)
                    {
                        var teacherDto = MapToTeacherDto(teacher);
                        
                        // Map subjects from the directly loaded subjects
                        if (subjectsByTeacherId.TryGetValue(teacher.Id, out var teacherSubjects) && teacherSubjects.Any())
                        {
                            teacherDto.Subjects = teacherSubjects.Select(s => new TeacherSubjectDto
                            {
                                Id = s.Id,
                                TeacherDtoId = teacherDto.Id,
                                TeacherDto = teacherDto,
                                SubjectId = s.SubjectId,
                                Grade = s.Grade,
                                SubGrade = s.SubGrade,
                                Name = s.Name
                            }).ToList();
                        }
                        else
                        {
                            teacherDto.Subjects = new List<TeacherSubjectDto>();
                        }
                        
                        result.TeachersUpdatedList.Add(teacherDto);
                    }
                    
                    var totalSubjectsInDtos = result.TeachersUpdatedList.Sum(t => t.Subjects.Count);
                    _logger?.LogInformation($"[Region {regionId}] Mapped {changedTeachers.Count} teachers with subjects to DTOs. Total subjects in DTOs: {totalSubjectsInDtos}");
                    
                    if (totalSubjectsInDtos == 0 && allSubjects.Count > 0)
                    {
                        _logger?.LogWarning($"[Region {regionId}] ⚠️ Subjects exist in DB ({allSubjects.Count}) but not mapped to DTOs!");
                    }
                    else if (totalSubjectsInDtos == 0)
                    {
                        _logger?.LogWarning($"[Region {regionId}] ⚠️ No subjects found in database for these teachers. Check if subjects were saved successfully in Step 2.");
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

        [HttpPost("sync/{regionId?}")]
        public async Task<IActionResult> SyncTeachers([FromRoute] int regionId)
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