namespace PordznakanAPI.DTOs
{
    public class SchoolChangeDto
    {
        public Guid DshhSchoolId { get; set; }
        public string KtakSchoolId { get; set; } = string.Empty;
        public int RegionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Marz { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Community { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }

    public class ClassroomChangeDto
    {
        public Guid Id { get; set; }
        public Guid SchoolId { get; set; }
        public string KtakSchoolId { get; set; } = string.Empty;
        public string KtakClassroomId { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Classifier { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string? Stream { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class PupilChangeDto
    {
        public Guid Id { get; set; }
        public string KtakPupilId { get; set; } = string.Empty;
        public string KtakSchoolId { get; set; } = string.Empty;
        public string ClassroomId { get; set; } = string.Empty;
        public Guid? ClassroomInternalId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string IdentDocument { get; set; } = string.Empty;
        public string IdentDocumentNumber { get; set; } = string.Empty;
        public string FromCountry { get; set; } = string.Empty;
        public string SocNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Sex { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }

    public class SyncChangesSummaryDto
    {
        public DateTime SyncCompletedAt { get; set; }
        public int TotalRegionsProcessed { get; set; }
        public int SuccessfulRegions { get; set; }
        public int FailedRegions { get; set; }
        public int TotalSchoolsAdded { get; set; }
        public int TotalSchoolsUpdated { get; set; }
        public int TotalClassroomsAdded { get; set; }
        public int TotalClassroomsUpdated { get; set; }
        public int TotalPupilsAdded { get; set; }
        public int TotalPupilsUpdated { get; set; }
        public List<SchoolDto> AllSchoolsUpdated { get; set; } = new();
        public List<ClassroomDto> AllClassroomsUpdated { get; set; } = new();
        public List<PupilDto> AllPupilsUpdated { get; set; } = new();
    }
}

