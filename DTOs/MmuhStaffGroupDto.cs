namespace PordznakanAPI.DTOs
{
    public class MmuhSubjectDto
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectType { get; set; } = string.Empty;
        public int SubjectTypeId { get; set; }
    }

    public class MmuhStaffGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public List<MmuhSubjectDto> Subjects { get; set; } = new();
    }
}
