using PordznakanAPI.Enums;

namespace PordznakanAPI.Models
{
    public class TeacherSubject
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Foreign key to Teacher
        /// </summary>
        public Guid TeacherId { get; set; }
        
        /// <summary>
        /// Navigation property to Teacher
        /// </summary>
        public Teacher Teacher { get; set; } = null!;
        
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