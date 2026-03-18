using PordznakanAPI.Enums;

namespace PordznakanAPI.Models
{
    public class NmuhStudent
    {
        public Guid Id { get; set; }
        public string NmuhStudentId { get; set; } = string.Empty;      // student_id from external API
        public string NmuhSchoolId { get; set; } = string.Empty;       // school_id from external API
        public int RegionId { get; set; }                               // Region identifier (1-11)
        public string SchoolName { get; set; } = string.Empty;         // school_name
        public string Marz { get; set; } = string.Empty;               // marz (region)
        
        public string FirstName { get; set; } = string.Empty;          // first_name
        public string LastName { get; set; } = string.Empty;          // last_name
        public string FatherName { get; set; } = string.Empty;        // father_name
        
        public DateOnly DateOfBirth { get; set; }                      // date_of_birth
        public string SocNumber { get; set; } = string.Empty;          // soc_number
        public string Sex { get; set; } = string.Empty;                // sex (stored as string to preserve original value)
        public bool Graduated { get; set; }                            // graduated (1 = true, 0 = false)
        public string EduYear { get; set; } = string.Empty;            // edu_year (e.g., "2020-2021")
        public string GroupId { get; set; } = string.Empty;            // group_id
        public EGrade ClassroomGrade { get; set; }                     // classroom_grade
        
        public string MD5 { get; set; } = string.Empty;                // hash of important fields

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

