using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    public class NmuhInstitutionSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int InstitutionsProcessed { get; set; }
        public int InstitutionsAdded { get; set; }
        public int InstitutionsUpdated { get; set; }
        public List<NmuhInstitutionDto> InstitutionsUpdatedList { get; set; } = new();
    }

    public class NmuhInstitutionChangedEntitiesDto
    {
        public List<NmuhInstitutionDto> InstitutionsUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class NmuhInstitutionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISyncReportService _syncReportService;
        private readonly ILogger<NmuhInstitutionController>? _logger;

        public NmuhInstitutionController(AppDbContext context, ISyncReportService syncReportService, ILogger<NmuhInstitutionController>? logger = null)
        {
            _context = context;
            _syncReportService = syncReportService;
            _logger = logger;
        }

        private NmuhInstitutionDto MapToDto(NmuhInstitution institution)
        {
            return new NmuhInstitutionDto
            {
                Id = institution.Id,
                InstId = institution.InstId,
                RegionId = institution.RegionId,
                Name = institution.Name,
                LegalMarzId = institution.LegalMarzId,
                LegalAddress = institution.LegalAddress,
                BusinessMarzId = institution.BusinessMarzId,
                BusinessAddress = institution.BusinessAddress,
                CreatedAt = institution.CreatedAt,
                UpdatedAt = institution.UpdatedAt
            };
        }

        [NonAction]
        public async Task SyncAllRegions()
        {
            var regionIds = Enumerable.Range(1, 11).ToArray();
            var results = new List<NmuhInstitutionSyncResult>();

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Syncing NmuhInstitution region {regionId}...");
                    var result = await SyncRegionInternal(regionId);
                    results.Add(result);

                    if (result.Success)
                    {
                        _logger?.LogInformation($"Region {regionId} synced successfully. " +
                            $"Institutions: {result.InstitutionsAdded} added, {result.InstitutionsUpdated} updated.");
                    }
                    else
                    {
                        _logger?.LogError($"Region {regionId} sync failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Exception while syncing NmuhInstitution region {regionId}");
                    results.Add(new NmuhInstitutionSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new NmuhInstitutionSyncSummaryDto
            {
                SyncCompletedAt = DateTime.UtcNow,
                TotalRegionsProcessed = regionIds.Length,
                SuccessfulRegions = results.Count(r => r.Success),
                FailedRegions = results.Count(r => !r.Success),
                TotalInstitutionsAdded = results.Sum(r => r.InstitutionsAdded),
                TotalInstitutionsUpdated = results.Sum(r => r.InstitutionsUpdated)
            };

            foreach (var result in results.Where(r => r.Success))
            {
                summary.AllInstitutionsUpdated.AddRange(result.InstitutionsUpdatedList);
            }

            _logger?.LogInformation($"NmuhInstitution sync completed for all regions. " +
                $"Success: {summary.SuccessfulRegions}/{regionIds.Length}. " +
                $"Total - Institutions: {summary.TotalInstitutionsAdded} added, {summary.TotalInstitutionsUpdated} updated.");
        }

        private async Task<NmuhInstitutionSyncResult> SyncRegionInternal(int regionId)
        {
            var result = new NmuhInstitutionSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                using var client = new HttpClient();
                var url = $"https://api.emis.am/v1/get_nmuh_institutions_by_marz/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var institutionsArray = JArray.Parse(responseText);

                await _context.NmuhInstitutionsStaging.ExecuteDeleteAsync();

                var now = DateTime.UtcNow;

                foreach (var token in institutionsArray)
                {
                    if (token is not JObject obj)
                        continue;

                    if (!int.TryParse(obj["id"]?.ToString(), out var instId))
                        continue;

                    var name = obj["name"]?.ToString() ?? string.Empty;
                    var legalMarzId = obj["legal_marz_id"]?.ToString() ?? string.Empty;
                    var legalAddress = obj["legal_address"]?.ToString() ?? string.Empty;
                    var businessMarzId = obj["business_marz_id"]?.ToString() ?? string.Empty;
                    var businessAddress = obj["business_address"]?.ToString() ?? string.Empty;

                    var md5 = SyncHelpers.ComputeMd5(
                        instId.ToString(),
                        name,
                        legalMarzId,
                        legalAddress,
                        businessMarzId,
                        businessAddress);

                    _context.NmuhInstitutionsStaging.Add(new NmuhInstitutionStaging
                    {
                        Id = Guid.NewGuid(),
                        InstId = instId,
                        RegionId = regionId,
                        Name = name,
                        LegalMarzId = legalMarzId,
                        LegalAddress = legalAddress,
                        BusinessMarzId = businessMarzId,
                        BusinessAddress = businessAddress,
                        MD5 = md5,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }

                await _context.SaveChangesAsync();

                var stagingRows = await _context.NmuhInstitutionsStaging.ToListAsync();

                var stagedIds = stagingRows.Select(s => s.InstId).Distinct().ToList();
                var existingInstitutions = await _context.NmuhInstitutions
                    .Where(i => stagedIds.Contains(i.InstId))
                    .ToListAsync();

                var existingDict = existingInstitutions.ToDictionary(i => i.InstId);

                var newInstitutions = new List<NmuhInstitution>();
                var updatedCount = 0;

                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.InstId, out var existing))
                    {
                        if (!string.Equals(existing.MD5, staging.MD5, StringComparison.OrdinalIgnoreCase))
                        {
                            existing.RegionId = staging.RegionId;
                            existing.Name = staging.Name;
                            existing.LegalMarzId = staging.LegalMarzId;
                            existing.LegalAddress = staging.LegalAddress;
                            existing.BusinessMarzId = staging.BusinessMarzId;
                            existing.BusinessAddress = staging.BusinessAddress;
                            existing.MD5 = staging.MD5;
                            existing.UpdatedAt = DateTime.UtcNow;

                            updatedCount++;
                            result.InstitutionsUpdatedList.Add(MapToDto(existing));
                        }
                    }
                    else
                    {
                        newInstitutions.Add(new NmuhInstitution
                        {
                            Id = Guid.NewGuid(),
                            InstId = staging.InstId,
                            RegionId = staging.RegionId,
                            Name = staging.Name,
                            LegalMarzId = staging.LegalMarzId,
                            LegalAddress = staging.LegalAddress,
                            BusinessMarzId = staging.BusinessMarzId,
                            BusinessAddress = staging.BusinessAddress,
                            MD5 = staging.MD5,
                            CreatedAt = staging.CreatedAt,
                            UpdatedAt = staging.UpdatedAt
                        });
                    }
                }

                _logger?.LogInformation($"[Region {regionId}] NmuhInstitution MD5 compare done. New={newInstitutions.Count}, Updated={updatedCount}. Saving...");

                if (newInstitutions.Any())
                {
                    _context.NmuhInstitutions.AddRange(newInstitutions);
                }

                if (updatedCount > 0 || newInstitutions.Any())
                {
                    await _context.SaveChangesAsync();
                }

                await _context.NmuhInstitutionsStaging.ExecuteDeleteAsync();

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
                _logger?.LogError(ex, $"Error syncing NmuhInstitution for region {regionId}");
                return result;
            }
        }

        [HttpPost("sync/{regionId?}")]
        public async Task<IActionResult> SyncInstitutions([FromRoute] int regionId = 1)
        {
            var result = await SyncRegionInternal(regionId);

            if (result.Success)
            {
                return Ok(new
                {
                    message = "NmuhInstitution sync completed successfully!",
                    regionId = result.RegionId,
                    institutionsProcessed = result.InstitutionsProcessed,
                    institutionsAdded = result.InstitutionsAdded,
                    institutionsUpdated = result.InstitutionsUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "NmuhInstitution sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestSync()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<NmuhInstitutionSyncSummaryDto>(
                    "NmuhInstitutionSyncReports", "nmuh-institution-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No NmuhInstitution sync file found. Run a sync first." });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load NmuhInstitution sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestChanges()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<NmuhInstitutionSyncSummaryDto>(
                    "NmuhInstitutionSyncReports", "nmuh-institution-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No NmuhInstitution sync file found. Run a sync first." });

                return Ok(new NmuhInstitutionChangedEntitiesDto { InstitutionsUpdated = summary.AllInstitutionsUpdated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load NmuhInstitution changes", message = ex.Message });
            }
        }

        [HttpGet("by-region/{regionId}")]
        public async Task<IActionResult> GetByRegion([FromRoute] int regionId)
        {
            var institutions = await _context.NmuhInstitutions
                .Where(i => i.RegionId == regionId)
                .Select(i => new NmuhInstitutionDto
                {
                    Id = i.Id,
                    InstId = i.InstId,
                    RegionId = i.RegionId,
                    Name = i.Name,
                    LegalMarzId = i.LegalMarzId,
                    LegalAddress = i.LegalAddress,
                    BusinessMarzId = i.BusinessMarzId,
                    BusinessAddress = i.BusinessAddress,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .ToListAsync();

            return Ok(institutions);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var institution = await _context.NmuhInstitutions
                .Where(i => i.Id == id)
                .Select(i => new NmuhInstitutionDto
                {
                    Id = i.Id,
                    InstId = i.InstId,
                    RegionId = i.RegionId,
                    Name = i.Name,
                    LegalMarzId = i.LegalMarzId,
                    LegalAddress = i.LegalAddress,
                    BusinessMarzId = i.BusinessMarzId,
                    BusinessAddress = i.BusinessAddress,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (institution == null)
                return NotFound(new { message = $"Institution with id {id} not found." });

            return Ok(institution);
        }
    }
}
