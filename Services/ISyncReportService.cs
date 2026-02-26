namespace PordznakanAPI.Services
{
    public interface ISyncReportService
    {
        /// <summary>
        /// Reads and deserializes the latest sync report JSON file.
        /// Returns null if the file does not exist or cannot be parsed.
        /// </summary>
        T? ReadLatestReport<T>(string reportDirectory, string fileName) where T : class;
    }
}
