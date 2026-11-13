namespace PordznakanAPI.DTOs
{
    public class MuhStudentDto
    {
        public Guid Id { get; set; }
        public int MuhStudentId { get; set; }
        public int InstitutionId { get; set; }
        public Guid? InternalSchoolId { get; set; }
        public int RegionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string InstitutionEmail { get; set; } = string.Empty;
        public int InstitutionType { get; set; }
        public string InstitutionTypeName { get; set; } = string.Empty;
        public string Marz { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Community { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EduProgramCode { get; set; } = string.Empty;
        public string EduProgramTitle { get; set; } = string.Empty;
        public string EduProgramProfCode { get; set; } = string.Empty;
        public string EduProgramProfTitle { get; set; } = string.Empty;
        public string EduYear { get; set; } = string.Empty;
        public int Base { get; set; }
        public int VeGroupTypes { get; set; }
        public string GroupType { get; set; } = string.Empty;
        public int Duration { get; set; }
        public decimal AnnualFee { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string IdentDocument { get; set; } = string.Empty;
        public string IdentDocumentNumber { get; set; } = string.Empty;
        public string FromCountry { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Sex { get; set; } = string.Empty;
        public bool FreeEdu { get; set; }
        public bool HasBook { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
