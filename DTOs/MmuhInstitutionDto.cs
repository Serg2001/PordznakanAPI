namespace PordznakanAPI.DTOs
{
    public class MmuhInstitutionDto
    {
        public Guid Id { get; set; }
        public int InstId { get; set; }
        public int RegionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LegalMarzId { get; set; } = string.Empty;
        public string LegalAddress { get; set; } = string.Empty;
        public string BusinessMarzId { get; set; } = string.Empty;
        public string BusinessAddress { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
