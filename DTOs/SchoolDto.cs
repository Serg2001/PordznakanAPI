namespace PordznakanAPI.DTOs
{
    public class SchoolDto
    {
        public Guid DshhSchoolId { get; set; }
        public int KtakSchoolId { get; set; }
        public int RegionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Marz { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Community { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
