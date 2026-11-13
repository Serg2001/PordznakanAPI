namespace PordznakanAPI.DTOs
{
    public class MuhInstitutionSyncSummaryDto
    {
        public DateTime SyncCompletedAt { get; set; }
        public int TotalRegionsProcessed { get; set; }
        public int SuccessfulRegions { get; set; }
        public int FailedRegions { get; set; }
        public int TotalInstitutionsAdded { get; set; }
        public int TotalInstitutionsUpdated { get; set; }
        public List<MuhInstitutionDto> AllInstitutionsUpdated { get; set; } = new();
    }
}
