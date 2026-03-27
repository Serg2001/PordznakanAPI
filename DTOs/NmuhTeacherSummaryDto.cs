using PordznakanAPI.Enums;

namespace PordznakanAPI.DTOs
{
    public class NmuhTeacherSummaryDto
    {
        public Guid Id { get; set; }
        public int KtakTeacherId { get; set; }   // NmuhStaffId from external API
        public int KtakSchoolId { get; set; }    // InstId from external API
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public KtakPlace Place { get; set; }
    }
}
