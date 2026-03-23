using PordznakanAPI.Enums;

namespace PordznakanAPI.DTOs
{
    public class PupilDto
    {
        public Guid Id { get; set; }
        public int KtakPupilId { get; set; }
        public int KtakSchoolId { get; set; }
        public int RegionId { get; set; }
        public string ClassroomId { get; set; } = string.Empty;
        public Guid? ClassroomInternalId { get; set; }
        public KtakPlace Place { get; set; }
        public EGrade Grade { get; set; }
        public ESubGrade SubGrade { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public ECertificateType CertificateType { get; set; }
        public string Certificate { get; set; } = string.Empty;
        public DateOnly Birthday { get; set; }
        public bool Gender { get; set; }
        public EPupilStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
