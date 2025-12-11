using PordznakanAPI.Enums;
using Newtonsoft.Json;

namespace PordznakanAPI.DTOs
{
    public class TeacherSubjectDto
    {
        /// <summary>
 /// Id
 /// </summary>
 public Guid Id { get; set; }
 public Guid TeacherDtoId { get; set; }   // FK
 [JsonIgnore] // Ignore navigation property to prevent circular reference in JSON serialization
 public TeacherDto TeacherDto { get; set; } = null!;
 /// <summary>
 /// Առարկայի ID Ktak-ից
 /// </summary>
 public int SubjectId { get; set; }
 /// <summary>
 /// Դասարան
 /// </summary>
 public EGrade Grade { get; set; }
        /// <summary>
        /// Ենթադասարան (ա, բ ,գ) 
        /// </summary>
        public ESubGrade SubGrade { get; set; }
 /// <summary>
 /// Առարկայի անունը
 /// </summary>
 public string Name { get; set; } = string.Empty;
    }
}