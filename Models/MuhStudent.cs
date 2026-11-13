namespace PordznakanAPI.Models
{
    /// <summary>
    /// Vocational (MMUH/NMUH) student from
    /// GET https://data-api.emis.am/v1/colleges/getAllData/{regionId}
    /// (institution → group → students, flattened into one row per student).
    /// </summary>
    public class MuhStudent
    {
        public Guid Id { get; set; }
        public int MuhStudentId { get; set; }                           // id from external API
        public int InstitutionId { get; set; }                          // institution_id from external API
        public Guid? InternalSchoolId { get; set; }                     // FK to MuhInstitution.Id in our DB
        public int RegionId { get; set; }                               // Region identifier (1-11)
        public string InstitutionName { get; set; } = string.Empty;     // institution_name
        public int GroupId { get; set; }                                // group id
        public string GroupName { get; set; } = string.Empty;           // group name
        public string InstitutionEmail { get; set; } = string.Empty;    // institution_email
        public int InstitutionType { get; set; }    // institution_type
        public string InstitutionTypeName { get; set; } = string.Empty;    // institution_type_name
        public string Marz { get; set; } = string.Empty;    // marz
        public string Region { get; set; } = string.Empty;    // region
        public string Community { get; set; } = string.Empty;    // community
        public string Address { get; set; } = string.Empty;    // address
        public string EduProgramCode { get; set; } = string.Empty;    // edu_program_code
        public string EduProgramTitle { get; set; } = string.Empty;    // edu_program_title
        public string EduProgramProfCode { get; set; } = string.Empty;    // edu_program_prof_code
        public string EduProgramProfTitle { get; set; } = string.Empty;    // edu_program_prof_title
        public string EduYear { get; set; } = string.Empty;    // edu_year
        public int Base { get; set; }    // base
        public int VeGroupTypes { get; set; }    // ve_group_types
        public string GroupType { get; set; } = string.Empty;    // group_type
        public int Duration { get; set; }    // duration
        public decimal AnnualFee { get; set; }    // annual_fee

        public string FirstName { get; set; } = string.Empty;           // first_name
        public string LastName { get; set; } = string.Empty;            // last_name
        public string FatherName { get; set; } = string.Empty;          // father_name

        public string IdentDocument { get; set; } = string.Empty;       // ident_document (stored as string to preserve original value)
        public string IdentDocumentNumber { get; set; } = string.Empty; // ident_document_number
        public string FromCountry { get; set; } = string.Empty;         // from_country
        public string SocNumber { get; set; } = string.Empty;           // soc_number
        public DateOnly DateOfBirth { get; set; }                       // date_of_birth
        public string Sex { get; set; } = string.Empty;                 // sex (stored as string to preserve original value)
        public bool FreeEdu { get; set; }                               // free_edu (1 = true, 0 = false)

        public bool HasBook { get; set; }                               // true if student has received their book

        public string MD5 { get; set; } = string.Empty;                 // hash of important fields

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
