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
                string url = $"https://api.emis.am/V1/getAllData/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var json = JObject.Parse(responseText);

                var classroomsToAdd = new List<Classroom>();
                var schoolKtakIds = new HashSet<int>();

                // === Pass 1: Collect all KtakSchoolIds and classrooms ===
                foreach (JProperty schoolProp in json.Properties())
                {
                    var school = schoolProp.Value;
                    int ktakSchoolId = (int)school["schools_id"];
                    schoolKtakIds.Add(ktakSchoolId);

                    var classroomsToken = school["classrooms"] as JObject;
                    if (classroomsToken != null)
                    {
                        foreach (JProperty classProp in classroomsToken.Properties())
                        {
                            var cl = classProp.Value;

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
                                // Do NOT set EmployeeId or any navigation prop here → avoids "EmployeeId" error
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

                // === Step 3: Insert only new classrooms (by KtakId) ===
                //var existingKtakIds = await _context.Classrooms
                //    .Where(c => classroomsToAdd.Select(x => x.KtakId).Contains(KtakId))
                //    .Select(c => c.KtakId)
                //    .ToHashSetAsync();

                var newClassrooms = classroomsToAdd
                    .Where(c => !string.IsNullOrWhiteSpace(c.KtakId))
                    .ToList();

                if (newClassrooms.Any())
                {
                    _context.Classrooms.AddRange(newClassrooms);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    message = "Sync completed successfully!",
                    regionId,
                    schoolsProcessed = schoolKtakIds.Count,
                    schoolsAdded = missingKtakIds.Count,
                    classroomsProcessed = classroomsToAdd.Count,
                    classroomsAdded = newClassrooms.Count
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