namespace PordznakanAPI.DTOs
{
    public class MuhStudentSyncSummaryDto
    {
        public DateTime SyncCompletedAt { get; set; }
        public int TotalRegionsProcessed { get; set; }
        public int SuccessfulRegions { get; set; }
        public int FailedRegions { get; set; }
        public int TotalStudentsAdded { get; set; }
        public int TotalStudentsUpdated { get; set; }
        public List<MuhStudentDto> AllStudentsUpdated { get; set; } = new();
    }
}
