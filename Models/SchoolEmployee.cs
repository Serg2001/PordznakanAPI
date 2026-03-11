namespace PordznakanAPI.Models
{
    public class SchoolEmployee
    {
        public Guid Id { get; set; }
        public int PersonId { get; set; }               // person_id from API
        public int SchoolId { get; set; }               // school_id from API
        public int RegionId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? MainSubjectId { get; set; }
        public string Position { get; set; } = string.Empty;       // position from person_positions
        public string StaffGroup { get; set; } = string.Empty;     // staff_group from person_positions
        public int? VacationId { get; set; }                        // vacantion_id from person_positions
        public string MD5 { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
