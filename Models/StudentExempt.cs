namespace PordznakanAPI.Models
{
    public class StudentExempt
    {
        public int[] students { get; set; }
        public int place { get; set; }
        public int school_id { get; set; }
        public string? command_date { get; set; }
        public string command_number { get; set; }
        public ExemptReason reason { get; set; }
    }

    public class ExemptReason
    {
        public int id { get; set; }
        public string message { get; set; }
    }
}
