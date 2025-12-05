using PordznakanAPI.Enums;

namespace PordznakanAPI.DTOs
{
    public class TeacherSubjectDto
    {
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