namespace PordznakanAPI.DTOs
{
    public class MmuhStaffDto
    {
        public Guid Id { get; set; }
        public string MmuhStaffId { get; set; } = string.Empty;
        public string InstId { get; set; } = string.Empty;
        public int RegionId { get; set; }
        public string InstName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string SocNumber { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Citizenship { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string IdentDocument { get; set; } = string.Empty;
        public string IdentDocumentNumber { get; set; } = string.Empty;
        public string FromCountry { get; set; } = string.Empty;
        public string InFiz { get; set; } = string.Empty;
        public string Druyq { get; set; } = string.Empty;
        public string? PartlyIds { get; set; }
        public string? PartlyInstNames { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string PositionId { get; set; } = string.Empty;
        public string PositionDetailId { get; set; } = string.Empty;
        public string PositionDetailName { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string GroupsJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

