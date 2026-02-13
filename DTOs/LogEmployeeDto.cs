namespace PordznakanAPI.DTOs
{
    public class LogEmployeeDto
    {
        public Guid Id { get; set; }
        public int LogId { get; set; }
        public int SchoolId { get; set; }
        public DateTime ActionDate { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Sent { get; set; } = string.Empty;
        public string Received { get; set; } = string.Empty;
        public bool Transferred { get; set; }
        public DateTime? TransferredAt { get; set; }
        public string? TransferError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

