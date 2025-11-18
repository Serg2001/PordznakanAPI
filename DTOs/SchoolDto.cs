using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace PordznakanAPI.DTOs
{
    public class SchoolDto
    {
        [JsonProperty("schools_id")]
        public string SchoolsId { get; set; } = string.Empty;

        [JsonProperty("school_name")]
        public string SchoolName { get; set; } = string.Empty;

        [JsonProperty("marz")]
        public string Marz { get; set; } = string.Empty;

        [JsonProperty("region")]
        public string Region { get; set; } = string.Empty;

        [JsonProperty("community")]
        public string Community { get; set; } = string.Empty;

        //// API returns director as escaped JSON string
        //[JsonProperty("director")]
        //public string Director { get; set; } = string.Empty;

        //[JsonProperty("classrooms")]
        //public Dictionary<string, ClassroomDto>? Classrooms { get; set; }

        //// Parsed manually from Director string
        //[JsonIgnore]
        //public List<DirectorDto> ParsedDirectors { get; set; } = new();
    }
}
