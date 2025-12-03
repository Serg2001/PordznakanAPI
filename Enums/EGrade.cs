using System.ComponentModel.DataAnnotations;

namespace PordznakanAPI.Enums
{
    /// <summary>
    /// Դասարան
    /// </summary>
    public enum EGrade : int
    {
        /// <summary>
        /// 1
        /// </summary>
        [Display(Name = "I")]
        G1 = 1,
        /// <summary>
        /// 2
        /// </summary>
        [Display(Name = "II")]
        G2 = 2,
        /// <summary>
        /// 3
        /// </summary>
        [Display(Name = "III")]
        G3 = 3,
        /// <summary>
        /// 4
        /// </summary>
        [Display(Name = "IV")]
        G4 = 4,
        /// <summary>
        /// 5
        /// </summary>
        [Display(Name = "V")]
        G5 = 5,
        /// <summary>
        /// 6
        /// </summary>
        [Display(Name = "VI")]
        G6 = 6,
        /// <summary>
        /// 7
        /// </summary>
        [Display(Name = "VII")]
        G7 = 7,
        /// <summary>
        /// 8
        /// </summary>
        [Display(Name = "VIII")]
        G8 = 8,
        /// <summary>
        /// 9
        /// </summary>
        [Display(Name = "IX")]
        G9 = 9,
        /// <summary>
        /// 10
        /// </summary>
        [Display(Name = "X")]
        G10 = 10,
        /// <summary>
        /// 11
        /// </summary>
        [Display(Name = "XI")]
        G11 = 11,
        /// <summary>
        /// 12
        /// </summary>
        [Display(Name = "XII")]
        G12 = 12
    }
}
