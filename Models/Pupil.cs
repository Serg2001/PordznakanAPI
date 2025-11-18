namespace PordznakanAPI.Models
{
    public class Pupil
    {
        public int Id { get; set; }
        public string ExternalPupilId { get; set; } = string.Empty;
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
        public int ClassroomId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Classroom Classroom { get; set; }
    }
}
