namespace PordznakanAPI.Models
{
    public class Classroom
    {
        public int Id { get; set; }

        public string KtakId { get; set; } = string.Empty;

        public string Grade { get; set; } = string.Empty;

        public string Classifier {  get; set; } = string.Empty;

        public string ClassName {  get; set; } = string.Empty;

        public string? Stream { get; set; } = string.Empty;

        public int SchoolId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        ////Navigation Properties
        //public School School { get; set; }
        //public ICollection<Pupil> Pupils { get; set; }
    }
}
