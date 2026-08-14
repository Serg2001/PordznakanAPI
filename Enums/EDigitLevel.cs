using System.ComponentModel.DataAnnotations;

namespace PordznakanAPI.Enums
{
    /// <summary>
    /// Տարակարգ (Կրթական աստիճան)
    /// </summary>
    public enum EDigitLevel : int
    {
        /// <summary>
        /// Անհայտ
        /// </summary>
        [Display(Name = "Անհայտ")]
        Unknown = 0,
        /// <summary>
        /// Առաջին 341
        /// </summary>
        [Display(Name = "Առաջին")]
        D1 = 1,
        /// <summary>
        /// Երկրորդ 342
        /// </summary>
        [Display(Name = "Երկրորդ")]
        D2 = 2,
        /// <summary>
        /// Երրորդ 344
        /// </summary>
        [Display(Name = "Երրորդ")]
        D3 = 3,
        /// <summary>
        /// Չորրորդ 346
        /// </summary>
        [Display(Name = "Չորրորդ")]
        D4 = 4
    }
}
