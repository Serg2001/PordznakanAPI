namespace PordznakanAPI.Models
{
    public class LogMmuhEmployee
    {
        public Guid Id { get; set; }
        public int LogId { get; set; }                              // id from external API
        public int SchoolId { get; set; }                            // school_id
        public DateTime ActionDate { get; set; }                     // action_date
        public string Method { get; set; } = string.Empty;          // method (e.g., "ExemptEmployee")
        public string Sent { get; set; } = string.Empty;            // sent (JSON string)
        public string Received { get; set; } = string.Empty;        // received (JSON string)
        public bool Transferred { get; set; } = false;              // Flag to track if data was transferred to external API
        public DateTime? TransferredAt { get; set; }                // Timestamp when data was transferred
        public string? TransferError { get; set; }                  // Error message if transfer failed
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

