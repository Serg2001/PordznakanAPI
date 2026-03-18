using PordznakanAPI.Enums;

namespace PordznakanAPI.Models
{
    public class Teacher
    {
        public Guid Id { get; set; }
        public int KtakTeacherId { get; set; }  // person_id from API
        public int KtakSchoolId { get; set; }   // school_id from API
        public int RegionId { get; set; }       // Region identifier (1-11)
        
        public KtakPlace Place { get; set; } = KtakPlace.School;
        
        /// <summary>
        /// Անուն
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// Ազգանուն
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// Հայրանուն
        /// </summary>
        public string FatherName { get; set; } = string.Empty;
        
        /// <summary>
        /// Սեռ (true-male, false-female)
        /// </summary>
        public bool Gender { get; set; }
        
        /// <summary>
        /// Ծննդյան ամսաթիվ
        /// </summary>
        public DateOnly? Birthday { get; set; }
        
        /// <summary>
        /// Հեռախոս
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        
        /// <summary>
        /// Հասցե
        /// </summary>
        public string Address { get; set; } = string.Empty;
        
        /// <summary>
        /// Էլ հասցե
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Սոց քարտ
        /// </summary>
        public string SocNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Աշխատանքային փորձ (Ստաժ)
        /// </summary>
        public int Experience { get; set; }
        
        /// <summary>
        /// Գիտական կոչում
        /// </summary>
        public ERank AcademicRank { get; set; } = ERank.Unknown;
        
        /// <summary>
        /// Կրթություն
        /// </summary>
        public EEducation Education { get; set; } = EEducation.Unknown;
        
        public DateTime? CommandDate { get; set; }
        
        public EDigitLevel DigitLevel { get; set; } = EDigitLevel.Unknown;
        
        public string Activated { get; set; } = string.Empty;
        public string WorkType { get; set; } = string.Empty;
        
        /// <summary>
        /// Main subject ID from API
        /// </summary>
        public string MainSubjectId { get; set; } = string.Empty;
        
        /// <summary>
        /// Main subject name
        /// </summary>
        public string MainSubject { get; set; } = string.Empty;
        
        public string? PersonPositions { get; set; }
        
        public string MD5 { get; set; } = string.Empty;  // hash of important fields
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public List<TeacherSubject> Subjects { get; set; } = new();
    }
}
