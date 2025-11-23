using PordznakanAPI.Enums;

namespace PordznakanAPI.Models
{
    public class Pupil
    {
        public Guid Id { get; set; }
        public string KtakPupilId { get; set; } = string.Empty;  // id from external API
        public string KtakSchoolId { get; set; } = string.Empty;  // schools_id from external API
        public string ClassroomId { get; set; } = string.Empty;  // classroom id from external API (KtakClassroomId)
        public Guid? ClassroomInternalId { get; set; }  // Foreign key to Classroom.Id (internal database ID)
        public string Grade { get; set; } = string.Empty;
        public string Classifier { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string? Stream { get; set; }
        public string FirstName { get; set; } = string.Empty;  // first_name
        public string LastName { get; set; } = string.Empty;  // last_name
        public string FatherName { get; set; } = string.Empty;  // father_name
        public string IdentDocument { get; set; } = string.Empty;  // ident_document
        public string IdentDocumentNumber { get; set; } = string.Empty;  // ident_document_number
        public string FromCountry { get; set; } = string.Empty;  // from_country
        public string SocNumber { get; set; } = string.Empty;  // soc_number
        public DateTime? DateOfBirth { get; set; }  // date_of_birth
        public string Sex { get; set; } = string.Empty;  // sex (code from external API)
        public string Status { get; set; } = string.Empty;  // status (e.g., "graduated")
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Classroom? Classroom { get; set; }
    }
}
