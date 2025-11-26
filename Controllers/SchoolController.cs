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
                ClassroomId = pupil.ClassroomId,
                ClassroomInternalId = pupil.ClassroomInternalId,
                FirstName = pupil.FirstName,
                LastName = pupil.LastName,
                FatherName = pupil.FatherName,
                IdentDocument = pupil.IdentDocument,
                IdentDocumentNumber = pupil.IdentDocumentNumber,
                FromCountry = pupil.FromCountry,
                SocNumber = pupil.SocNumber,
                DateOfBirth = pupil.DateOfBirth,
                Sex = pupil.Sex,
                Status = pupil.Status,
                CreatedAt = pupil.CreatedAt,
                UpdatedAt = pupil.UpdatedAt
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

            // Generate and save JSON file
            await SaveChangesToJsonFile(summary);

            // Get changed entities for external API
            var changedEntities = GetChangedEntities(results);

            _logger?.LogInformation($"Sync completed for all regions. " +
                $"Success: {successCount}/{regionIds.Length}. " +
                $"Total - Schools: {summary.TotalSchoolsAdded} added, {summary.TotalSchoolsUpdated} updated. " +
                $"Classrooms: {summary.TotalClassroomsAdded} added, {summary.TotalClassroomsUpdated} updated. " +
                $"Pupils: {summary.TotalPupilsAdded} added, {summary.TotalPupilsUpdated} updated. " +
                $"Changes saved to JSON file.");

            // TODO: Send changedEntities to external API here
            // await SendToExternalApi(changedEntities);
        }

        /// <summary>
        /// Returns a list of all changed entities (added and updated) as DTOs.
        /// This function can be used to send data to another API.
        /// </summary>
        public ChangedEntitiesDto GetChangedEntities(List<SyncResult> syncResults)
        {
            var changedEntities = new ChangedEntitiesDto();

            foreach (var result in syncResults.Where(r => r.Success))
            {
                changedEntities.SchoolsUpdated.AddRange(result.SchoolsUpdatedList);
                changedEntities.ClassroomsUpdated.AddRange(result.ClassroomsUpdatedList);
                changedEntities.PupilsUpdated.AddRange(result.PupilsUpdatedList);
            }

            return changedEntities;
        }

        /// <summary>
        /// Saves the sync changes summary to a JSON file
        /// </summary>
        private async Task SaveChangesToJsonFile(SyncChangesSummaryDto summary)
        {
            try
            {
                var json = JsonConvert.SerializeObject(summary, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
                });

                // Create a directory for sync reports if it doesn't exist
                var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SyncReports");
                if (!Directory.Exists(reportsDirectory))
                {
                    Directory.CreateDirectory(reportsDirectory);
                }

                // Save with timestamp in filename
                var fileName = $"sync-changes-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
                var filePath = Path.Combine(reportsDirectory, fileName);

                await System.IO.File.WriteAllTextAsync(filePath, json);

                // Also save as "latest" for easy access
                var latestFilePath = Path.Combine(reportsDirectory, "sync-changes-latest.json");
                await System.IO.File.WriteAllTextAsync(latestFilePath, json);

                _logger?.LogInformation($"Sync changes saved to: {filePath}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save sync changes to JSON file");
            }
        }

        /// <summary>
        /// HTTP endpoint to sync a specific region
        /// </summary>
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
                var pupilsToProcess = new List<Pupil>();
                var schoolKtakIds = new HashSet<string>();
                var classroomKeys = new HashSet<string>(); // KtakSchoolId-KtakClassroomId
                var pupilKeys = new HashSet<string>(); // KtakSchoolId-ClassroomId-IdentDocumentNumber

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

                                                        var pupilId = student["id"]?.ToString();
                                                        if (string.IsNullOrWhiteSpace(pupilId))
                                                            continue;

                                                        string identDocNumber = student["ident_document_number"]?.ToString() ?? "";
                                                        string pupilKey = $"{ktakSchoolId}-{ktakClassroomId}-{identDocNumber}";
                                                        pupilKeys.Add(pupilKey);

                                                        // Parse date of birth
                                                        DateTime? dateOfBirth = null;
                                                        var dobString = student["date_of_birth"]?.ToString();
                                                        if (!string.IsNullOrWhiteSpace(dobString))
                                                        {
                                                            if (DateTime.TryParse(dobString, out DateTime parsedDate))
                                                            {
                                                                dateOfBirth = parsedDate;
                                                            }
                                                        }

                                                        pupilsToProcess.Add(new Pupil
                                                        {
                                                            Id = Guid.NewGuid(),
                                                            KtakPupilId = pupilId,
                                                            KtakSchoolId = ktakSchoolId,
                                                            ClassroomId = ktakClassroomId,
                                                            ClassroomInternalId = null, // Will be set after classrooms are saved
                                                            FirstName = student["first_name"]?.ToString() ?? "",
                                                            LastName = student["last_name"]?.ToString() ?? "",
                                                            FatherName = student["father_name"]?.ToString() ?? "",
                                                            IdentDocument = student["ident_document"]?.ToString() ?? "",
                                                            IdentDocumentNumber = identDocNumber,
                                                            FromCountry = student["from_country"]?.ToString() ?? "",
                                                            SocNumber = student["soc_number"]?.ToString() ?? "",
                                                            DateOfBirth = dateOfBirth,
                                                            Sex = student["sex"]?.ToString() ?? "",
                                                            Status = student["status"]?.ToString() ?? "",
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
                        // Update existing school
                        bool hasChanges = false;

                        if (existingSchool.Name != school.Name)
                        {
                            existingSchool.Name = school.Name;
                            hasChanges = true;
                        }
                        if (existingSchool.Marz != school.Marz)
                        {
                            existingSchool.Marz = school.Marz;
                            hasChanges = true;
                        }
                        if (existingSchool.Region != school.Region)
                        {
                            existingSchool.Region = school.Region;
                            hasChanges = true;
                        }
                        if (existingSchool.Community != school.Community)
                        {
                            existingSchool.Community = school.Community;
                            hasChanges = true;
                        }

                        if (hasChanges)
                        {
                            existingSchool.UpdatedAt = DateTime.UtcNow;
                            schoolsUpdated++;
                            
                            // Track updated school using DTO
                            result.SchoolsUpdatedList.Add(MapToSchoolDto(existingSchool));
                        }

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
                        // Update existing classroom
                        bool hasChanges = false;

                        if (existingClassroom.Grade != classroom.Grade)
                        {
                            existingClassroom.Grade = classroom.Grade;
                            hasChanges = true;
                        }
                        if (existingClassroom.Classifier != classroom.Classifier)
                        {
                            existingClassroom.Classifier = classroom.Classifier;
                            hasChanges = true;
                        }
                        if (existingClassroom.ClassName != classroom.ClassName)
                        {
                            existingClassroom.ClassName = classroom.ClassName;
                            hasChanges = true;
                        }
                        if (existingClassroom.Stream != classroom.Stream)
                        {
                            existingClassroom.Stream = classroom.Stream;
                            hasChanges = true;
                        }
                        if (existingClassroom.SchoolId != classroom.SchoolId)
                        {
                            existingClassroom.SchoolId = classroom.SchoolId;
                            hasChanges = true;
                        }

                        if (hasChanges)
                        {
                            existingClassroom.UpdatedAt = DateTime.UtcNow;
                            classroomsUpdated++;
                            
                            // Track updated classroom using DTO
                            result.ClassroomsUpdatedList.Add(MapToClassroomDto(existingClassroom));
                        }

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

                // === Step 4: Process Pupils ===
                // Update ClassroomInternalId references in pupils after classrooms are saved
                foreach (var pupil in pupilsToProcess)
                {
                    string classroomKey = $"{pupil.KtakSchoolId}-{pupil.ClassroomId}";
                    if (classroomMapping.TryGetValue(classroomKey, out var classroomId))
                    {
                        pupil.ClassroomInternalId = classroomId;
                    }
                }

                var existingPupils = await _context.Pupils
                    .Where(p => pupilsToProcess.Select(pl => pl.KtakSchoolId).Contains(p.KtakSchoolId) &&
                                pupilsToProcess.Select(pl => pl.ClassroomId).Contains(p.ClassroomId) &&
                                pupilsToProcess.Select(pl => pl.IdentDocumentNumber).Contains(p.IdentDocumentNumber))
                    .ToListAsync();

                var existingPupilsDict = existingPupils.ToDictionary(p => $"{p.KtakSchoolId}-{p.ClassroomId}-{p.IdentDocumentNumber}");

                var newPupils = new List<Pupil>();
                int pupilsUpdated = 0;

                foreach (var pupil in pupilsToProcess)
                {
                    string pupilKey = $"{pupil.KtakSchoolId}-{pupil.ClassroomId}-{pupil.IdentDocumentNumber}";

                    if (existingPupilsDict.TryGetValue(pupilKey, out var existingPupil))
                    {
                        // Update existing pupil
                        bool hasChanges = false;

                        if (existingPupil.KtakPupilId != pupil.KtakPupilId)
                        {
                            existingPupil.KtakPupilId = pupil.KtakPupilId;
                            hasChanges = true;
                        }
                        if (existingPupil.ClassroomInternalId != pupil.ClassroomInternalId)
                        {
                            existingPupil.ClassroomInternalId = pupil.ClassroomInternalId;
                            hasChanges = true;
                        }
                        if (existingPupil.FirstName != pupil.FirstName)
                        {
                            existingPupil.FirstName = pupil.FirstName;
                            hasChanges = true;
                        }
                        if (existingPupil.LastName != pupil.LastName)
                        {
                            existingPupil.LastName = pupil.LastName;
                            hasChanges = true;
                        }
                        if (existingPupil.FatherName != pupil.FatherName)
                        {
                            existingPupil.FatherName = pupil.FatherName;
                            hasChanges = true;
                        }
                        if (existingPupil.IdentDocument != pupil.IdentDocument)
                        {
                            existingPupil.IdentDocument = pupil.IdentDocument;
                            hasChanges = true;
                        }
                        if (existingPupil.FromCountry != pupil.FromCountry)
                        {
                            existingPupil.FromCountry = pupil.FromCountry;
                            hasChanges = true;
                        }
                        if (existingPupil.SocNumber != pupil.SocNumber)
                        {
                            existingPupil.SocNumber = pupil.SocNumber;
                            hasChanges = true;
                        }
                        if (existingPupil.DateOfBirth != pupil.DateOfBirth)
                        {
                            existingPupil.DateOfBirth = pupil.DateOfBirth;
                            hasChanges = true;
                        }
                        if (existingPupil.Sex != pupil.Sex)
                        {
                            existingPupil.Sex = pupil.Sex;
                            hasChanges = true;
                        }
                        if (existingPupil.Status != pupil.Status)
                        {
                            existingPupil.Status = pupil.Status;
                            hasChanges = true;
                        }

                        if (hasChanges)
                        {
                            existingPupil.UpdatedAt = DateTime.UtcNow;
                            pupilsUpdated++;
                            
                            // Track updated pupil using DTO
                            result.PupilsUpdatedList.Add(MapToPupilDto(existingPupil));
                        }
                    }
                    else
                    {
                        // Add new pupil
                        newPupils.Add(pupil);
                    }
                }

                // Save pupils
                if (newPupils.Any())
                {
                    _context.Pupils.AddRange(newPupils);
                }

                if (pupilsUpdated > 0 || newPupils.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Track added pupils after save using DTOs
                // Set result properties
                result.Success = true;
                result.SchoolsProcessed = schoolsToProcess.Count;
                result.SchoolsAdded = schoolsAdded;
                result.SchoolsUpdated = schoolsUpdated;
                result.ClassroomsProcessed = classroomsToProcess.Count;
                result.ClassroomsAdded = classroomsAdded;
                result.ClassroomsUpdated = classroomsUpdated;
                result.PupilsProcessed = pupilsToProcess.Count;
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