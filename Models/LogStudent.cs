namespace PordznakanAPI.Models
{
    public class LogStudent
    {
        public int LogId { get; set; }                              // id from external API
        public int SchoolId { get; set; }                            // school_id
        public DateTime ActionDate { get; set; }                     // action_date
        public string Method { get; set; } = string.Empty;          // method (e.g., "StudentEdit")
        public string Sent { get; set; } = string.Empty;            // sent (JSON string)
        public string Received { get; set; } = string.Empty;        // received (JSON string)
    }
}

