using PordznakanAPI.Enums;

namespace PordznakanAPI.Models
{
    public class TeacherSubject
    {
        public Guid Id { get; set; }
        public Guid TeacherId { get; set; }
        
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
        
        /// <summary>
        /// Classroom ID from Ktak
        /// </summary>
        public string ClassroomId { get; set; } = string.Empty;

        // Navigation property
        public Teacher? Teacher { get; set; }
    }
}