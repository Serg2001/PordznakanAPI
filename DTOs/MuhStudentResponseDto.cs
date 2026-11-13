namespace PordznakanAPI.DTOs
{
    /// <summary>
    /// Shape returned by the muh-students by-region / by-institution / by-id endpoints
    /// (the shape the former MMUH/NMUH student endpoints had). Graduated and ClassroomGrade
    /// no longer exist in our database, so they are always null.
    /// </summary>
    public class MuhStudentResponseDto
    {
        public Guid Id { get; set; }
        public int MuhStudentId { get; set; }
        public int MuhSchoolId { get; set; }
        public int RegionId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string Marz { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string SocNumber { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public bool? Graduated { get; set; }
        public string EduYear { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public int? ClassroomGrade { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
