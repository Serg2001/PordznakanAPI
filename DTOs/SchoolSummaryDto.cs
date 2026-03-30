using PordznakanAPI.Enums;

namespace PordznakanAPI.DTOs
{
    public class SchoolSummaryDto
    {
        public Guid DshhSchoolId { get; set; }
        public int KtakSchoolId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public KtakPlace Place { get; set; }
    }
}
