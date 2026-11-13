namespace PordznakanAPI.Models
{
    /// <summary>
    /// Vocational (MMUH/NMUH) institution from
    /// GET https://data-api.emis.am/v1/colleges/getCollegesGeography/{regionId}.
    /// </summary>
    public class MuhInstitution
    {
        public Guid Id { get; set; }
        public int InstitutionId { get; set; }                                  // institution_id from external API
        public int RegionId { get; set; }                                       // region queried (1-11)
        public string Name { get; set; } = string.Empty;                        // name
        public string Email { get; set; } = string.Empty;                       // email
        public int InstitutionType { get; set; }                                // institution_type
        public string InstitutionTypeName { get; set; } = string.Empty;         // institution_type_name
        public string Marz { get; set; } = string.Empty;                        // marz
        public string Region { get; set; } = string.Empty;                      // region
        public string Community { get; set; } = string.Empty;                   // community
        public string Address { get; set; } = string.Empty;                     // address

        public string MD5 { get; set; } = string.Empty;                         // hash of important fields

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
