namespace PordznakanAPI.Models
{
    public class MmuhSubject
    {
        public Guid Id { get; set; }
        public Guid MmuhStaffGroupId { get; set; }               // FK to MmuhStaffGroup
        public int SubjectId { get; set; }                       // subject_id from API
        public string SubjectName { get; set; } = string.Empty;  // subject from API
        public string SubjectType { get; set; } = string.Empty;  // subject_type from API
        public int SubjectTypeId { get; set; }                   // subject_type_id from API

        public MmuhStaffGroup MmuhStaffGroup { get; set; } = null!;
    }
}
