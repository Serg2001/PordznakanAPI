namespace PordznakanAPI.DTOs
{
    public class MmuhInstitutionSyncSummaryDto
    {
        public DateTime SyncCompletedAt { get; set; }
        public int TotalRegionsProcessed { get; set; }
        public int SuccessfulRegions { get; set; }
        public int FailedRegions { get; set; }
        public int TotalInstitutionsAdded { get; set; }
        public int TotalInstitutionsUpdated { get; set; }
        public List<MmuhInstitutionDto> AllInstitutionsUpdated { get; set; } = new();
    }
}
