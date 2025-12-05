using PordznakanAPI.Enums;

namespace PordznakanAPI.DTOs
{
    public class TeacherDto
    {
        public Guid Id { get; set; }
        public int KtakTeacherId { get; set; }
        public int KtakSchoolId { get; set; }
        public KtakPlace Place { get; set; }
        
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
        public ERank AcademicRank { get; set; }
        
        /// <summary>
        /// Կրթություն
        /// </summary>
        public EEducation Education { get; set; }
        
        public DateTime? CommandDate { get; set; }
        
        public List<TeacherSubjectDto> Subjects { get; set; } = new List<TeacherSubjectDto>();
        
        public EDigitLevel DigitLevel { get; set; }
        
        public string Activated { get; set; } = string.Empty;
        public string WorkType { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}