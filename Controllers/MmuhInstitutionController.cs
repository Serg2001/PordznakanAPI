using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PordznakanAPI.Data;
using PordznakanAPI.DTOs;
using PordznakanAPI.Models;
using PordznakanAPI.Services;

namespace PordznakanAPI.Controllers
{
    public class MmuhInstitutionSyncResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RegionId { get; set; }
        public int InstitutionsProcessed { get; set; }
        public int InstitutionsAdded { get; set; }
        public int InstitutionsUpdated { get; set; }
        public List<MmuhInstitutionDto> InstitutionsUpdatedList { get; set; } = new();
    }

    public class MmuhInstitutionChangedEntitiesDto
    {
        public List<MmuhInstitutionDto> InstitutionsUpdated { get; set; } = new();
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MmuhInstitutionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISyncReportService _syncReportService;
        private readonly ILogger<MmuhInstitutionController>? _logger;

        public MmuhInstitutionController(AppDbContext context, ISyncReportService syncReportService, ILogger<MmuhInstitutionController>? logger = null)
        {
            _context = context;
            _syncReportService = syncReportService;
            _logger = logger;
        }

        private MmuhInstitutionDto MapToDto(MmuhInstitution institution)
        {
            return new MmuhInstitutionDto
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
            var results = new List<MmuhInstitutionSyncResult>();

            foreach (var regionId in regionIds)
            {
                try
                {
                    _logger?.LogInformation($"Syncing MmuhInstitution region {regionId}...");
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
                    _logger?.LogError(ex, $"Exception while syncing MmuhInstitution region {regionId}");
                    results.Add(new MmuhInstitutionSyncResult
                    {
                        RegionId = regionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            var summary = new MmuhInstitutionSyncSummaryDto
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

            _logger?.LogInformation($"MmuhInstitution sync completed for all regions. " +
                $"Success: {summary.SuccessfulRegions}/{regionIds.Length}. " +
                $"Total - Institutions: {summary.TotalInstitutionsAdded} added, {summary.TotalInstitutionsUpdated} updated.");
        }

        private async Task<MmuhInstitutionSyncResult> SyncRegionInternal(int regionId)
        {
            var result = new MmuhInstitutionSyncResult
            {
                RegionId = regionId,
                Success = false
            };

            try
            {
                using var client = new HttpClient();
                var url = $"https://api.emis.am/v1/get_mmuh_institutions_by_marz/{regionId}";
                var responseText = await client.GetStringAsync(url);
                var institutionsArray = JArray.Parse(responseText);

                // Clear staging table for this sync
                await _context.MmuhInstitutionsStaging.ExecuteDeleteAsync();

                var now = DateTime.UtcNow;

                // Process all institutions from API into staging
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

                    _context.MmuhInstitutionsStaging.Add(new MmuhInstitutionStaging
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

                // Save all staging data
                await _context.SaveChangesAsync();

                // Load all staged institutions
                var stagingRows = await _context.MmuhInstitutionsStaging.ToListAsync();

                // Get existing institutions by InstId
                var stagedIds = stagingRows.Select(s => s.InstId).Distinct().ToList();
                var existingInstitutions = await _context.MmuhInstitutions
                    .Where(i => stagedIds.Contains(i.InstId))
                    .ToListAsync();

                var existingDict = existingInstitutions.ToDictionary(i => i.InstId);  // int key

                var newInstitutions = new List<MmuhInstitution>();
                var updatedCount = 0;

                // Process each staged institution
                foreach (var staging in stagingRows)
                {
                    if (existingDict.TryGetValue(staging.InstId, out var existing))
                    {
                        // Compare MD5
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
                        newInstitutions.Add(new MmuhInstitution
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

                _logger?.LogInformation($"[Region {regionId}] MmuhInstitution MD5 compare done. New={newInstitutions.Count}, Updated={updatedCount}. Saving...");

                if (newInstitutions.Any())
                {
                    _context.MmuhInstitutions.AddRange(newInstitutions);
                }

                if (updatedCount > 0 || newInstitutions.Any())
                {
                    await _context.SaveChangesAsync();
                }

                // Cleanup staging
                await _context.MmuhInstitutionsStaging.ExecuteDeleteAsync();

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
                _logger?.LogError(ex, $"Error syncing MmuhInstitution for region {regionId}");
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
                    message = "MmuhInstitution sync completed successfully!",
                    regionId = result.RegionId,
                    institutionsProcessed = result.InstitutionsProcessed,
                    institutionsAdded = result.InstitutionsAdded,
                    institutionsUpdated = result.InstitutionsUpdated
                });
            }

            return StatusCode(500, new
            {
                error = "MmuhInstitution sync failed",
                message = result.ErrorMessage,
                regionId = result.RegionId
            });
        }

        [HttpGet("sync-changes/latest")]
        public IActionResult GetLatestSync()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MmuhInstitutionSyncSummaryDto>(
                    "MmuhInstitutionSyncReports", "mmuh-institution-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MmuhInstitution sync file found. Run a sync first." });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MmuhInstitution sync file", message = ex.Message });
            }
        }

        [HttpGet("changed-entities/latest")]
        public IActionResult GetLatestChanges()
        {
            try
            {
                var summary = _syncReportService.ReadLatestReport<MmuhInstitutionSyncSummaryDto>(
                    "MmuhInstitutionSyncReports", "mmuh-institution-sync-latest.json");

                if (summary == null)
                    return NotFound(new { message = "No MmuhInstitution sync file found. Run a sync first." });

                return Ok(new MmuhInstitutionChangedEntitiesDto { InstitutionsUpdated = summary.AllInstitutionsUpdated });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load MmuhInstitution changes", message = ex.Message });
            }
        }

        [HttpGet("by-region/{regionId}")]
        public async Task<IActionResult> GetByRegion([FromRoute] int regionId)
        {
            var institutions = await _context.MmuhInstitutions
                .Where(i => i.RegionId == regionId)
                .Select(i => new MmuhInstitutionDto
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
            var institution = await _context.MmuhInstitutions
                .Where(i => i.Id == id)
                .Select(i => new MmuhInstitutionDto
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
