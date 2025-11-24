namespace PordznakanAPI.DTOs
{
    public class PupilDto
    {
        public Guid Id { get; set; }
        public string KtakPupilId { get; set; } = string.Empty;
        public string KtakSchoolId { get; set; } = string.Empty;
        public string ClassroomId { get; set; } = string.Empty;
        public Guid? ClassroomInternalId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string IdentDocument { get; set; } = string.Empty;
        public string IdentDocumentNumber { get; set; } = string.Empty;
        public string FromCountry { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Sex { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

