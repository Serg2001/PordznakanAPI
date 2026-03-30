namespace PordznakanAPI.DTOs
{
    public class MmuhStudentDto
    {
        public Guid Id { get; set; }
        public int MmuhStudentId { get; set; }
        public int MmuhSchoolId { get; set; }
        public Guid? InternalSchoolId { get; set; }
        public int RegionId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string Marz { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string SocNumber { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public bool Graduated { get; set; }
        public string GroupId { get; set; } = string.Empty;
        public int ClassroomGrade { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

