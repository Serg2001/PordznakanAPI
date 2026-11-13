using System.Text.Json.Serialization;

namespace PordznakanAPI.DTOs
{
    /// <summary>
    /// One (pupil_id, hasBook) pair sent by the external caller.
    /// </summary>
    public class HasBookUpdateItemDto
    {
        [JsonPropertyName("pupil_id")]
        public int PupilId { get; set; }

        [JsonPropertyName("hasBook")]
        public bool HasBook { get; set; }
    }

    /// <summary>
    /// Result of a batch HasBook update: how many rows were changed, and which
    /// requested pupil_id values did not match any record.
    /// </summary>
    public class HasBookUpdateResultDto
    {
        public int Updated { get; set; }
        public List<int> NotFound { get; set; } = new();

        /// <summary>Students whose value EMIS accepted before the response was returned.</summary>
        public int SentToEmis { get; set; }

        /// <summary>
        /// pupil_id values EMIS did not accept (error or no answer in time). Their HasBook is saved
        /// in the database but not sent; they are never retried automatically, so the caller
        /// should send these again.
        /// </summary>
        public List<int> FailedToSend { get; set; } = new();
    }
}
