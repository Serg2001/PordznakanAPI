namespace PordznakanAPI.Models
{
    public class MmuhInstitution
    {
        public Guid Id { get; set; }
        public int InstId { get; set; }                                  // id from external API
        public int RegionId { get; set; }                               // marz queried (1-11)
        public string Name { get; set; } = string.Empty;                // name
        public string LegalMarzId { get; set; } = string.Empty;        // legal_marz_id
        public string LegalAddress { get; set; } = string.Empty;       // legal_address
        public string BusinessMarzId { get; set; } = string.Empty;     // business_marz_id
        public string BusinessAddress { get; set; } = string.Empty;    // business_address

        public string MD5 { get; set; } = string.Empty;                // hash of important fields

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
