using PordznakanAPI.Enums;

namespace PordznakanAPI.DTOs
{
    public class NmuhStudentSummaryDto
    {
        public Guid Id { get; set; }
        public int KtakPupilId { get; set; }     // NmuhStudentId from external API
        public int KtakSchoolId { get; set; }    // NmuhSchoolId (InstId) from external API
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public EGrade Grade { get; set; }        // ClassroomGrade
        public KtakPlace Place { get; set; }
    }
}
