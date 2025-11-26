namespace PordznakanAPI.DTOs
{
    public class TeacherSyncSummaryDto
    {
        public DateTime SyncCompletedAt { get; set; }
        public int TotalRegionsProcessed { get; set; }
        public int SuccessfulRegions { get; set; }
        public int FailedRegions { get; set; }
        public int TotalTeachersAdded { get; set; }
        public int TotalTeachersUpdated { get; set; }
        public List<TeacherDto> AllTeachersUpdated { get; set; } = new();
    }
}

