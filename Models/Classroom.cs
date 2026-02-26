namespace PordznakanAPI.Models
{
    public class Classroom
    {
        public Guid Id { get; set; }

        public string KtakSchoolId { get; set; } = string.Empty;

        public string KtakClassroomId { get; set; } = string.Empty;
        
        public int RegionId { get; set; }              // Region identifier (1-10)

        public string Grade { get; set; } = string.Empty;

        public string Classifier {  get; set; } = string.Empty;

        public string ClassName {  get; set; } = string.Empty;

        public string? Stream { get; set; } = string.Empty;

        public Guid SchoolId { get; set; }  // Foreign key to School.DshhSchoolId

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        public School School { get; set; } = null!;
        public ICollection<Pupil> Pupils { get; set; } = new List<Pupil>();
    }
}
