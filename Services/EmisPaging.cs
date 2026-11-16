using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PordznakanAPI.Services
{
    /// <summary>
    /// Result of reading every page of a paginated EMIS getAllData response.
    /// </summary>
    public sealed class EmisPagedResult
    {
        /// <summary>All school objects of all pages, keyed by the school key. Pagination metadata is removed.</summary>
        public JObject Data { get; init; } = new();

        /// <summary>Number of HTTP pages actually fetched.</summary>
        public int PagesFetched { get; init; }

        /// <summary>"last_page" from EMIS (null when the response was not paginated).</summary>
        public int? LastPage { get; init; }

        /// <summary>"total" from EMIS: how many schools the region has in all (null when not sent).</summary>
        public int? Total { get; init; }

        /// <summary>"per_page" from EMIS: schools per page (null when not sent).</summary>
        public int? PerPage { get; init; }

        /// <summary>The same school key arrived on two pages (EMIS data shifted while we were reading).</summary>
        public int DuplicateKeys { get; init; }

        /// <summary>
        /// True when we received at least "total" schools. A false value means part of the region is
        /// missing, so callers must not treat the missing pupils as removed by EMIS.
        /// </summary>
        public bool Complete => Total is null || Data.Count >= Total.Value;
    }

    /// <summary>
    /// Result of reading every page of a paginated EMIS endpoint that returns a list of objects.
    /// </summary>
    public sealed class EmisPagedItems
    {
        public JArray Items { get; init; } = new();
        public int PagesFetched { get; init; }
        public int? LastPage { get; init; }
        public int? Total { get; init; }
        public int? PerPage { get; init; }

        /// <summary>True when we received at least "total" items (or EMIS sent no total).</summary>
        public bool Complete => Total is null || Items.Count >= Total.Value;
    }

    /// <summary>
    /// Reads a paginated EMIS endpoint. EMIS answers e.g. getAllData/5?page=1 with the schools of that
    /// page plus four top-level numbers:
    ///   page       - the page number of this response
    ///   per_page   - how many schools one page holds
    ///   total      - how many schools the whole region has
    ///   last_page  - the number of the last page (ceil(total / per_page))
    /// A response without these numbers (an older or unpaginated endpoint) is treated as a single page.
    /// </summary>
    public static class EmisPaging
    {
        private static readonly HashSet<string> MetaKeys =
            new(StringComparer.OrdinalIgnoreCase) { "page", "per_page", "total", "last_page" };

        // Hard stop against an endless loop if EMIS ever reports an absurd last_page.
        private const int MaxPages = 2000;

        public static async Task<EmisPagedResult> FetchAllPagesAsync(
            HttpClient client,
            string path,
            ILogger? logger = null,
            string logPrefix = "",
            CancellationToken cancellationToken = default)
        {
            var merged = new JObject();
            int? lastPage = null, total = null, perPage = null;
            var pagesFetched = 0;
            var duplicateKeys = 0;
            var page = 1;

            while (true)
            {
                var url = $"{path}{(path.Contains('?') ? '&' : '?')}page={page}";

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (body.Length > 500) body = body[..500] + "...";
                    throw new HttpRequestException(
                        $"EMIS {url} returned {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
                }

                // Parse straight off the stream; a page is never materialised as a string.
                // An empty region may come back as [] instead of {}; that is an empty page.
                JObject pageJson;
                using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var streamReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    pageJson = await JToken.LoadAsync(jsonReader, cancellationToken) as JObject ?? new JObject();
                }

                pagesFetched++;

                // EMIS now wraps the schools: { "data": { "<schoolKey>": {...}, ... }, <paging numbers> }.
                // The older shape had the school keys at the top level. Accept both. The paging
                // numbers may sit at the top level or inside a "meta" object.
                var schoolsNode = pageJson["data"] is JObject dataObj ? dataObj : pageJson;
                var metaObj = pageJson["meta"] as JObject;

                lastPage ??= ReadMeta(pageJson, metaObj, "last_page");
                total = ReadMeta(pageJson, metaObj, "total") ?? total;
                perPage = ReadMeta(pageJson, metaObj, "per_page") ?? perPage;

                if (lastPage is null && total is > 0 && perPage is > 0)
                    lastPage = (int)Math.Ceiling(total.Value / (double)perPage.Value);

                var returnedPage = ReadMeta(pageJson, metaObj, "page");
                if (returnedPage is not null && returnedPage != page)
                {
                    logger?.LogWarning(
                        $"{logPrefix} Asked EMIS for page {page} but it answered page {returnedPage}.");
                }

                var added = 0;
                foreach (var prop in schoolsNode.Properties())
                {
                    // Only school objects are data; page/per_page/total/last_page are metadata.
                    if (MetaKeys.Contains(prop.Name) || prop.Value.Type != JTokenType.Object)
                        continue;

                    // A school key that already arrived on another page is REPLACED, never merged:
                    // merging would concatenate its students and store every pupil twice.
                    if (merged.ContainsKey(prop.Name))
                        duplicateKeys++;

                    merged[prop.Name] = prop.Value.DeepClone();
                    added++;
                }

                logger?.LogInformation(
                    $"{logPrefix} EMIS page {page}/{lastPage?.ToString() ?? "?"}: {added} schools " +
                    $"({merged.Count}{(total is null ? "" : $" of {total}")} so far).");

                if (added == 0 && page == 1)
                {
                    // Make a silently empty answer visible: show which top-level keys EMIS sent.
                    var keys = string.Join(", ", pageJson.Properties().Select(p => $"{p.Name}:{p.Value.Type}").Take(15));
                    logger?.LogWarning(
                        $"{logPrefix} EMIS page 1 contained no school objects. Top-level keys: [{keys}].");
                }

                // Not paginated, or this was the last page.
                if (lastPage is null || page >= lastPage.Value)
                    break;

                if (added == 0)
                {
                    logger?.LogWarning(
                        $"{logPrefix} EMIS page {page} contained no schools although last_page is {lastPage}; stopping.");
                    break;
                }

                if (page >= MaxPages)
                    throw new InvalidOperationException(
                        $"EMIS {path} reports last_page={lastPage}, which is more than the {MaxPages}-page safety limit.");

                page++;
            }

            if (duplicateKeys > 0)
            {
                logger?.LogWarning(
                    $"{logPrefix} {duplicateKeys} school key(s) arrived on more than one page; the later copy was kept.");
            }

            return new EmisPagedResult
            {
                Data = merged,
                PagesFetched = pagesFetched,
                LastPage = lastPage,
                Total = total,
                PerPage = perPage,
                DuplicateKeys = duplicateKeys
            };
        }

        /// <summary>
        /// Same page loop for endpoints that answer with a list of objects (get_personnel,
        /// get_personnel_list) instead of an object keyed by school/institution. Each page may be a
        /// bare array (not paginated, one page) or an object carrying page/per_page/total/last_page
        /// next to the array (under data/results/items/teachers or any other array property).
        /// </summary>
        public static async Task<EmisPagedItems> FetchAllItemsAsync(
            HttpClient client,
            string path,
            ILogger? logger = null,
            string logPrefix = "",
            CancellationToken cancellationToken = default)
        {
            var items = new JArray();
            int? lastPage = null, total = null, perPage = null;
            var pagesFetched = 0;
            var page = 1;

            while (true)
            {
                var url = $"{path}{(path.Contains('?') ? '&' : '?')}page={page}";

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (body.Length > 500) body = body[..500] + "...";
                    throw new HttpRequestException(
                        $"EMIS {url} returned {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
                }

                JToken pageToken;
                using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var streamReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    pageToken = await JToken.LoadAsync(jsonReader, cancellationToken);
                }

                pagesFetched++;

                JArray? pageItems = pageToken as JArray;
                if (pageToken is JObject obj)
                {
                    lastPage ??= ReadInt(obj, "last_page");
                    total = ReadInt(obj, "total") ?? total;
                    perPage = ReadInt(obj, "per_page") ?? perPage;

                    if (lastPage is null && total is > 0 && perPage is > 0)
                        lastPage = (int)Math.Ceiling(total.Value / (double)perPage.Value);

                    var returnedPage = ReadInt(obj, "page");
                    if (returnedPage is not null && returnedPage != page)
                    {
                        logger?.LogWarning(
                            $"{logPrefix} Asked EMIS for page {page} but it answered page {returnedPage}.");
                    }

                    pageItems = obj["data"] as JArray
                             ?? obj["results"] as JArray
                             ?? obj["items"] as JArray
                             ?? obj["teachers"] as JArray
                             ?? obj.Properties().Select(p => p.Value).OfType<JArray>().FirstOrDefault();

                    if (pageItems is null)
                    {
                        var error = obj["error"]?.ToString() ?? obj["message"]?.ToString() ?? obj["ErrorMessage"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(error))
                            throw new InvalidOperationException($"EMIS {url} returned an error: {error}");

                        // A lone person object, or an empty page without a list.
                        pageItems = obj["person_id"] != null ? new JArray(obj) : new JArray();
                    }
                }
                else if (pageItems is null)
                {
                    throw new InvalidOperationException($"EMIS {url} returned {pageToken.Type}; expected an array or an object.");
                }

                foreach (var item in pageItems)
                    items.Add(item.DeepClone());

                logger?.LogInformation(
                    $"{logPrefix} EMIS page {page}/{lastPage?.ToString() ?? "?"}: {pageItems.Count} items " +
                    $"({items.Count}{(total is null ? "" : $" of {total}")} so far).");

                if (lastPage is null || page >= lastPage.Value)
                    break;

                if (pageItems.Count == 0)
                {
                    logger?.LogWarning(
                        $"{logPrefix} EMIS page {page} was empty although last_page is {lastPage}; stopping.");
                    break;
                }

                if (page >= MaxPages)
                    throw new InvalidOperationException(
                        $"EMIS {path} reports last_page={lastPage}, which is more than the {MaxPages}-page safety limit.");

                page++;
            }

            return new EmisPagedItems
            {
                Items = items,
                PagesFetched = pagesFetched,
                LastPage = lastPage,
                Total = total,
                PerPage = perPage
            };
        }

        private static int? ReadMeta(JObject json, JObject? meta, string name) =>
            ReadInt(json, name) ?? (meta is null ? null : ReadInt(meta, name));

        private static int? ReadInt(JObject json, string name)
        {
            var token = json[name];
            if (token is null || token.Type == JTokenType.Null)
                return null;

            return int.TryParse(token.ToString(), out var value) ? value : null;
        }
    }
}
