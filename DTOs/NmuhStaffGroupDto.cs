namespace PordznakanAPI.DTOs
{
    public class NmuhSubjectDto
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectType { get; set; } = string.Empty;
        public int SubjectTypeId { get; set; }
    }

    public class NmuhStaffGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public List<NmuhSubjectDto> Subjects { get; set; } = new();
    }
}
