using PordznakanAPI.Enums;
using System.Security.Cryptography;
using System.Text;

namespace PordznakanAPI.Services
{
    public static class SyncHelpers
    {
        /// <summary>
        /// Joins all fields with '|' and returns their MD5 hash as an uppercase hex string.
        /// Pass null-safe string values; nulls are treated as empty string.
        /// </summary>
        public static string ComputeMd5(params string?[] fields)
        {
            var raw = string.Join('|', fields.Select(f => f ?? string.Empty));
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// Parses a numeric grade string (1-12) into an EGrade enum value.
        /// Returns 0 for null, empty, or out-of-range values.
        /// </summary>
        public static EGrade MapGrade(string? grade)
        {
            if (int.TryParse(grade, out var g) && g >= 1 && g <= 12)
                return (EGrade)g;
            return 0;
        }

        /// <summary>
        /// Parses a graduated flag from API strings ("1", "true") into a bool.
        /// </summary>
        public static bool MapGraduated(string? graduated)
        {
            if (string.IsNullOrWhiteSpace(graduated)) return false;
            var v = graduated.Trim();
            return v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
