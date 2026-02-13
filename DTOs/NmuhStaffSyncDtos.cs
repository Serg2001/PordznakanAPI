namespace PordznakanAPI.DTOs
{
    public class NmuhStaffSyncSummaryDto
    {
        public DateTime SyncCompletedAt { get; set; }
        public int TotalRegionsProcessed { get; set; }
        public int SuccessfulRegions { get; set; }
        public int FailedRegions { get; set; }
        public int TotalStaffAdded { get; set; }
        public int TotalStaffUpdated { get; set; }
        public List<NmuhStaffDto> AllStaffUpdated { get; set; } = new();
    }
}

