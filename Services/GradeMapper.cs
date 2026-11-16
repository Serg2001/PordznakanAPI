using PordznakanAPI.Enums;

namespace PordznakanAPI.Services
{
    /// <summary>
    /// Maps the grade / classifier text EMIS sends ("5", "ա") onto our enums. Shared by the syncs
    /// and by the roster lookups, which must agree on what a class "5ա" is.
    /// </summary>
    public static class GradeMapper
    {
        public static EGrade MapGrade(string? grade)
        {
            if (int.TryParse(grade, out var g) && g >= 1 && g <= 12)
                return (EGrade)g;

            return 0;
        }

        public static ESubGrade MapSubGrade(string? classifier)
        {
            if (string.IsNullOrWhiteSpace(classifier))
                return ESubGrade.Unknown;

            // EMIS may send the letter in upper case ("Ռ"); the cases below are lower case.
            return classifier.Trim().ToLowerInvariant() switch
            {
                "ա" => ESubGrade.Sg1,
                "բ" => ESubGrade.Sg2,
                "գ" => ESubGrade.Sg3,
                "դ" => ESubGrade.Sg4,
                "ե" => ESubGrade.Sg5,
                "զ" => ESubGrade.Sg6,
                "է" => ESubGrade.Sg7,
                "ը" => ESubGrade.Sg8,
                "թ" => ESubGrade.Sg9,
                "ժ" => ESubGrade.Sg10,
                "ի" => ESubGrade.Sg11,
                "լ" => ESubGrade.Sg12,
                "խ" => ESubGrade.Sg13,
                "ծ" => ESubGrade.Sg14,
                "կ" => ESubGrade.Sg15,
                "հ" => ESubGrade.Sg16,
                "ձ" => ESubGrade.Sg17,
                "ռ" => ESubGrade.Sg18,
                _ => ESubGrade.Unknown
            };
        }
    }
}
