using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PordznakanAPI.Data;
using PordznakanAPI.Services;
using System.Text;

namespace PordznakanAPI.Controllers
{
    public class BookCheckingSourceResult
    {
        public string Source { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Sent { get; set; }
        public int Failed { get; set; }
        public bool Aborted { get; set; }   // stopped early because EMIS kept failing
    }

    /// <summary>
    /// Pushes the "has the student received the book" flag to EMIS
    /// (POST https://data-api.emis.am/v1/save_book_checking), one request per student,
    /// for every row of Pupils. Only runs when someone calls POST api/BookChecking/send by hand;
    /// there is no scheduled run, because each HasBook change is sent as it happens.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookCheckingController : ControllerBase
    {
        private const string Endpoint = "save_book_checking";

        // If this many requests in a row fail, EMIS is down or rejecting us (bad token, maintenance);
        // going on would only hold the job (and its lock) for hours while every student retries.
        private const int MaxConsecutiveFailures = 20;

        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BookCheckingController>? _logger;

        public BookCheckingController(AppDbContext context, IHttpClientFactory httpClientFactory, ILogger<BookCheckingController>? logger = null)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // Only the three values EMIS needs.
        private sealed record BookRow(int StudentId, string FullName, bool HasBook);

        private static string FullName(string lastName, string firstName, string fatherName) =>
            string.Join(" ", new[] { lastName, firstName, fatherName }.Where(s => !string.IsNullOrWhiteSpace(s)));

        [NonAction]
        public async Task<List<BookCheckingSourceResult>> SendAll(CancellationToken cancellationToken = default)
        {
            // The shared EMIS client already carries the base URL, X-Api-Token and retry handler (see Program.cs).
            var client = _httpClientFactory.CreateClient(EmisClients.GetAllData);
            var results = new List<BookCheckingSourceResult>();

            var pupils = (await _context.Pupils.AsNoTracking()
                    .Select(p => new { p.KtakPupilId, p.LastName, p.FirstName, p.FatherName, p.HasBook })
                    .ToListAsync(cancellationToken))
                .Select(p => new BookRow(p.KtakPupilId, FullName(p.LastName, p.FirstName, p.FatherName), p.HasBook))
                .ToList();
            results.Add(await SendRows("Pupil", pupils, client, cancellationToken));

            return results;
        }

        private async Task<BookCheckingSourceResult> SendRows(
            string source, List<BookRow> rows, HttpClient client, CancellationToken cancellationToken)
        {
            var result = new BookCheckingSourceResult { Source = source, Total = rows.Count };

            // One request per student, sequentially, so a failure never blocks the others.
            var consecutiveFailures = 0;
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (consecutiveFailures >= MaxConsecutiveFailures)
                {
                    result.Aborted = true;
                    _logger?.LogError($"[{source}] save_book_checking aborted after {consecutiveFailures} failures in a row.");
                    break;
                }

                try
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        student_id = row.StudentId,
                        student_full_name = row.FullName,
                        has_book = row.HasBook ? 1 : 0
                    });

                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    using var response = await client.PostAsync(Endpoint, content, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        result.Sent++;
                        consecutiveFailures = 0;
                    }
                    else
                    {
                        result.Failed++;
                        consecutiveFailures++;
                        var body = await response.Content.ReadAsStringAsync(cancellationToken);
                        _logger?.LogWarning($"[{source}] save_book_checking failed for student {row.StudentId}: " +
                            $"HTTP {(int)response.StatusCode}. {body}");
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
                {
                    result.Failed++;
                    consecutiveFailures++;
                    _logger?.LogError(ex, $"[{source}] save_book_checking threw for student {row.StudentId}");
                }
            }

            _logger?.LogInformation($"[{source}] save_book_checking done. Total={result.Total}, Sent={result.Sent}, Failed={result.Failed}");
            return result;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send(CancellationToken cancellationToken)
        {
            var results = await SendAll(cancellationToken);

            return Ok(new
            {
                message = "Book checking sent to EMIS.",
                sources = results,
                totalSent = results.Sum(r => r.Sent),
                totalFailed = results.Sum(r => r.Failed)
            });
        }
    }
}
