using System.Text.Json.Serialization;

namespace PordznakanAPI.Models
{
    public class StudentHierResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("school_id")]
        public int SchoolId { get; set; }

        [JsonPropertyName("action_date")]
        public DateTime ActionDate { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("sent")]
        public StudentHierSent? Sent { get; set; }

        [JsonPropertyName("received")]
        public StudentHierReceived? Received { get; set; }
    }

    public class StudentHierSent
    {
        [JsonPropertyName("place")]
        public int Place { get; set; }

        [JsonPropertyName("student_id")]
        public int StudentId { get; set; }

        [JsonPropertyName("school_id")]
        public string SchoolId { get; set; } = string.Empty;

        [JsonPropertyName("education_year")]
        public int EducationYear { get; set; }

        [JsonPropertyName("command_date")]
        public string CommandDate { get; set; } = string.Empty;

        [JsonPropertyName("command_number")]
        public string CommandNumber { get; set; } = string.Empty;

        [JsonPropertyName("class_number")]
        public string ClassNumber { get; set; } = string.Empty;

        [JsonPropertyName("classifier")]
        public string Classifier { get; set; } = string.Empty;

        [JsonPropertyName("personal_info")]
        public PersonalInfo? PersonalInfo { get; set; }
    }

    public class PersonalInfo
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("father_name")]
        public string FatherName { get; set; } = string.Empty;

        [JsonPropertyName("soc_number")]
        public string SocNumber { get; set; } = string.Empty;

        [JsonPropertyName("date_of_birth")]
        public string DateOfBirth { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("guardian_phone")]
        public string GuardianPhone { get; set; } = string.Empty;

        [JsonPropertyName("sex")]
        public string Sex { get; set; } = string.Empty; // "48" in your example - might be enum value

        [JsonPropertyName("invalid")]
        public int Invalid { get; set; }
    }

    public class StudentHierReceived
    {
        [JsonPropertyName("$id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; } = string.Empty;
    }
}








