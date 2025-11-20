using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.Models;

namespace PordznakanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SchoolController(AppDbContext context)
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

                var classroomsToAdd = new List<Classroom>();
                var schoolKtakIds = new HashSet<int>();

                // === Pass 1: Collect all KtakSchoolIds and classrooms ===
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

                    int ktakSchoolId = (int)schoolsIdToken;
                    schoolKtakIds.Add(ktakSchoolId);

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

                                classroomsToAdd.Add(new Classroom
                                {
                                    KtakId = cl["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                                    Grade = cl["grade"]?.ToString() ?? "",
                                    Classifier = cl["classifier"]?.ToString() ?? "",
                                    ClassName = cl["class"]?.ToString() ?? "",
                                    Stream = cl["stream"]?.ToString(),
                                    SchoolId = ktakSchoolId, // temporary: will be replaced
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }

                // === Step 1: Ensure all schools exist (only query KtakId and Id) ===
                var schoolMap = await _context.Schools
                    .AsNoTracking()
                    .Where(s => schoolKtakIds.Contains(s.KtakId))
                    .Select(s => new { s.KtakId, s.Id })
                    .ToDictionaryAsync(x => x.KtakId, x => x.Id);

                var missingKtakIds = schoolKtakIds.Except(schoolMap.Keys).ToList();

                if (missingKtakIds.Any())
                {
                    var schoolsToAdd = new List<School>();

                    foreach (JProperty schoolProp in json.Properties())
                    {
                        var school = schoolProp.Value;
                        int ktakId = (int)school["schools_id"];

                        if (missingKtakIds.Contains(ktakId))
                        {
                            schoolsToAdd.Add(new School
                            {
                                KtakId = ktakId,
                                Name = school["name"]?.ToString() ?? $"School {ktakId}",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    _context.Schools.AddRange(schoolsToAdd);
                    await _context.SaveChangesAsync();

                    // Add newly inserted schools to map
                    foreach (var school in schoolsToAdd)
                    {
                        schoolMap[school.KtakId] = school.Id;
                    }
                }

                // === Step 2: Fix SchoolId in classrooms (KtakId → Local DB Id) ===
                foreach (var classroom in classroomsToAdd)
                {
                    if (!schoolMap.TryGetValue(classroom.SchoolId, out int localId))
                    {
                        // Fallback (should not happen)
                        localId = schoolMap.Values.First();
                    }
                    classroom.SchoolId = localId;
                }

                // === Step 3: Get existing classrooms and compare ===
                var ktakIdsToCheck = classroomsToAdd.Select(x => x.KtakId).ToList();
                var existingClassrooms = await _context.Classrooms
                    .Where(c => ktakIdsToCheck.Contains(c.KtakId))
                    .ToListAsync();

                var existingKtakIdsSet = existingClassrooms.Select(c => c.KtakId).ToHashSet();

                // Separate into new and potentially updated classrooms
                var newClassrooms = classroomsToAdd
                    .Where(c => !string.IsNullOrWhiteSpace(c.KtakId) && !existingKtakIdsSet.Contains(c.KtakId))
                    .ToList();

                var classroomsToUpdate = classroomsToAdd
                    .Where(c => !string.IsNullOrWhiteSpace(c.KtakId) && existingKtakIdsSet.Contains(c.KtakId))
                    .ToList();

                // === Step 4: Update existing classrooms if changed ===
                int updatedCount = 0;
                foreach (var updatedClassroom in classroomsToUpdate)
                {
                    var existingClassroom = existingClassrooms.First(c => c.KtakId == updatedClassroom.KtakId);

                    bool hasChanges = false;

                    if (existingClassroom.Grade != updatedClassroom.Grade)
                    {
                        existingClassroom.Grade = updatedClassroom.Grade;
                        hasChanges = true;
                    }

                    if (existingClassroom.Classifier != updatedClassroom.Classifier)
                    {
                        existingClassroom.Classifier = updatedClassroom.Classifier;
                        hasChanges = true;
                    }

                    if (existingClassroom.ClassName != updatedClassroom.ClassName)
                    {
                        existingClassroom.ClassName = updatedClassroom.ClassName;
                        hasChanges = true;
                    }

                    if (existingClassroom.Stream != updatedClassroom.Stream)
                    {
                        existingClassroom.Stream = updatedClassroom.Stream;
                        hasChanges = true;
                    }

                    if (existingClassroom.SchoolId != updatedClassroom.SchoolId)
                    {
                        existingClassroom.SchoolId = updatedClassroom.SchoolId;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingClassroom.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                // === Step 5: Save all changes ===
                if (newClassrooms.Any())
                {
                    _context.Classrooms.AddRange(newClassrooms);
                }

                if (updatedCount > 0 || newClassrooms.Any())
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    message = "Sync completed successfully!",
                    regionId,
                    schoolsProcessed = schoolKtakIds.Count,
                    schoolsAdded = missingKtakIds.Count,
                    classroomsProcessed = classroomsToAdd.Count,
                    classroomsAdded = newClassrooms.Count,
                    classroomsUpdated = updatedCount
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