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

                var pupilsToAdd = new List<Pupil>();
                var pupilKtakIds = new HashSet<string>();

                // === Pass 1: Collect all pupils from all schools and classrooms ===
                foreach (JProperty schoolProp in json.Properties())
                {
                    var school = schoolProp.Value;

                    // Skip if not a JObject
                    if (school.Type != JTokenType.Object)
                        continue;

                    var schoolObj = school as JObject;
                    if (schoolObj == null)
                        continue;

                    // Safely get schools_id
                    var schoolsIdToken = schoolObj["schools_id"];
                    if (schoolsIdToken == null || schoolsIdToken.Type == JTokenType.Null)
                        continue;

                    string ktakSchoolId = schoolsIdToken.ToString();

                    // Safely get classrooms
                    var classroomsToken = schoolObj["classrooms"];
                    if (classroomsToken != null && classroomsToken.Type == JTokenType.Object)
                    {
                        var classroomsObj = classroomsToken as JObject;
                        if (classroomsObj != null)
                        {
                            foreach (JProperty classProp in classroomsObj.Properties())
                            {
                                var cl = classProp.Value;

                                // Skip if not a JObject
                                if (cl.Type != JTokenType.Object)
                                    continue;

                                var classObj = cl as JObject;
                                if (classObj == null)
                                    continue;

                                string classroomId = cl["id"]?.ToString() ?? "";
                                string grade = cl["grade"]?.ToString() ?? "";
                                string classifier = cl["classifier"]?.ToString() ?? "";
                                string className = cl["class"]?.ToString() ?? "";
                                string stream = cl["stream"]?.ToString() ?? "";

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

                                            // Students can be an array
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

                                                        pupilKtakIds.Add(pupilId);

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

                                                        pupilsToAdd.Add(new Pupil
                                                        {
                                                            Id = Guid.NewGuid(),
                                                            KtakSchoolId = ktakSchoolId,
                                                            ClassroomId = classroomId,
                                                            Grade = grade,
                                                            Classifier = classifier,
                                                            Class = className,
                                                            Stream = stream,
                                                            FirstName = student["first_name"]?.ToString() ?? "",
                                                            LastName = student["last_name"]?.ToString() ?? "",
                                                            FatherName = student["father_name"]?.ToString() ?? "",
                                                            IdentDocument = student["ident_document"]?.ToString() ?? "",
                                                            IdentDocumentNumber = student["ident_document_number"]?.ToString() ?? "",
                                                            FromCountry = student["from_country"]?.ToString() ?? "",
                                                            SocNumber = student["soc_number"]?.ToString() ?? "",
                                                            DateOfBirth = dateOfBirth,
                                                            Sex = student["sex"]?.ToString() ?? "",
                                                            Status = student["status"]?.ToString() ?? "",
                                                            CreatedAt = DateTime.UtcNow,
                                                            UpdatedAt = DateTime.UtcNow
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

                // === Step 2: Get existing pupils and compare ===
                var existingPupils = await _context.Pupils
                    .Where(p => pupilKtakIds.Contains(p.KtakSchoolId + "-" + p.ClassroomId + "-" + p.IdentDocumentNumber))
                    .ToListAsync();

                // Create a composite key for comparison (since we don't have a single KtakId for pupils)
                var existingPupilsSet = existingPupils
                    .Select(p => $"{p.KtakSchoolId}-{p.ClassroomId}-{p.IdentDocumentNumber}")
                    .ToHashSet();

                // Separate into new and potentially updated pupils
                var newPupils = pupilsToAdd
                    .Where(p => !existingPupilsSet.Contains($"{p.KtakSchoolId}-{p.ClassroomId}-{p.IdentDocumentNumber}"))
                    .ToList();

                var pupilsToUpdate = pupilsToAdd
                    .Where(p => existingPupilsSet.Contains($"{p.KtakSchoolId}-{p.ClassroomId}-{p.IdentDocumentNumber}"))
                    .ToList();

                // === Step 3: Update existing pupils if changed ===
                int updatedCount = 0;
                foreach (var updatedPupil in pupilsToUpdate)
                {
                    var existingPupil = existingPupils.First(p =>
                        p.KtakSchoolId == updatedPupil.KtakSchoolId &&
                        p.ClassroomId == updatedPupil.ClassroomId &&
                        p.IdentDocumentNumber == updatedPupil.IdentDocumentNumber);

                    bool hasChanges = false;

                    if (existingPupil.Grade != updatedPupil.Grade)
                    {
                        existingPupil.Grade = updatedPupil.Grade;
                        hasChanges = true;
                    }

                    if (existingPupil.Classifier != updatedPupil.Classifier)
                    {
                        existingPupil.Classifier = updatedPupil.Classifier;
                        hasChanges = true;
                    }

                    if (existingPupil.Class != updatedPupil.Class)
                    {
                        existingPupil.Class = updatedPupil.Class;
                        hasChanges = true;
                    }

                    if (existingPupil.Stream != updatedPupil.Stream)
                    {
                        existingPupil.Stream = updatedPupil.Stream;
                        hasChanges = true;
                    }

                    if (existingPupil.FirstName != updatedPupil.FirstName)
                    {
                        existingPupil.FirstName = updatedPupil.FirstName;
                        hasChanges = true;
                    }

                    if (existingPupil.LastName != updatedPupil.LastName)
                    {
                        existingPupil.LastName = updatedPupil.LastName;
                        hasChanges = true;
                    }

                    if (existingPupil.FatherName != updatedPupil.FatherName)
                    {
                        existingPupil.FatherName = updatedPupil.FatherName;
                        hasChanges = true;
                    }

                    if (existingPupil.IdentDocument != updatedPupil.IdentDocument)
                    {
                        existingPupil.IdentDocument = updatedPupil.IdentDocument;
                        hasChanges = true;
                    }

                    if (existingPupil.FromCountry != updatedPupil.FromCountry)
                    {
                        existingPupil.FromCountry = updatedPupil.FromCountry;
                        hasChanges = true;
                    }

                    if (existingPupil.SocNumber != updatedPupil.SocNumber)
                    {
                        existingPupil.SocNumber = updatedPupil.SocNumber;
                        hasChanges = true;
                    }

                    if (existingPupil.DateOfBirth != updatedPupil.DateOfBirth)
                    {
                        existingPupil.DateOfBirth = updatedPupil.DateOfBirth;
                        hasChanges = true;
                    }

                    if (existingPupil.Sex != updatedPupil.Sex)
                    {
                        existingPupil.Sex = updatedPupil.Sex;
                        hasChanges = true;
                    }

                    if (existingPupil.Status != updatedPupil.Status)
                    {
                        existingPupil.Status = updatedPupil.Status;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingPupil.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                // === Step 4: Save all changes ===
                if (newPupils.Any())
                {
                    _context.Pupils.AddRange(newPupils);
                }

                if (updatedCount > 0 || newPupils.Any())
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    message = "Pupil sync completed successfully!",
                    regionId,
                    pupilsProcessed = pupilsToAdd.Count,
                    pupilsAdded = newPupils.Count,
                    pupilsUpdated = updatedCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Pupil sync failed",
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}