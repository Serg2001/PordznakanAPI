namespace PordznakanAPI.Models
{
    public class NmuhStaffGroup
    {
        public Guid Id { get; set; }
        public Guid NmuhStaffId { get; set; }               // FK to NmuhStaff
        public int GroupId { get; set; }                    // group_id from API
        public string GroupName { get; set; } = string.Empty; // group from API

        public NmuhStaff NmuhStaff { get; set; } = null!;
        public List<NmuhSubject> Subjects { get; set; } = new();
    }
}
