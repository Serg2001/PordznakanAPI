using PordznakanAPI.Enums;

namespace PordznakanAPI.DTOs
{
    public class NmuhStudentDto
    {
        public Guid Id { get; set; }
        public string NmuhStudentId { get; set; } = string.Empty;
        public string NmuhSchoolId { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string Marz { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string SocNumber { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public bool Graduated { get; set; }
        public string EduYear { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public EGrade ClassroomGrade { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

