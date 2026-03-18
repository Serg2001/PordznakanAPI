namespace PordznakanAPI.DTOs
{
    public class ClassroomDto
    {
        public Guid Id { get; set; }
        public int KtakSchoolId { get; set; }
        public string KtakClassroomId { get; set; } = string.Empty;
        public int RegionId { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string Classifier { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string? Stream { get; set; }
        public Guid SchoolId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

