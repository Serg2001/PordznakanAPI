namespace PordznakanAPI.DTOs
{
    public class NmuhInstitutionSyncSummaryDto
    {
        public DateTime SyncCompletedAt { get; set; }
        public int TotalRegionsProcessed { get; set; }
        public int SuccessfulRegions { get; set; }
        public int FailedRegions { get; set; }
        public int TotalInstitutionsAdded { get; set; }
        public int TotalInstitutionsUpdated { get; set; }
        public List<NmuhInstitutionDto> AllInstitutionsUpdated { get; set; } = new();
    }
}
