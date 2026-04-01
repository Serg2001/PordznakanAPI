namespace PordznakanAPI.Models
{
    public class NmuhSubject
    {
        public Guid Id { get; set; }
        public Guid NmuhStaffGroupId { get; set; }              // FK to NmuhStaffGroup
        public int SubjectId { get; set; }                      // subject_id from API
        public string SubjectName { get; set; } = string.Empty; // subject from API
        public string SubjectType { get; set; } = string.Empty; // subject_type from API
        public int SubjectTypeId { get; set; }                  // subject_type_id from API
        public int Grade { get; set; }                          // grade
        public int SubGrade { get; set; }                       // sub grade

        public NmuhStaffGroup NmuhStaffGroup { get; set; } = null!;
    }
}
