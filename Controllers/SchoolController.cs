using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.Models;

namespace PordznakanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PupilController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PupilController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("sync/{regionId?}")]
        public async Task<IActionResult> Sync([FromRoute] int regionId = 1)
        {
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
                                                            Grade = classroomToAdd.Grade,
                                                            Classifier = classroomToAdd.Classifier,
                                                            Class = classroomToAdd.ClassName,
                                                            Stream = classroomToAdd.Stream,
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
                        if (existingPupil.Grade != pupil.Grade)
                        {
                            existingPupil.Grade = pupil.Grade;
                            hasChanges = true;
                        }
                        if (existingPupil.Classifier != pupil.Classifier)
                        {
                            existingPupil.Classifier = pupil.Classifier;
                            hasChanges = true;
                        }
                        if (existingPupil.Class != pupil.Class)
                        {
                            existingPupil.Class = pupil.Class;
                            hasChanges = true;
                        }
                        if (existingPupil.Stream != pupil.Stream)
                        {
                            existingPupil.Stream = pupil.Stream;
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

                return Ok(new
                {
                    message = "Sync completed successfully!",
                    regionId,
                    schoolsProcessed = schoolsToProcess.Count,
                    schoolsAdded,
                    schoolsUpdated,
                    classroomsProcessed = classroomsToProcess.Count,
                    classroomsAdded,
                    classroomsUpdated,
                    pupilsProcessed = pupilsToProcess.Count,
                    pupilsAdded = newPupils.Count,
                    pupilsUpdated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Sync failed",
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}