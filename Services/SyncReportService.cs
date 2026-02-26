using Newtonsoft.Json;

namespace PordznakanAPI.Services
{
    public class SyncReportService : ISyncReportService
    {
        public T? ReadLatestReport<T>(string reportDirectory, string fileName) where T : class
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), reportDirectory, fileName);

            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
