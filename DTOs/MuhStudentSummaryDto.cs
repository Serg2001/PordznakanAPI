namespace PordznakanAPI.DTOs
{
    public class MuhStudentSummaryDto
    {
        public Guid Id { get; set; }
        public int KtakPupilId { get; set; }     // MuhStudentId (id) from external API
        public int KtakSchoolId { get; set; }    // InstitutionId (institution_id) from external API
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public bool HasBook { get; set; }
    }
}
