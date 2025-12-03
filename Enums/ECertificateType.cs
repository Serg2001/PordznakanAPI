using System.ComponentModel.DataAnnotations;

namespace PordznakanAPI.Enums
{
    /// <summary>
    /// Սովորողների փաստաթղթի տեսակը
    /// </summary>
    public enum ECertificateType : int
    {
        [Display(Name = "Անհայտ")]
        Unknown = 0,
        [Display(Name = "Այլ փաստաթուղթ")]
        Other = 1,  /*| 1018*/
        [Display(Name = "Ծննդյան վկայական")]
        HHCertificate = 2,  /*| 1017*/
        [Display(Name = "ՀՀ անձնագիր")]
        HHPasport = 3,  /*| 973*/
        [Display(Name = "ՀՀ սոցքարտ")]
        HHSocNumber = 4,
        [Display(Name = "Նույնականացման քարտ")]
        IDCard = 974,
        [Display(Name = "Փախստականի անձնագիր")]
        RefugeePassport = 975,
        [Display(Name = "Քաղաքացիություն չունեցողի անձնագիր")]
        StatelessPasport = 976,
        [Display(Name = "ՀՀ կենսաչափական անձնագիր")]
        BiometricPassport = 1013,
        [Display(Name = "Օտարերկրյա փաստաթուղթ")]
        ForeignDocument = 1014,
        [Display(Name = "Կոնվենցիոն փաստաթուղթ")]
        ConventionCard = 1015,
        [Display(Name = "Կացության քարտ")]
        ResidenceCard = 1016,
        [Display(Name = "Ճամփորդական փաստաթուղթ")]
        TravelDocument = 1061
    }
}


