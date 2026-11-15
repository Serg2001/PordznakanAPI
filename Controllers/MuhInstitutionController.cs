using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    public class MuhInstitutionSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int InstitutionsProcessed { get; set; }
        public int InstitutionsAdded { get; set; }
        public int InstitutionsUpdated { get; set; }
        public List<MuhInstitutionDto> InstitutionsUpdatedList { get; set; } = new();
    }

    public class MuhInstitutionChangedEntitiesDto
    {
        public List<MuhInstitutionDto> InstitutionsUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MuhInstitutionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISyncReportService _syncReportService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MuhInstitutionController>? _logger;

        public MuhInstitutionController(
            AppDbContext context,
            ISyncReportService syncReportService,
            IHttpClientFactory httpClientFactory,
            ILogger<MuhInstitutionController>? logger = null)
        {
            _context = context;
            _syncReportService = syncReportService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private static MuhInstitutionDto MapToDto(MuhInstitution institution)
        {
            return new MuhInstitutionDto
            {
                Id = institution.Id,
                InstitutionId = institution.InstitutionId,
                RegionId = institution.RegionId,
                Name = institution.Name,
                Email = institution.Email,
                InstitutionType = institution.InstitutionType,
                InstitutionTypeName = institution.InstitutionTypeName,
                Marz = institution.Marz,
                Region = institution.Region,
                Community = institution.Community,
                Address = institution.Address,
                CreatedAt = institution.CreatedAt,
                UpdatedAt = institution.UpdatedAt
            };
        }

        /// <summary>
        /// Manually maintained (regionId, subId) pairs. Institutions in these areas are not returned by
        /// colleges/getCollegesGeography/{regionId}, so each one is fetched from
        /// colleges/getCollegesGeography/{regionId}/{subId} during SyncAllRegions.
        /// </summary>
        internal static readonly (int RegionId, int SubId)[] ExtraGeographyLinks =
        {
            (5, 1429), (5, 1427), (5, 1428), (5, 1434), (5, 1436), (5, 1438), (5, 1443), (5, 1444),
            (5, 1445), (5, 1446), (5, 1432), (5, 1435), (5, 1451), (5, 1529), (5, 1537),
            (7, 1509), (7, 1510), (7, 1512), (7, 1514), (7, 1515), (7, 1508), (7, 1513),
            (3, 1505), (3, 1506), (3, 1507),
            (2, 1517), (2, 1518), (2, 1519), (2, 1520),
            (1, 1524), (1, 1525),
            (8, 1462), (8, 1463), (8, 1464), (8, 1465), (8, 1466), (8, 1467), (8, 1468), (8, 1469),
            (8, 1470), (8, 1473),
            (6, 1479), (6, 1474), (6, 1483), (6, 1476), (6, 1480), (6, 1481),
            (4, 1485), (4, 1486), (4, 1487), (4, 1489),
            (11, 1500), (11, 1501), (11, 1502), (11, 1503),
            (9, 1491), (9, 1492), (9, 1495), (9, 1496), (9, 1497),
            (10, 1521)
        };

        [NonAction]
        public async Task SyncAllRegions()
        {
            using var guard = SyncGuard.TryAcquire(nameof(MuhInstitutionController));
            if (guard is null)
            {
                _logger?.LogWarning("MuhInstitution sync is already running; this run was skipped.");
                return;
            }

            var targets = Enumerable.Range(1, 11)
                .Select(r => (RegionId: r, SubId: (int?)null))
                .Concat(ExtraGeographyLinks.Select(l => (l.RegionId, SubId: (int?)l.SubId)))
                .ToArray();
            var results = new List<MuhInstitutionSyncResult>();

            foreach (var (regionId, subId) in targets)
            {
                var label = subId.HasValue ? $"{regionId}/{subId}" : regionId.ToString();
                try
                {
                    _logger?.LogInformation($"Syncing MuhInstitution region {label}...");
                    var result = await SyncRegionInternal(regionId, subId);
                    results.Add(result);

                    if (result.Success)
                    {
                        _logger?.LogInformation($"Region {label} synced successfully. " +
                            $"Institutions: {result.InstitutionsAdded} added, {result.InstitutionsUpdated} updated.");
                    }
                    else
                    {
                        _logger?.LogError($"Region {label} sync failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while syncing MuhInstitution region {label}");
                    results.Add(new MuhInstitutionSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new MuhInstitutionSyncSummaryDto
            {
                SyncCompletedAt = DateTime.UtcNow,
                TotalRegionsProcessed = targets.Length,
                SuccessfulRegions = results.Count(r => r.Success),
                FailedRegions = results.Count(r => !r.Success),
                TotalInstitutionsAdded = results.Sum(r => r.InstitutionsAdded),
                TotalInstitutionsUpdated = results.Sum(r => r.InstitutionsUpdated)
            };

            foreach (var result in results.Where(r => r.Success))
            {
                summary.AllInstitutionsUpdated.AddRange(result.InstitutionsUpdatedList);
            }

            _logger?.LogInformation($"MuhInstitution sync completed for all regions. " +
                $"Success: {summary.SuccessfulRegions}/{targets.Length}. " +
                $"Total - Institutions: {summary.TotalInstitutionsAdded} added, {summary.TotalInstitutionsUpdated} updated.");
        }

        /// <summary>
        /// Parses the getCollegesGeography response: a JSON object keyed by institution_id
        /// ("1458": { "institution_id": 1458, "name": ..., ... }). The key is used when the
        /// inner institution_id is missing. An empty region may come back as [] or {}.
        /// </summary>
        private static List<MuhInstitutionStaging> ParseInstitutions(JObject dict, int regionId, DateTime now)
        {
            var rows = new List<MuhInstitutionStaging>();

            foreach (var property in dict.Properties())
            {
                if (property.Value is not JObject obj)
                    continue;

                var idText = obj["institution_id"]?.ToString();
                if (!int.TryParse(idText, out var institutionId) && !int.TryParse(property.Name, out institutionId))
                    continue;

                int.TryParse(obj["institution_type"]?.ToString(), out var institutionType);

                var name = obj["name"]?.ToString() ?? string.Empty;
                var email = obj["email"]?.ToString() ?? string.Empty;
                var institutionTypeName = obj["institution_type_name"]?.ToString() ?? string.Empty;
                var marz = obj["marz"]?.ToString() ?? string.Empty;
                var region = obj["region"]?.ToString() ?? string.Empty;
                var community = obj["community"]?.ToString() ?? string.Empty;
                var address = obj["address"]?.ToString() ?? string.Empty;

                var md5 = SyncHelpers.ComputeMd5(
                    institutionId.ToString(),
                    name,
                    email,
                    institutionType.ToString(),
                    institutionTypeName,
                    marz,
                    region,
                    community,
                    address);

                rows.Add(new MuhInstitutionStaging
                {
                    Id = Guid.NewGuid(),
                    InstitutionId = institutionId,
                    RegionId = regionId,
                    Name = name,
                    Email = email,
                    InstitutionType = institutionType,
                    InstitutionTypeName = institutionTypeName,
                    Marz = marz,
                    Region = region,
                    Community = community,
                    Address = address,
                    MD5 = md5,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            return rows;
        }

        private async Task<MuhInstitutionSyncResult> SyncRegionInternal(int regionId, int? subId = null)
        {
            var result = new MuhInstitutionSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                // Shared EMIS client: base URL, X-Api-Token and retry handler come from Program.cs.
                var client = _httpClientFactory.CreateClient(EmisClients.GetAllData);
                var path = subId.HasValue
                    ? $"colleges/getCollegesGeography/{regionId}/{subId}"
                    : $"colleges/getCollegesGeography/{regionId}";

                // EMIS paginates colleges/getCollegesGeography/...?page=N; read every page and merge the institutions.
                var pages = await EmisPaging.FetchAllPagesAsync(client, path, _logger, $"[MuhInstitution][Region {regionId}]");
                if (!pages.Complete)
                {
                    _logger?.LogWarning(
                        $"[MuhInstitution][Region {regionId}] Received {pages.Data.Count} institutions but EMIS reports total={pages.Total}.");
                }

                var now = DateTime.UtcNow;
                var parsed = ParseInstitutions(pages.Data, regionId, now);

                // Clear staging table for this sync
                await _context.MuhInstitutionsStaging.ExecuteDeleteAsync();

                // The same institution_id can only appear once per sync
                _context.MuhInstitutionsStaging.AddRange(
                    parsed.GroupBy(p => p.InstitutionId).Select(g => g.First()));

                // Save all staging data
                await _context.SaveChangesAsync();

                // Load all staged institutions
                var stagingRows = await _context.MuhInstitutionsStaging.ToListAsync();

                // Get existing institutions by InstitutionId
                var stagedIds = stagingRows.Select(s => s.InstitutionId).Distinct().ToList();
                var existingInstitutions = await _context.MuhInstitutions
                    .Where(i => stagedIds.Contains(i.InstitutionId))
                    .ToListAsync();

                var existingDict = existingInstitutions.ToDictionary(i => i.InstitutionId);

                var newInstitutions = new List<MuhInstitution>();
                var updatedCount = 0;

                // Process each staged institution
                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.InstitutionId, out var existing))
                    {
                        // Compare MD5
                        if (!string.Equals(existing.MD5, staging.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            existing.RegionId = staging.RegionId;
                            existing.Name = staging.Name;
                            existing.Email = staging.Email;
                            existing.InstitutionType = staging.InstitutionType;
                            existing.InstitutionTypeName = staging.InstitutionTypeName;
                            existing.Marz = staging.Marz;
                            existing.Region = staging.Region;
                            existing.Community = staging.Community;
                            existing.Address = staging.Address;
                            existing.MD5 = staging.MD5;
                            existing.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                            result.InstitutionsUpdatedList.Add(MapToDto(existing));
                        }
                    }
                    else
                    {
                        newInstitutions.Add(new MuhInstitution
                        {
                            Id = Guid.NewGuid(),
                            InstitutionId = staging.InstitutionId,
                            RegionId = staging.RegionId,
                            Name = staging.Name,
                            Email = staging.Email,
                            InstitutionType = staging.InstitutionType,
                            InstitutionTypeName = staging.InstitutionTypeName,
                            Marz = staging.Marz,
                            Region = staging.Region,
                            Community = staging.Community,
                            Address = staging.Address,
                            MD5 = staging.MD5,
                            CreatedAt = staging.CreatedAt,
                            UpdatedAt = staging.UpdatedAt
                        });
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] MuhInstitution MD5 compare done. New={newInstitutions.Count}, Updated={updatedCount}. Saving...");

                if (newInstitutions.Any())
                {
                    _context.MuhInstitutions.AddRange(newInstitutions);
                }

                if (updatedCount > 0 || newInstitutions.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Cleanup staging
                await _context.MuhInstitutionsStaging.ExecuteDeleteAsync();

                result.Success = true;
                result.InstitutionsProcessed = stagingRows.Count;
                result.InstitutionsAdded = newInstitutions.Count;
                result.InstitutionsUpdated = updatedCount;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger?.LogError(ex, $"Error syncing MuhInstitution for region {regionId}");
                return result;
            }
        }

        [HttpPost("sync/{regionId?}")]
        public async Task<IActionResult> SyncInstitutions([FromRoute] int regionId = 1)
        {
            using var guard = SyncGuard.TryAcquire(nameof(MuhInstitutionController));
            if (guard is null)
                return Conflict(new { error = "MuhInstitution sync is already running. Try again when it finishes." });

            var result = await SyncRegionInternal(regionId);

            if (result.Success)
            {
                return Ok(new
                {
                    message = "MuhInstitution sync completed successfully!",
                    regionId = result.RegionId,
                    institutionsProcessed = result.InstitutionsProcessed,
                    institutionsAdded = result.InstitutionsAdded,
                    institutionsUpdated = result.InstitutionsUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "MuhInstitution sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestSync()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MuhInstitutionSyncSummaryDto>(
                    "MuhInstitutionSyncReports", "muh-institution-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MuhInstitution sync file found. Run a sync first." });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MuhInstitution sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestChanges()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MuhInstitutionSyncSummaryDto>(
                    "MuhInstitutionSyncReports", "muh-institution-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MuhInstitution sync file found. Run a sync first." });

                return Ok(new MuhInstitutionChangedEntitiesDto { InstitutionsUpdated = summary.AllInstitutionsUpdated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MuhInstitution changes", message = ex.Message });
            }
        }

        [HttpGet("by-region/{regionId}")]
        public async Task<IActionResult> GetByRegion([FromRoute] int regionId)
        {
            var institutions = await _context.MuhInstitutions
                .Where(i => i.RegionId == regionId)
                .ToListAsync();

            return Ok(institutions.Select(MapToDto));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var institution = await _context.MuhInstitutions
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();

            if (institution == null)
                return NotFound(new { message = $"Institution with id {id} not found." });

            return Ok(MapToDto(institution));
        }
    }
}
