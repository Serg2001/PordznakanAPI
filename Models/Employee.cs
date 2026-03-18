namespace PordznakanAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public int ExternalPersonId { get; set; }
        public string Ssn { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty; // "Director", "Teacher", etc.     
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property - Schools where this employee is a director
        public ICollection<School> DirectedSchools { get; set; }
    }
}
