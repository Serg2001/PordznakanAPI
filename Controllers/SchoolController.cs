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
    // Result class for sync operations
    public class SyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int SchoolsProcessed { get; set; }
        public int SchoolsAdded { get; set; }
        public int SchoolsUpdated { get; set; }
        public int ClassroomsProcessed { get; set; }
        public int ClassroomsAdded { get; set; }
        public int ClassroomsUpdated { get; set; }
        public int PupilsProcessed { get; set; }
        public int PupilsAdded { get; set; }
        public int PupilsUpdated { get; set; }
        
        // Lists of changed entities using DTOs
        public List<SchoolDto> SchoolsUpdatedList { get; set; } = new();
        public List<ClassroomDto> ClassroomsUpdatedList { get; set; } = new();
        public List<PupilDto> PupilsUpdatedList { get; set; } = new();
    }

    // Class to hold all changed entities for external API
    public class ChangedEntitiesDto
    {
        public List<SchoolDto> SchoolsUpdated { get; set; } = new();
        public List<ClassroomDto> ClassroomsUpdated { get; set; } = new();
        public List<PupilDto> PupilsUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class PupilController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PupilController>? _logger;

        public PupilController(AppDbContext context, ILogger<PupilController>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        // Helper methods to map Models to DTOs
        private SchoolDto MapToSchoolDto(School school)
        {
            return new SchoolDto
            {
                DshhSchoolId = school.DshhSchoolId,
                KtakSchoolId = school.KtakSchoolId,
                RegionId = school.RegionId,
                Name = school.Name,
                Marz = school.Marz,
                Region = school.Region,
                Community = school.Community,
                CreatedAt = school.CreatedAt,
                UpdatedAt = school.UpdatedAt
            };
        }

        private ClassroomDto MapToClassroomDto(Classroom classroom)
        {
            return new ClassroomDto
            {
                Id = classroom.Id,
                KtakSchoolId = classroom.KtakSchoolId,
                KtakClassroomId = classroom.KtakClassroomId,
                RegionId = classroom.RegionId,
                Grade = classroom.Grade,
                Classifier = classroom.Classifier,
                ClassName = classroom.ClassName,
                Stream = classroom.Stream,
                SchoolId = classroom.SchoolId,
                CreatedAt = classroom.CreatedAt,
                UpdatedAt = classroom.UpdatedAt
            };
        }

        private PupilDto MapToPupilDto(Pupil pupil)
        {
            return new PupilDto
            {
                Id = pupil.Id,
                KtakPupilId = pupil.KtakPupilId,
                KtakSchoolId = pupil.KtakSchoolId,
                RegionId = pupil.RegionId,
                ClassroomId = pupil.ClassroomId,
                ClassroomInternalId = pupil.ClassroomInternalId,
                Place = pupil.Place,
                Grade = pupil.Grade,
                SubGrade = pupil.SubGrade,
                FirstName = pupil.FirstName,
                LastName = pupil.LastName,
                FatherName = pupil.FatherName,
                CertificateType = pupil.CertificateType,
                Certificate = pupil.Certificate,
                Birthday = pupil.Birthday,
                Gender = pupil.Gender,
                Status = pupil.Status,
                CreatedAt = pupil.CreatedAt,
                UpdatedAt = pupil.UpdatedAt
            };
        }

        // === Helper for MD5 generation ===
        private static string ComputePupilMd5(int ktakPupilId, int ktakSchoolId, string classroomId,
            KtakPlace place, EGrade grade, ESubGrade subGrade,
            string firstName, string lastName, string fatherName,
            ECertificateType certificateType, string certificate,
            DateOnly birthday, bool gender, EPupilStatus status)
        {
            var raw = string.Join('|', new[]
            {
                ktakPupilId.ToString(),
                ktakSchoolId.ToString(),
                classroomId ?? string.Empty,
                place.ToString(),
                grade.ToString(),
                subGrade.ToString(),
                firstName ?? string.Empty,
                lastName ?? string.Empty,
                fatherName ?? string.Empty,
                certificateType.ToString(),
                certificate ?? string.Empty,
                birthday.ToString("yyyy-MM-dd"),
                gender ? "1" : "0",
                status.ToString()
            });

            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = md5.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        // === Helper mappers for enums ===
        private static EGrade MapGrade(string? grade)
        {
            if (int.TryParse(grade, out var g) && g >= 1 && g <= 12)
            {
                return (EGrade)g;
            }
            return 0; // default
        }

        private static ESubGrade MapSubGrade(string? classifier)
        {
            if (string.IsNullOrWhiteSpace(classifier))
                return ESubGrade.Unknown;

            // normalize Armenian letters
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

        private static ECertificateType MapCertificateType(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return ECertificateType.Unknown;

            // If API already returns numeric code matching enum values
            if (int.TryParse(code, out var numeric) && Enum.IsDefined(typeof(ECertificateType), numeric))
            {
                return (ECertificateType)numeric;
            }

            // Fallback simple name-based mapping
            var normalized = code.Trim().ToLowerInvariant();
            if (normalized.Contains("birth"))
                return ECertificateType.HHCertificate;
            if (normalized.Contains("passport"))
                return ECertificateType.HHPasport;

            return ECertificateType.Other;
        }

        private static bool MapGender(string? sexCode)
        {
            // Adjust mapping according to real API semantics.
            // For now: treat "47" or "1" as male, everything else as female/false.
            var v = sexCode?.Trim();
            return v == "47" || v == "1" || string.Equals(v, "m", StringComparison.OrdinalIgnoreCase);
        }

        private static EPupilStatus MapPupilStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return EPupilStatus.New;

            var s = status.Trim().ToLowerInvariant();
            return s switch
            {
                "old" => EPupilStatus.Օld,
                "new" => EPupilStatus.New,
                "repeater" => EPupilStatus.Repeater,
                "incomplete" => EPupilStatus.Incomplete,
                "graduated" => EPupilStatus.Graduated,
                _ => EPupilStatus.New
            };
        }

        /// <summary>
        /// Syncs all 10 regions. This method is designed to be called by Hangfire.
        /// </summary>
        public async Task SyncAllRegions()
        {
            var regionIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var results = new List<SyncResult>();
            var syncStartTime = DateTime.UtcNow;

            _logger?.LogInformation($"Starting sync for all {regionIds.Length} regions at {syncStartTime}");

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Syncing region {regionId}...");
                    var result = await SyncRegionInternal(regionId);
                    results.Add(result);
                    
                    if (result.Success)
                    {
                        _logger?.LogInformation($"Region {regionId} synced successfully. " +
                            $"Schools: {result.SchoolsAdded} added, {result.SchoolsUpdated} updated. " +
                            $"Classrooms: {result.ClassroomsAdded} added, {result.ClassroomsUpdated} updated. " +
                            $"Pupils: {result.PupilsAdded} added, {result.PupilsUpdated} updated.");
                    }
                    else
                    {
                        _logger?.LogError($"Region {regionId} sync failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while syncing region {regionId}");
                    results.Add(new SyncResult
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
            var summary = new SyncChangesSummaryDto
            {
                SyncCompletedAt = syncEndTime,
                TotalRegionsProcessed = regionIds.Length,
                SuccessfulRegions = successCount,
                FailedRegions = failedCount,
                TotalSchoolsAdded = results.Sum(r => r.SchoolsAdded),
                TotalSchoolsUpdated = results.Sum(r => r.SchoolsUpdated),
                TotalClassroomsAdded = results.Sum(r => r.ClassroomsAdded),
                TotalClassroomsUpdated = results.Sum(r => r.ClassroomsUpdated),
                TotalPupilsAdded = results.Sum(r => r.PupilsAdded),
                TotalPupilsUpdated = results.Sum(r => r.PupilsUpdated)
            };

            // Aggregate all changed entities
            foreach (var result in results.Where(r => r.Success))
            {
                summary.AllSchoolsUpdated.AddRange(result.SchoolsUpdatedList);
                summary.AllClassroomsUpdated.AddRange(result.ClassroomsUpdatedList);
                summary.AllPupilsUpdated.AddRange(result.PupilsUpdatedList);
            }

            _logger?.LogInformation($"Sync completed for all regions. " +
                $"Success: {successCount}/{regionIds.Length}. " +
                $"Total - Schools: {summary.TotalSchoolsAdded} added, {summary.TotalSchoolsUpdated} updated. " +
                $"Classrooms: {summary.TotalClassroomsAdded} added, {summary.TotalClassroomsUpdated} updated. " +
                $"Pupils: {summary.TotalPupilsAdded} added, {summary.TotalPupilsUpdated} updated.");
        }

        /// <summary>
        /// Internal method that performs the sync operation for a single region
        /// </summary>
        private async Task<SyncResult> SyncRegionInternal(int regionId)
        {
            var result = new SyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                using var client = new HttpClient();
                string url = $"https://crmapi.dshh.am/api/Integration/SendRequest?myUrl=V1/getAllData/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var json = JObject.Parse(responseText);

                // === Step 1: Collect all schools, classrooms, and pupils from JSON ===
                var schoolsToProcess = new List<School>();
                var classroomsToProcess = new List<Classroom>();
                var schoolKtakIds = new HashSet<string>();
                var classroomKeys = new HashSet<string>(); // KtakSchoolId-KtakClassroomId

                // Clear staging table for this region
                await _context.PupilsStaging.ExecuteDeleteAsync();

                foreach (JProperty schoolProp in json.Properties())
                {
                    var school = schoolProp.Value;

                    // Skip if not a JObject
                    if (school.Type != JTokenType.Object)
                        continue;

                    var schoolObj = school as JObject;
                    if (schoolObj == null)
                        continue;

                    // Get school data
                    var schoolsIdToken = schoolObj["schools_id"];
                    if (schoolsIdToken == null || schoolsIdToken.Type == JTokenType.Null)
                        continue;

                    string ktakSchoolId = schoolsIdToken.ToString();
                    schoolKtakIds.Add(ktakSchoolId);

                    // Parse date for timestamps
                    var now = DateTime.UtcNow;

                    // Create school object
                    var schoolToAdd = new School
                    {
                        DshhSchoolId = Guid.NewGuid(),
                        KtakSchoolId = ktakSchoolId,
                        RegionId = regionId,
                        Name = schoolObj["school_name"]?.ToString() ?? "",
                        Marz = schoolObj["marz"]?.ToString() ?? "",
                        Region = schoolObj["region"]?.ToString() ?? "",
                        Community = schoolObj["community"]?.ToString() ?? "",
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    schoolsToProcess.Add(schoolToAdd);

                    // Get classrooms
                    var classroomsToken = schoolObj["classrooms"];
                    if (classroomsToken != null && classroomsToken.Type == JTokenType.Object)
                    {
                        var classroomsObj = classroomsToken as JObject;
                        if (classroomsObj != null)
                        {
                            foreach (JProperty classProp in classroomsObj.Properties())
                            {
                                var cl = classProp.Value;

                                if (cl.Type != JTokenType.Object)
                                    continue;

                                var classObj = cl as JObject;
                                if (classObj == null)
                                    continue;

                                string ktakClassroomId = cl["id"]?.ToString() ?? "";
                                if (string.IsNullOrWhiteSpace(ktakClassroomId))
                                    continue;

                                string classroomKey = $"{ktakSchoolId}-{ktakClassroomId}";
                                classroomKeys.Add(classroomKey);

                                // Create classroom object
                                var classroomToAdd = new Classroom
                                {
                                    Id = Guid.NewGuid(),
                                    KtakSchoolId = ktakSchoolId,
                                    KtakClassroomId = ktakClassroomId,
                                    RegionId = regionId,
                                    Grade = cl["grade"]?.ToString() ?? "",
                                    Classifier = cl["classifier"]?.ToString() ?? "",
                                    ClassName = cl["class"]?.ToString() ?? "",
                                    Stream = cl["stream"]?.ToString(),
                                    SchoolId = schoolToAdd.DshhSchoolId, // Link to school
                                    CreatedAt = now,
                                    UpdatedAt = now
                                };
                                classroomsToProcess.Add(classroomToAdd);

                                // Get students from this classroom
                                var studentsToken = classObj["students"];
                                if (studentsToken != null && studentsToken.Type == JTokenType.Object)
                                {
                                    var studentsObj = studentsToken as JObject;
                                    if (studentsObj != null)
                                    {
                                        foreach (JProperty studentsProp in studentsObj.Properties())
                                        {
                                            var studentsList = studentsProp.Value;

                                            if (studentsList.Type == JTokenType.Array)
                                            {
                                                var studentsArray = studentsList as JArray;
                                                if (studentsArray != null)
                                                {
                                                    foreach (var student in studentsArray)
                                                    {
                                                        if (student.Type != JTokenType.Object)
                                                            continue;

                                                        var pupilIdString = student["id"]?.ToString();
                                                        if (string.IsNullOrWhiteSpace(pupilIdString))
                                                            continue;

                                                        if (!int.TryParse(pupilIdString, out var pupilId))
                                                            continue;

                                                        // Parse school id to int
                                                        int.TryParse(ktakSchoolId, out var ktakSchoolIdInt);

                                                        string identDocNumber = student["ident_document_number"]?.ToString() ?? "";

                                                        // Parse date of birth
                                                        DateOnly birthday = default;
                                                        var dobString = student["date_of_birth"]?.ToString();
                                                        if (!string.IsNullOrWhiteSpace(dobString)
                                                            && DateOnly.TryParse(dobString, out var parsedDate))
                                                        {
                                                            birthday = parsedDate;
                                                        }

                                                        var gradeEnum = MapGrade(cl["grade"]?.ToString());
                                                        var subGradeEnum = MapSubGrade(cl["classifier"]?.ToString());
                                                        var certType = MapCertificateType(student["ident_document"]?.ToString());
                                                        var gender = MapGender(student["sex"]?.ToString());
                                                        var statusEnum = MapPupilStatus(student["status"]?.ToString());

                                                        var firstName = student["first_name"]?.ToString() ?? "";
                                                        var lastName = student["last_name"]?.ToString() ?? "";
                                                        var fatherName = student["father_name"]?.ToString() ?? "";

                                                        // Compute MD5 for this pupil
                                                        var md5 = ComputePupilMd5(
                                                            pupilId,
                                                            ktakSchoolIdInt,
                                                            ktakClassroomId,
                                                            KtakPlace.School,
                                                            gradeEnum,
                                                            subGradeEnum,
                                                            firstName,
                                                            lastName,
                                                            fatherName,
                                                            certType,
                                                            identDocNumber,
                                                            birthday,
                                                            gender,
                                                            statusEnum);

                                                        // Stream directly into staging table
                                                        _context.PupilsStaging.Add(new PupilStaging
                                                        {
                                                            Id = Guid.NewGuid(),
                                                            KtakPupilId = pupilId,
                                                            KtakSchoolId = ktakSchoolIdInt,
                                                            RegionId = regionId,
                                                            ClassroomId = ktakClassroomId,
                                                            ClassroomInternalId = null, // will be set after classrooms are saved
                                                            Place = KtakPlace.School,
                                                            Grade = gradeEnum,
                                                            SubGrade = subGradeEnum,
                                                            FirstName = firstName,
                                                            LastName = lastName,
                                                            FatherName = fatherName,
                                                            CertificateType = certType,
                                                            Certificate = identDocNumber,
                                                            Birthday = birthday,
                                                            Gender = gender,
                                                            Status = statusEnum,
                                                            MD5 = md5,
                                                            CreatedAt = now,
                                                            UpdatedAt = now
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // === Step 2: Process Schools ===
                var existingSchools = await _context.Schools
                    .Where(s => schoolKtakIds.Contains(s.KtakSchoolId) && s.RegionId == regionId)
                    .ToListAsync();

                var existingSchoolsDict = existingSchools.ToDictionary(s => s.KtakSchoolId);
                var schoolMapping = new Dictionary<string, Guid>(); // KtakSchoolId -> DshhSchoolId

                int schoolsAdded = 0;
                int schoolsUpdated = 0;

                foreach (var school in schoolsToProcess)
                {
                    if (existingSchoolsDict.TryGetValue(school.KtakSchoolId, out var existingSchool))
                    {
                        // Overwrite all fields without per-property comparison
                        existingSchool.Name = school.Name;
                        existingSchool.Marz = school.Marz;
                        existingSchool.Region = school.Region;
                        existingSchool.Community = school.Community;
                        existingSchool.UpdatedAt = DateTime.UtcNow;

                        schoolsUpdated++;
                        result.SchoolsUpdatedList.Add(MapToSchoolDto(existingSchool));

                        schoolMapping[school.KtakSchoolId] = existingSchool.DshhSchoolId;
                    }
                    else
                    {
                        // Add new school
                        _context.Schools.Add(school);
                        schoolMapping[school.KtakSchoolId] = school.DshhSchoolId;
                        schoolsAdded++;
                    }
                }

                // Save schools first
                if (schoolsAdded > 0 || schoolsUpdated > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // === Step 3: Process Classrooms ===
                // Update SchoolId references in classrooms after schools are saved
                foreach (var classroom in classroomsToProcess)
                {
                    classroom.SchoolId = schoolMapping[classroom.KtakSchoolId];
                }

                var existingClassrooms = await _context.Classrooms
                    .Where(c => classroomsToProcess.Select(cl => cl.KtakSchoolId).Contains(c.KtakSchoolId) &&
                                classroomsToProcess.Select(cl => cl.KtakClassroomId).Contains(c.KtakClassroomId))
                    .ToListAsync();

                var existingClassroomsDict = existingClassrooms.ToDictionary(c => $"{c.KtakSchoolId}-{c.KtakClassroomId}");
                var classroomMapping = new Dictionary<string, Guid>(); // KtakSchoolId-KtakClassroomId -> Classroom.Id

                int classroomsAdded = 0;
                int classroomsUpdated = 0;

                foreach (var classroom in classroomsToProcess)
                {
                    string classroomKey = $"{classroom.KtakSchoolId}-{classroom.KtakClassroomId}";

                    if (existingClassroomsDict.TryGetValue(classroomKey, out var existingClassroom))
                    {
                        // Overwrite all fields without per-property comparison
                        existingClassroom.RegionId = classroom.RegionId;
                        existingClassroom.Grade = classroom.Grade;
                        existingClassroom.Classifier = classroom.Classifier;
                        existingClassroom.ClassName = classroom.ClassName;
                        existingClassroom.Stream = classroom.Stream;
                        existingClassroom.SchoolId = classroom.SchoolId;
                        existingClassroom.UpdatedAt = DateTime.UtcNow;

                        classroomsUpdated++;
                        result.ClassroomsUpdatedList.Add(MapToClassroomDto(existingClassroom));

                        classroomMapping[classroomKey] = existingClassroom.Id;
                    }
                    else
                    {
                        // Add new classroom
                        _context.Classrooms.Add(classroom);
                        classroomMapping[classroomKey] = classroom.Id;
                        classroomsAdded++;
                    }
                }

                // Save classrooms
                if (classroomsAdded > 0 || classroomsUpdated > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // === Step 4: Process Pupils with MD5 + staging (no in-memory Pupil list) ===
                // Ensure all staged pupils are saved
                await _context.SaveChangesAsync();

                // Load all staged pupils for this sync
                var stagingRows = await _context.PupilsStaging.ToListAsync();

                // Update ClassroomInternalId in staging using classroom mapping
                foreach (var s in stagingRows)
                {
                    string classroomKey = $"{s.KtakSchoolId}-{s.ClassroomId}";
                    if (classroomMapping.TryGetValue(classroomKey, out var classroomId))
                    {
                        s.ClassroomInternalId = classroomId;
                    }
                }

                await _context.SaveChangesAsync();

                // Load real pupils that match staged KtakPupilId values
                var stagedIds = stagingRows.Select(s => s.KtakPupilId).Distinct().ToList();

                var existingPupils = await _context.Pupils
                    .Where(p => stagedIds.Contains(p.KtakPupilId))
                    .ToListAsync();

                var existingDict = existingPupils.ToDictionary(p => p.KtakPupilId);

                var newPupils = new List<Pupil>();
                var pupilsUpdated = 0;

                foreach (var s in stagingRows)
                {
                    if (existingDict.TryGetValue(s.KtakPupilId, out var r))
                    {
                        if (!string.Equals(r.MD5, s.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            // MD5 changed → update real row from staging
                            r.KtakSchoolId = s.KtakSchoolId;
                            r.RegionId = s.RegionId;
                            r.ClassroomId = s.ClassroomId;
                            r.ClassroomInternalId = s.ClassroomInternalId;
                            r.Place = s.Place;
                            r.Grade = s.Grade;
                            r.SubGrade = s.SubGrade;
                            r.FirstName = s.FirstName;
                            r.LastName = s.LastName;
                            r.FatherName = s.FatherName;
                            r.CertificateType = s.CertificateType;
                            r.Certificate = s.Certificate;
                            r.Birthday = s.Birthday;
                            r.Gender = s.Gender;
                            r.Status = s.Status;
                            r.MD5 = s.MD5;
                            r.UpdatedAt = DateTime.UtcNow;

                            pupilsUpdated++;
                            result.PupilsUpdatedList.Add(MapToPupilDto(r));
                        }
                    }
                    else
                    {
                        // New KtakPupilId → insert
                        newPupils.Add(new Pupil
                        {
                            Id = Guid.NewGuid(),
                            KtakPupilId = s.KtakPupilId,
                            KtakSchoolId = s.KtakSchoolId,
                            RegionId = s.RegionId,
                            ClassroomId = s.ClassroomId,
                            ClassroomInternalId = s.ClassroomInternalId,
                            Place = s.Place,
                            Grade = s.Grade,
                            SubGrade = s.SubGrade,
                            FirstName = s.FirstName,
                            LastName = s.LastName,
                            FatherName = s.FatherName,
                            CertificateType = s.CertificateType,
                            Certificate = s.Certificate,
                            Birthday = s.Birthday,
                            Gender = s.Gender,
                            Status = s.Status,
                            MD5 = s.MD5,
                            CreatedAt = s.CreatedAt,
                            UpdatedAt = s.UpdatedAt
                        });
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] Pupil MD5 compare done. New={newPupils.Count}, Updated={pupilsUpdated}. Saving...");

                if (newPupils.Any())
                {
                    _context.Pupils.AddRange(newPupils);
                }

                if (newPupils.Any() || pupilsUpdated > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // Cleanup staging
                await _context.PupilsStaging.ExecuteDeleteAsync();

                // Set result properties
                result.Success = true;
                result.SchoolsProcessed = schoolsToProcess.Count;
                result.SchoolsAdded = schoolsAdded;
                result.SchoolsUpdated = schoolsUpdated;
                result.ClassroomsProcessed = classroomsToProcess.Count;
                result.ClassroomsAdded = classroomsAdded;
                result.ClassroomsUpdated = classroomsUpdated;
                result.PupilsProcessed = stagingRows.Count;
                result.PupilsAdded = newPupils.Count;
                result.PupilsUpdated = pupilsUpdated;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// HTTP endpoint to sync a specific region
        /// </summary>
        [HttpPost("sync/{regionId?}")]
        public async Task<IActionResult> Sync([FromRoute] int regionId = 1)
        {
            var result = await SyncRegionInternal(regionId);
            
            if (result.Success)
            {
                return Ok(new
                {
                    message = "Sync completed successfully!",
                    regionId = result.RegionId,
                    schoolsProcessed = result.SchoolsProcessed,
                    schoolsAdded = result.SchoolsAdded,
                    schoolsUpdated = result.SchoolsUpdated,
                    classroomsProcessed = result.ClassroomsProcessed,
                    classroomsAdded = result.ClassroomsAdded,
                    classroomsUpdated = result.ClassroomsUpdated,
                    pupilsProcessed = result.PupilsProcessed,
                    pupilsAdded = result.PupilsAdded,
                    pupilsUpdated = result.PupilsUpdated
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    error = "Sync failed",
                    message = result.ErrorMessage,
                    regionId = result.RegionId
                });
            }
        }

        /// <summary>
        /// HTTP endpoint to get the latest sync changes JSON
        /// </summary>
        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestSyncChanges()
        {
            try
            {
                var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SyncReports");
                var latestFilePath = Path.Combine(reportsDirectory, "sync-changes-latest.json");

                if (!System.IO.File.Exists(latestFilePath))
                {
                    return NotFound(new { message = "No sync changes file found. Run a sync first." });
                }

                var jsonContent = System.IO.File.ReadAllText(latestFilePath);
                var summary = JsonConvert.DeserializeObject<SyncChangesSummaryDto>(jsonContent);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to retrieve sync changes",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// HTTP endpoint to get all sync change files
        /// </summary>
        [HttpGet("sync-changes/files")]
        public IActionResult GetSyncChangeFiles()
        {
            try
            {
                var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SyncReports");
                
                if (!Directory.Exists(reportsDirectory))
                {
                    return Ok(new { files = new List<string>() });
                }

                var files = Directory.GetFiles(reportsDirectory, "sync-changes-*.json")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Select(f => new
                    {
                        fileName = f.Name,
                        filePath = f.FullName,
                        created = f.CreationTime,
                        size = f.Length
                    })
                    .ToList();

                return Ok(new { files });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to retrieve sync change files",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// HTTP endpoint to get changed entities from the latest sync.
        /// Returns all added and updated schools, classrooms, and pupils as DTOs.
        /// </summary>
        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestChangedEntities()
        {
            try
            {
                var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SyncReports");
                var latestFilePath = Path.Combine(reportsDirectory, "sync-changes-latest.json");

                if (!System.IO.File.Exists(latestFilePath))
                {
                    return NotFound(new { message = "No sync changes file found. Run a sync first." });
                }

                var jsonContent = System.IO.File.ReadAllText(latestFilePath);
                var summary = JsonConvert.DeserializeObject<SyncChangesSummaryDto>(jsonContent);

                if (summary == null)
                {
                    return NotFound(new { message = "Could not parse sync changes file." });
                }

                var changedEntities = new ChangedEntitiesDto
                {
                    SchoolsUpdated = summary.AllSchoolsUpdated,
                    ClassroomsUpdated = summary.AllClassroomsUpdated,
                    PupilsUpdated = summary.AllPupilsUpdated
                };

                return Ok(changedEntities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to retrieve changed entities",
                    message = ex.Message
                });
            }
        }
    }
}