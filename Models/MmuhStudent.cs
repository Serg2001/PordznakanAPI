using PordznakanAPI.Enums;

namespace PordznakanAPI.Models
{
    public class MmuhStudent
    {
        public Guid Id { get; set; }
        public int MmuhStudentId { get; set; }                          // student_id from external API
        public int MmuhSchoolId { get; set; }                          // school_id from external API
        public Guid? InternalSchoolId { get; set; }                    // FK to MmuhInstitution.Id in our DB
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
        public string GroupId { get; set; } = string.Empty;            // group_id
        public EGrade ClassroomGrade { get; set; }                     // classroom_grade
        
        public string MD5 { get; set; } = string.Empty;                // hash of important fields

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

