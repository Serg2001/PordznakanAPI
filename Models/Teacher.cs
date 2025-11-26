namespace PordznakanAPI.Models
{
    public class Teacher
    {
        public Guid Id { get; set; }
        public string PersonId { get; set; } = string.Empty; // person_id from API
        public string SchoolId { get; set; } = string.Empty; // school_id from API (matches KtakSchoolId)
        public string SchoolName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Activated { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string WorkType { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public DateTime? CommandDate { get; set; }
        public string SubjectId { get; set; } = string.Empty;
        public string MainSubject { get; set; } = string.Empty;
        public string? PersonPositions { get; set; }
        public string? SubjectsJson { get; set; }
        public string? DigitLevel { get; set; }
        public string? Experience { get; set; }
        public string? AcademicRank { get; set; }
        public string? AcademicRankId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

