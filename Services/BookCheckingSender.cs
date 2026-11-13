using Newtonsoft.Json;
using System.Text;

namespace PordznakanAPI.Services
{
    /// <summary>
    /// Posts one student's has_book flag to EMIS (POST v1/save_book_checking).
    /// Called straight from the HasBook endpoints. One attempt only: a throw here is reported to the caller, never retried.
    /// </summary>
    public class BookCheckingSender
    {
        private const string Endpoint = "save_book_checking";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BookCheckingSender>? _logger;

        public BookCheckingSender(IHttpClientFactory httpClientFactory, ILogger<BookCheckingSender>? logger = null)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SendAsync(int studentId, string fullName, bool hasBook, CancellationToken cancellationToken)
        {
            // Shared EMIS client: base URL, X-Api-Token and retry handler come from Program.cs.
            var client = _httpClientFactory.CreateClient(EmisClients.GetAllData);

            var json = JsonConvert.SerializeObject(new
            {
                student_id = studentId,
                student_full_name = fullName,
                has_book = hasBook ? 1 : 0
            });

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(Endpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"save_book_checking failed for student {studentId}: HTTP {(int)response.StatusCode}. {body}");
            }

            _logger?.LogInformation($"save_book_checking sent for student {studentId}, has_book={(hasBook ? 1 : 0)}.");
        }
    }
}
