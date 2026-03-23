namespace PordznakanAPI.Models
{
    public class NmuhStaff
    {
        public Guid Id { get; set; }
        public int NmuhStaffId { get; set; }                               // staff_id from external API
        public int InstId { get; set; }                                    // inst_id (institution ID)
        public Guid? InternalSchoolId { get; set; }                        // FK to NmuhInstitution.Id in our DB
        public int RegionId { get; set; }                                  // Region identifier (1-11)
        public string InstName { get; set; } = string.Empty;              // inst_name (institution name)
        
        public string FirstName { get; set; } = string.Empty;              // first_name
        public string LastName { get; set; } = string.Empty;              // last_name
        public string FatherName { get; set; } = string.Empty;            // father_name
        
        public DateOnly DateOfBirth { get; set; }                          // date_of_birth
        public string SocNumber { get; set; } = string.Empty;              // soc_number
        public bool Sex { get; set; }                                          // sex: true = male (1), false = female (0)
        public string Address { get; set; } = string.Empty;                // address
        public string Phone { get; set; } = string.Empty;                  // phone
        public string Citizenship { get; set; } = string.Empty;           // citizenship
        public string Nationality { get; set; } = string.Empty;            // nationality
        public string IdentDocument { get; set; } = string.Empty;          // ident_document
        public string IdentDocumentNumber { get; set; } = string.Empty;    // ident_document_number
        public string FromCountry { get; set; } = string.Empty;            // from_country
        public string InFiz { get; set; } = string.Empty;                  // in_fiz
        public string Druyq { get; set; } = string.Empty;                  // druyq
        public string? PartlyIds { get; set; }                              // partly_ids
        public string? PartlyInstNames { get; set; }                       // partly_inst_names
        public string PositionName { get; set; } = string.Empty;          // position_name
        public string PositionId { get; set; } = string.Empty;            // position_id
        public string PositionDetailId { get; set; } = string.Empty;       // position_detail_id
        public string PositionDetailName { get; set; } = string.Empty;    // position_detail_name
        public List<int> GroupIds { get; set; } = new();                      // group_id parsed as list of ints (stored as JSON)

        public List<NmuhStaffGroup> Groups { get; set; } = new();          // groups with subjects (navigation property)
        
        public string MD5 { get; set; } = string.Empty;                    // hash of important fields
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

