using PordznakanAPI.Enums;

namespace PordznakanAPI.Models
{
    public class Pupil
    {
        public Guid Id { get; set; }
        public int KtakPupilId { get; set; }           // id from external API
        public int KtakSchoolId { get; set; }          // schools_id from external API
        public string ClassroomId { get; set; } = string.Empty;  // classroom id from external API (KtakClassroomId)
        public Guid? ClassroomInternalId { get; set; }           // Foreign key to Classroom.Id (internal database ID)

        public KtakPlace Place { get; set; }
        public EGrade Grade { get; set; }
        public ESubGrade SubGrade { get; set; }

        public string FirstName { get; set; } = string.Empty;    // first_name
        public string LastName { get; set; } = string.Empty;     // last_name
        public string FatherName { get; set; } = string.Empty;   // father_name

        public ECertificateType CertificateType { get; set; }    // ident_document
        public string Certificate { get; set; } = string.Empty;  // ident_document_number

        public DateOnly Birthday { get; set; }                   // date_of_birth
        public bool Gender { get; set; }                         // true-male, false-female

        public EPupilStatus Status { get; set; }

        public string MD5 { get; set; } = string.Empty;          // hash of important fields

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Classroom? Classroom { get; set; }
    }
}
