using PordznakanAPI.Enums;

namespace PordznakanAPI.Models
{
    /// <summary>
    /// Staging copy of teacher schema used for MD5-based sync.
    /// </summary>
    public class TeacherStaging
    {
        public Guid Id { get; set; }
        public int KtakTeacherId { get; set; }
        public int KtakSchoolId { get; set; }
        
        public KtakPlace Place { get; set; } = KtakPlace.School;
        
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        
        public bool Gender { get; set; }
        public DateOnly? Birthday { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public int Experience { get; set; }
        public ERank AcademicRank { get; set; } = ERank.Unknown;
        public EEducation Education { get; set; } = EEducation.Unknown;
        public DateTime? CommandDate { get; set; }
        public EDigitLevel DigitLevel { get; set; } = EDigitLevel.Unknown;
        public string Activated { get; set; } = string.Empty;
        public string WorkType { get; set; } = string.Empty;
        public string MainSubjectId { get; set; } = string.Empty;
        public string MainSubject { get; set; } = string.Empty;
        public string? PersonPositions { get; set; }
        
        public string MD5 { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}