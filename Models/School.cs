using PordznakanAPI.Models;

public class School
{
    public int Id { get; set; }
    public int KtakId { get; set; }           // This is the external ID from EMIS
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}