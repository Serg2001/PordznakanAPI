using System.Text.Json.Serialization;
using PordznakanAPI.Enums;

namespace PordznakanAPI.DTOs
{
    /// <summary>Short teacher card used in the roster responses.</summary>
    public class TeacherBriefDto
    {
        public Guid Id { get; set; }
        public int KtakTeacherId { get; set; }
        public int KtakSchoolId { get; set; }
        public int RegionId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string MainSubject { get; set; } = string.Empty;
    }

    /// <summary>Short pupil card used in the roster responses.</summary>
    public class PupilBriefDto
    {
        public Guid Id { get; set; }
        public int KtakPupilId { get; set; }
        public int KtakSchoolId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public EGrade Grade { get; set; }
        public ESubGrade SubGrade { get; set; }
        public EPupilStatus Status { get; set; }
    }

    /// <summary>Class (classroom) card used in the roster responses.</summary>
    public class ClassBriefDto
    {
        // Negative order: the class fields are written first, before the fields of derived
        // types (subjects, pupils), which would otherwise come out ahead of them.
        [JsonPropertyOrder(-10)] public Guid Id { get; set; }
        [JsonPropertyOrder(-9)] public string KtakClassroomId { get; set; } = string.Empty;
        [JsonPropertyOrder(-8)] public int KtakSchoolId { get; set; }
        [JsonPropertyOrder(-7)] public string Grade { get; set; } = string.Empty;
        [JsonPropertyOrder(-6)] public string Classifier { get; set; } = string.Empty;
        [JsonPropertyOrder(-5)] public string ClassName { get; set; } = string.Empty;
        [JsonPropertyOrder(-4)] public string? Stream { get; set; }
    }

    /// <summary>A class the teacher teaches, with its pupils.</summary>
    public class TeacherClassDto : ClassBriefDto
    {
        /// <summary>Names of the subjects this teacher teaches in this class.</summary>
        public List<string> Subjects { get; set; } = new();
        public int PupilCount { get; set; }
        public List<PupilBriefDto> Pupils { get; set; } = new();
    }

    public class TeacherClassesResponse
    {
        public TeacherBriefDto Teacher { get; set; } = null!;
        public List<TeacherClassDto> Classes { get; set; } = new();
    }

    /// <summary>One subject a teacher teaches, with the grades it is taught in.</summary>
    public class TeacherSubjectItemDto
    {
        /// <summary>Subject id in KTAK/EMIS (the data holds no separate subject code).</summary>
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<SubjectGradeDto> Grades { get; set; } = new();
    }

    public class SubjectGradeDto
    {
        public EGrade Grade { get; set; }
        public ESubGrade SubGrade { get; set; }
    }

    public class TeacherSubjectsResponse
    {
        public TeacherBriefDto Teacher { get; set; } = null!;
        public List<TeacherSubjectItemDto> Subjects { get; set; } = new();
    }

    public class ClassmatesResponse
    {
        public PupilBriefDto Pupil { get; set; } = null!;

        /// <summary>The pupil's class; null when the class is not (yet) synced.</summary>
        public ClassBriefDto? Class { get; set; }

        public int ClassmateCount { get; set; }
        public List<PupilBriefDto> Classmates { get; set; } = new();
    }
}
