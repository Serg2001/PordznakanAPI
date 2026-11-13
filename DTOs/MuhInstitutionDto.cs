namespace PordznakanAPI.DTOs
{
    public class MuhInstitutionDto
    {
        public Guid Id { get; set; }
        public int InstitutionId { get; set; }
        public int RegionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int InstitutionType { get; set; }
        public string InstitutionTypeName { get; set; } = string.Empty;
        public string Marz { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Community { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
