namespace PordznakanAPI.Models
{
    public class MmuhStaffGroup
    {
        public Guid Id { get; set; }
        public Guid MmuhStaffId { get; set; }                // FK to MmuhStaff
        public int GroupId { get; set; }                     // group_id from API
        public string GroupName { get; set; } = string.Empty; // group from API

        public MmuhStaff MmuhStaff { get; set; } = null!;
        public List<MmuhSubject> Subjects { get; set; } = new();
    }
}
