using System.ComponentModel.DataAnnotations;

namespace PordznakanAPI.Enums
{
    /// <summary>
    /// Ենթադասարան
    /// </summary>
    public enum ESubGrade : int
    {
        /// <summary>
        /// Անհայտ
        /// </summary>
        [Display(Name = "Չնշված")]
        Unknown = 0,
        /// <summary>
        /// Ա ենթադասարան
        /// </summary>
        [Display(Name = "ա")]
        Sg1 = 1,
        /// <summary>
        /// Բ ենթադասարան
        /// </summary>
        [Display(Name = "բ")]
        Sg2,
        /// <summary>
        /// Գ ենթադասարան
        /// </summary>
        [Display(Name = "գ")]
        Sg3,
        /// <summary>
        /// Դ ենթադասարան
        /// </summary>
        [Display(Name = "դ")]
        Sg4,
        /// <summary>
        /// Ե ենթադասարան
        /// </summary>
        [Display(Name = "ե")]
        Sg5,
        /// <summary>
        /// Զ ենթադասարան
        /// </summary>
        [Display(Name = "զ")]
        Sg6,
        /// <summary>
        /// Է ենթադասարան
        /// </summary>
        [Display(Name = "է")]
        Sg7,
        /// <summary>
        /// Ը ենթադասարան
        /// </summary>
        [Display(Name = "ը")]
        Sg8,
        /// <summary>
        /// Թ ենթադասարան
        /// </summary>
        [Display(Name = "թ")]
        Sg9,
        /// <summary>
        /// Ժ ենթադասարան
        /// </summary>
        [Display(Name = "ժ")]
        Sg10,
        /// <summary>
        /// Ի ենթադասարան
        /// </summary>
        [Display(Name = "ի")]
        Sg11,
        /// <summary>
        /// Լ ենթադասարան
        /// </summary>
        [Display(Name = "լ")]
        Sg12,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "խ")]
        Sg13,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "ծ")]
        Sg14,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "կ")]
        Sg15,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "հ")]
        Sg16,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "ձ")]
        Sg17,
        /// <summary>
        /// Ռ
        /// </summary>
        [Display(Name = "ռ")]
        Sg18,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "-*-")]
        Sg19, // 12 dasaran 2 kisamyakum
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "ռ1")]
        Sg28 = 28,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "ռ2")]
        Sg29 = 29,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "ռ3")]
        Sg30 = 30,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "ռ4")]
        Sg31 = 31,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "ռ5")]
        Sg32 = 32,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "փ1")]
        Sg33 = 33,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "փ2")]
        Sg34 = 34,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "փ3")]
        Sg35 = 35
    }
}

