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
using System.Net.Http.Json;
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
        public int PupilsDeleted { get; set; }

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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PupilController>? _logger;

        // A sync that would delete more than this share of a region's pupils is treated as a
        // broken EMIS answer (partial or truncated data), not as real removals, and deletes nothing.
        private const double MaxPupilDeleteShare = 0.2;

        // Same idea for inserts: more than this share of a region's existing pupils appearing as
        // "new" in one sync (and at least MinNewPupilsForInsertGuard of them) is treated as a
        // failed existence check, not a real intake, and inserts nothing.
        private const double MaxPupilInsertShare = 0.2;
        private const int MinNewPupilsForInsertGuard = 500;

        public PupilController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PupilController>? logger = null)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
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
                Email = school.Email,
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
                SocNumber = pupil.SocNumber,
                CertificateType = pupil.CertificateType,
                Certificate = pupil.Certificate,
                Birthday = pupil.Birthday,
                Gender = pupil.Gender,
                Status = pupil.Status,
                HasBook = pupil.HasBook,
                CreatedAt = pupil.CreatedAt,
                UpdatedAt = pupil.UpdatedAt
            };
        }

        // === Helper for MD5 generation ===
        private static string ComputePupilMd5(int ktakPupilId, int ktakSchoolId, string classroomId,
            KtakPlace place, EGrade grade, ESubGrade subGrade,
            string firstName, string lastName, string fatherName,
            string socNumber,
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
                socNumber ?? string.Empty,
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
        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

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

        // ident_document codes used by data-api.emis.am. The retired api.emis.am used a
        // completely different dictionary (1017 = birth certificate, 973 = passport, ...),
        // so both are mapped here and neither may be resolved by numeric value: the new
        // codes overlap ECertificateType's own low numbers with different meanings.
        private static readonly Dictionary<string, ECertificateType> DocumentTypeCodes = new()
        {
            // data-api.emis.am (current)
            ["8"] = ECertificateType.HHCertificate,   // ԱԲ214015 – birth certificate
            ["4"] = ECertificateType.HHPasport,       // AX0268329 – HH passport

            // api.emis.am (legacy, kept so a re-point at the old host still maps)
            ["1017"] = ECertificateType.HHCertificate,
            ["973"] = ECertificateType.HHPasport,
            ["974"] = ECertificateType.IDCard,
            ["975"] = ECertificateType.RefugeePassport,
            ["976"] = ECertificateType.StatelessPasport,
            ["1013"] = ECertificateType.BiometricPassport,
            ["1014"] = ECertificateType.ForeignDocument,
            ["1015"] = ECertificateType.ConventionCard,
            ["1016"] = ECertificateType.ResidenceCard,
            ["1018"] = ECertificateType.Other,
            ["1061"] = ECertificateType.TravelDocument
        };

        private static ECertificateType MapCertificateType(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return ECertificateType.Unknown;

            var normalized = code.Trim();

            if (DocumentTypeCodes.TryGetValue(normalized, out var mapped))
                return mapped;

            // Fallback simple name-based mapping
            var lowered = normalized.ToLowerInvariant();
            if (lowered.Contains("birth"))
                return ECertificateType.HHCertificate;
            if (lowered.Contains("passport"))
                return ECertificateType.HHPasport;

            // Codes 5, 6, 7, 9 and 10 (~1.5% of pupils) are not confirmed with EMIS yet;
            // SyncRegionInternal logs every unmapped code it sees so they can be added here.
            return ECertificateType.Other;
        }

        private static bool IsKnownDocumentCode(string? code) =>
            !string.IsNullOrWhiteSpace(code) && DocumentTypeCodes.ContainsKey(code.Trim());

        private static bool MapGender(string? sexCode)
        {
            // data-api.emis.am: 3 = male, 4 = female.
            // api.emis.am (legacy): 48 = male, 47 = female — the previous mapping had
            // these two the wrong way round, which stored every gender inverted.
            var v = sexCode?.Trim();

            return v switch
            {
                "3" or "48" or "1" => true,
                "4" or "47" or "2" => false,
                _ => string.Equals(v, "m", StringComparison.OrdinalIgnoreCase)
            };
        }

        private static bool IsKnownSexCode(string? code)
        {
            var v = code?.Trim();
            return v is "3" or "4" or "48" or "47" or "1" or "2";
        }

        // Schools known to come back missing/incomplete from the region-wide
        // getAllData/{regionId} response and that EMIS instead serves individually via
        // getAllData/{regionId}/{schoolId}. Fetched separately per region and merged in.
        private static readonly (int RegionId, int SchoolId)[] SpecialCaseSchools =
        {
            (2, 364),
            (3, 469),
            (3, 486),
            (3,504),
            (3, 507),
            (3, 514),
            (3, 517),
            (3, 518),
            (3, 1315),
            (3, 1316),
            (3, 1317),
            (3, 1318),
            (3, 1319),
            (3, 1320),
            (6, 724),
            (10, 1114),
            (10, 1117),
            (10, 1118),
            (10, 1121),
            (10, 1122),
            (10, 1123),
            (10, 1125),
            (10, 1126),
            (10, 1127),
            (10, 1128),
            (10, 1132),
            (10, 1133),
            (10, 1135),
            (10, 1136),
            (11, 1164),
            (11, 1165),
            (11, 1168),
            (11, 1173),
            (11, 1174),
            (11, 1177),
            (11, 1179),
            (11, 1182),
            (11, 1195),
            (11, 1199),
            (11, 1212),
            (11, 1213),
            (11, 1214),
            (11, 1216),
            (11, 1217),
            (11, 1218),
            (11, 1219),
            (11, 1220),
            (11, 1223)
        };

        private static EPupilStatus MapPupilStatus(string? status)
        {
            // data-api.emis.am returns only currently enrolled pupils and leaves status
            // null for them, so null keeps meaning New — that is what "active pupil"
            // endpoints such as RegionData 1.4 filter on.
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
                "hayt_admission" => EPupilStatus.HaytAdmission,
                _ => EPupilStatus.New
            };
        }

        /// <summary>
        /// Syncs all 10 regions. This method is designed to be called by Hangfire.
        /// </summary>
        [NonAction]
        public async Task SyncAllRegions()
        {
            using var guard = SyncGuard.TryAcquire(nameof(PupilController));
            if (guard is null)
            {
                _logger?.LogWarning("Pupil sync is already running; this run was skipped.");
                return;
            }

            var regionIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
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
                            $"Pupils: {result.PupilsAdded} added, {result.PupilsUpdated} updated, {result.PupilsDeleted} deleted.");
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
                $"Pupils: {summary.TotalPupilsAdded} added, {summary.TotalPupilsUpdated} updated, {results.Sum(r => r.PupilsDeleted)} deleted.");

            await NotifyCrmSyncCompleted(summary);
        }

        /// <summary>
        /// Tells the CRM the nightly pupils sync has finished. Best-effort: a failure here
        /// must not fail the Hangfire job, so it's logged and swallowed.
        /// </summary>
        private async Task NotifyCrmSyncCompleted(SyncChangesSummaryDto summary)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ktakapi");

                var outgoingKey = _configuration["Integration:OutgoingApiKey"];
                client.DefaultRequestHeaders.Remove("X-API-KEY");
                client.DefaultRequestHeaders.Add("X-API-KEY", outgoingKey);

                var response = await client.PostAsJsonAsync("sync-pupils", new
                {
                    status = "completed",
                    completedAt = summary.SyncCompletedAt,
                    successfulRegions = summary.SuccessfulRegions,
                    failedRegions = summary.TotalRegionsProcessed - summary.SuccessfulRegions
                });

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger?.LogError($"CRM sync-pupils notification returned {(int)response.StatusCode}: {body}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to notify CRM that the pupils sync completed");
            }
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
                // Dedicated getAllData client: base address (https://data-api.emis.am/v1/),
                // the required X-Api-Token header, gzip and retries. Scoped to this endpoint
                // only — see Program.cs / appsettings.json.
                var client = _httpClientFactory.CreateClient(EmisClients.GetAllData);
                var fetchStarted = DateTime.UtcNow;

                // EMIS paginates getAllData/{regionId}?page=N (page / per_page / total / last_page).
                // Read every page and merge the schools into one object; a page that fails after the
                // retries throws, so a region is never synced from only part of its schools.
                var regionPages = await EmisPaging.FetchAllPagesAsync(
                    client, $"getAllData/{regionId}", _logger, $"[Region {regionId}]");
                var json = regionPages.Data;

                _logger?.LogInformation(
                    $"[Region {regionId}] Fetched getAllData: {regionPages.PagesFetched} page(s), " +
                    $"{json.Count} school(s)" +
                    (regionPages.Total is null ? "" : $" of {regionPages.Total}") +
                    $" in {(DateTime.UtcNow - fetchStarted).TotalSeconds:0.0}s.");

                if (!regionPages.Complete)
                {
                    _logger?.LogWarning(
                        $"[Region {regionId}] Received {json.Count} schools but EMIS reports total={regionPages.Total}. " +
                        "Treating the region as incomplete: pupils missing from it will NOT be deleted.");
                }

                // Merge in special-case schools for this region — each fetched individually
                // by schools_id (same response shape, just one school) and merged into the
                // region JSON so Step 1 below picks them up like any other school.
                // Set when a special-case school could not be fetched: its pupils are then missing
                // from this sync, so they must not be mistaken for pupils EMIS removed.
                var regionDataIncomplete = !regionPages.Complete;

                foreach (var special in SpecialCaseSchools.Where(s => s.RegionId == regionId))
                {
                    JObject specialJson;
                    try
                    {
                        // Same reader: copes with a single-page answer and with a paginated one.
                        var specialPages = await EmisPaging.FetchAllPagesAsync(
                            client,
                            $"getAllData/{special.RegionId}/{special.SchoolId}",
                            _logger,
                            $"[Region {regionId}] [School {special.SchoolId}]");
                        specialJson = specialPages.Data;
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger?.LogWarning(
                            $"[Region {regionId}] Special-case school {special.SchoolId} fetch failed: {ex.Message}");
                        regionDataIncomplete = true;
                        continue;
                    }

                    // Top-level keys are school keys. Replace each key outright: JObject.Merge would
                    // recurse into a school already present in the region response and CONCATENATE
                    // its students arrays, so every pupil of that school was stored twice.
                    foreach (var specialProp in specialJson.Properties())
                    {
                        json[specialProp.Name] = specialProp.Value.DeepClone();
                    }

                    _logger?.LogInformation(
                        $"[Region {regionId}] Merged special-case school {special.SchoolId} into region data.");
                }

                // === Step 1: Collect all schools, classrooms, and pupils from JSON ===
                var schoolsToProcess = new List<School>();
                var classroomsToProcess = new List<Classroom>();
                var schoolKtakIds = new HashSet<int>();
                var classroomKeys = new HashSet<string>(); // KtakSchoolId-KtakClassroomId

                // Reference codes EMIS sent that we have no mapping for; logged once per region
                // so the dictionaries above can be completed instead of silently defaulting.
                var unmappedDocumentCodes = new HashSet<string>();
                var unmappedSexCodes = new HashSet<string>();

                // Clear staging rows for this region only (PupilsStaging is shared by all regions)
                await _context.PupilsStaging.Where(s => s.RegionId == regionId).ExecuteDeleteAsync();

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

                    if (!int.TryParse(schoolsIdToken.ToString(), out var ktakSchoolId))
                        continue;
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
                        Email = schoolObj["school_email"]?.ToString() ?? "",
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

                                                        string identDocNumber = student["ident_document_number"]?.ToString() ?? "";
                                                        string socNumber = student["soc_number"]?.ToString() ?? "";

                                                        // Parse date of birth
                                                        DateOnly birthday = default;
                                                        var dobString = student["date_of_birth"]?.ToString();
                                                        if (!string.IsNullOrWhiteSpace(dobString)
                                                            && DateOnly.TryParse(dobString, out var parsedDate))
                                                        {
                                                            birthday = parsedDate;
                                                        }

                                                        var documentCode = student["ident_document"]?.ToString();
                                                        var sexCode = student["sex"]?.ToString();

                                                        if (!string.IsNullOrWhiteSpace(documentCode) && !IsKnownDocumentCode(documentCode))
                                                            unmappedDocumentCodes.Add(documentCode.Trim());

                                                        if (!IsKnownSexCode(sexCode))
                                                            unmappedSexCodes.Add(string.IsNullOrWhiteSpace(sexCode) ? "(null)" : sexCode.Trim());

                                                        var gradeEnum = MapGrade(cl["grade"]?.ToString());
                                                        var subGradeEnum = MapSubGrade(cl["classifier"]?.ToString());
                                                        var certType = MapCertificateType(documentCode);
                                                        var gender = MapGender(sexCode);
                                                        var statusEnum = MapPupilStatus(student["status"]?.ToString());

                                                        var firstName = student["first_name"]?.ToString() ?? "";
                                                        var lastName = student["last_name"]?.ToString() ?? "";
                                                        var fatherName = student["father_name"]?.ToString() ?? "";

                                                        // Compute MD5 for this pupil
                                                        var md5 = ComputePupilMd5(
                                                            pupilId,
                                                            ktakSchoolId,
                                                            ktakClassroomId,
                                                            KtakPlace.School,
                                                            gradeEnum,
                                                            subGradeEnum,
                                                            firstName,
                                                            lastName,
                                                            fatherName,
                                                            socNumber,
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
                                                            KtakSchoolId = ktakSchoolId,
                                                            RegionId = regionId,
                                                            ClassroomId = ktakClassroomId,
                                                            ClassroomInternalId = null, // will be set after classrooms are saved
                                                            Place = KtakPlace.School,
                                                            Grade = gradeEnum,
                                                            SubGrade = subGradeEnum,
                                                            FirstName = firstName,
                                                            LastName = lastName,
                                                            FatherName = fatherName,
                                                            SocNumber = socNumber,
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

                if (unmappedDocumentCodes.Count > 0)
                {
                    _logger?.LogWarning($"[Region {regionId}] Unmapped ident_document codes stored as Other: " +
                        string.Join(", ", unmappedDocumentCodes.OrderBy(c => c)));
                }

                if (unmappedSexCodes.Count > 0)
                {
                    _logger?.LogWarning($"[Region {regionId}] Unmapped sex codes stored as female: " +
                        string.Join(", ", unmappedSexCodes.OrderBy(c => c)));
                }

                // === Step 2: Process Schools ===
                var existingSchools = await _context.Schools
                    .Where(s => schoolKtakIds.Contains(s.KtakSchoolId) && s.RegionId == regionId)
                    .ToListAsync();

                var existingSchoolsDict = existingSchools.ToDictionary(s => s.KtakSchoolId);
                var schoolMapping = new Dictionary<int, Guid>(); // KtakSchoolId -> DshhSchoolId

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
                        existingSchool.Email = school.Email;
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
                var stagingRows = await _context.PupilsStaging
                    .Where(s => s.RegionId == regionId)
                    .ToListAsync();

                // The same KtakPupilId must never produce two Pupils rows. If EMIS (or a merge)
                // delivered a pupil twice, keep one staging row per KtakPupilId.
                var duplicateStaging = stagingRows
                    .GroupBy(s => s.KtakPupilId)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (duplicateStaging.Count > 0)
                {
                    _logger?.LogWarning(
                        $"[Region {regionId}] {duplicateStaging.Count} KtakPupilId(s) appear more than once in the EMIS data; keeping one row each.");

                    var surplus = duplicateStaging
                        .SelectMany(g => g.OrderByDescending(x => x.UpdatedAt).Skip(1))
                        .ToList();
                    _context.PupilsStaging.RemoveRange(surplus);
                    var surplusIds = surplus.Select(x => x.Id).ToHashSet();
                    stagingRows = stagingRows.Where(s => !surplusIds.Contains(s.Id)).ToList();
                }

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

                // Load real pupils that match staged KtakPupilId values. Deliberately NOT filtered by
                // RegionId: KtakPupilId is unique, so a pupil who moved to this region is the same
                // row (the update below sets RegionId), and must not be inserted a second time.
                var stagedIds = stagingRows.Select(s => s.KtakPupilId).Distinct().ToList();

                var existingPupils = await _context.Pupils
                    .Where(p => stagedIds.Contains(p.KtakPupilId))
                    .ToListAsync();

                // Pupils has no unique constraint on KtakPupilId, so duplicate rows can exist
                // from before this sync logic was in place. ToDictionary would throw on the
                // first duplicate and abort the whole region, so keep the most recently
                // updated row per KtakPupilId and log the rest for cleanup instead.
                var duplicateGroups = existingPupils
                    .GroupBy(p => p.KtakPupilId)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (duplicateGroups.Count > 0)
                {
                    _logger?.LogWarning(
                        $"[Region {regionId}] Found {duplicateGroups.Count} KtakPupilId(s) with duplicate rows in Pupils: " +
                        string.Join(", ", duplicateGroups.Select(g => g.Key)));
                }

                var existingDict = existingPupils
                    .GroupBy(p => p.KtakPupilId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.UpdatedAt).First());

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
                            r.SocNumber = s.SocNumber;
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
                            SocNumber = s.SocNumber,
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

                // Guard against mass duplication: a sync that wants to insert a large share of the
                // pupils we already hold is far more likely a failed existence check than a real
                // intake, so insert nothing (mirrors MaxPupilDeleteShare for deletions).
                if (existingDict.Count > 0
                    && newPupils.Count >= MinNewPupilsForInsertGuard
                    && newPupils.Count > existingDict.Count * MaxPupilInsertShare)
                {
                    _logger?.LogError(
                        $"[Region {regionId}] {newPupils.Count} new pupils vs {existingDict.Count} existing " +
                        $"(more than {MaxPupilInsertShare:P0}); nothing inserted or updated, existence check looks broken.");

                    _context.ChangeTracker.Clear();
                    await _context.PupilsStaging.Where(s => s.RegionId == regionId).ExecuteDeleteAsync();

                    result.Success = false;
                    result.ErrorMessage = "Too many new pupils compared to existing ones; aborted to avoid mass duplication.";
                    return result;
                }

                if (newPupils.Any())
                {
                    _context.Pupils.AddRange(newPupils);
                }

                if (newPupils.Any() || pupilsUpdated > 0)
                {
                    await _context.SaveChangesAsync();
                }

                // === Step 5: Delete pupils that EMIS no longer returns for this region ===
                var pupilsDeleted = 0;

                if (regionDataIncomplete)
                {
                    _logger?.LogWarning($"[Region {regionId}] A special-case school failed to load; skipping deletion of missing pupils.");
                }
                else if (stagedIds.Count == 0)
                {
                    _logger?.LogWarning($"[Region {regionId}] EMIS returned no pupils; skipping deletion of missing pupils.");
                }
                else
                {
                    var stagedIdSet = stagedIds.ToHashSet();
                    var regionRows = await _context.Pupils.AsNoTracking()
                        .Where(p => p.RegionId == regionId && p.Place == KtakPlace.School)
                        .Select(p => new { p.Id, p.KtakPupilId })
                        .ToListAsync();

                    var staleIds = regionRows
                        .Where(p => !stagedIdSet.Contains(p.KtakPupilId))
                        .Select(p => p.Id)
                        .ToList();

                    if (staleIds.Count > regionRows.Count * MaxPupilDeleteShare)
                    {
                        _logger?.LogError(
                            $"[Region {regionId}] {staleIds.Count} of {regionRows.Count} pupils are missing from EMIS " +
                            $"(more than {MaxPupilDeleteShare:P0}); nothing deleted, EMIS data looks incomplete.");
                    }
                    else
                    {
                        foreach (var chunk in staleIds.Chunk(1000))
                        {
                            pupilsDeleted += await _context.Pupils
                                .Where(p => chunk.Contains(p.Id))
                                .ExecuteDeleteAsync();
                        }

                        _logger?.LogInformation($"[Region {regionId}] Deleted {pupilsDeleted} pupils that EMIS no longer returns.");
                    }
                }

                // Cleanup staging (this region only)
                await _context.PupilsStaging.Where(s => s.RegionId == regionId).ExecuteDeleteAsync();

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
                result.PupilsDeleted = pupilsDeleted;

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
            using var guard = SyncGuard.TryAcquire(nameof(PupilController));
            if (guard is null)
                return Conflict(new { error = "Pupil sync is already running. Try again when it finishes." });

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
                    pupilsUpdated = result.PupilsUpdated,
                    pupilsDeleted = result.PupilsDeleted
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