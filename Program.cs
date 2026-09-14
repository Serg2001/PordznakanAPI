using PordznakanAPI;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PordznakanAPI.Data;
using PordznakanAPI.Services;
using Hangfire;
using Hangfire.SqlServer;
using PordznakanAPI.Controllers;
using PordznakanAPI.Filters;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// When Integration:RequireApiKey is true every endpoint demands the X-API-KEY header
// (see Filters/ApiKeyAuthAttribute.cs). Turn it on only once every caller sends the key,
// otherwise existing consumers start getting 401.
builder.Services.AddControllers(options =>
{
    if (builder.Configuration.GetValue("Integration:RequireApiKey", false))
        options.Filters.Add<ApiKeyAuthAttribute>();
});
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(200)
    );
});

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

builder.Services.AddHttpClient("ktakapi", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://crmapi.dshh.am/api/integration/");
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    };
});

// Client for data-api.emis.am/v1/getAllData/{regionId} ONLY — the pupil/school/classroom
// feed used by PupilController. Every other EMIS endpoint in this project deliberately
// keeps its own HttpClient and its existing host; nothing here affects them.
//
// That endpoint builds each region on demand and is slow: time-to-first-byte for the
// smallest region is 25-50 seconds, and a busy server answers 500 with a maintenance
// body instead of waiting. Hence the generous per-attempt budget plus retries.
builder.Services.AddHttpClient(EmisClients.GetAllData, httpClient =>
{
    var emis = builder.Configuration.GetSection("EmisApi");
    var baseUrl = emis["BaseUrl"] ?? "https://data-api.emis.am/v1/";
    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";

    httpClient.BaseAddress = new Uri(baseUrl);
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

    var apiToken = emis["ApiToken"];
    if (!string.IsNullOrWhiteSpace(apiToken))
        httpClient.DefaultRequestHeaders.Add("X-Api-Token", apiToken);

    // Overall budget: must cover every attempt EmisRetryHandler makes plus the waits
    // between them, otherwise the last attempts could never start.
    httpClient.Timeout = TimeSpan.FromMinutes(emis.GetValue<double?>("TotalTimeoutMinutes") ?? 45);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // getAllData/1 is 4.7 MB raw but only 0.6 MB gzipped, and the bigger regions scale
    // with it, so ask for compression and decompress transparently.
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
})
.AddHttpMessageHandler<EmisRetryHandler>();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddTransient<EmisRetryHandler>();
builder.Services.AddScoped<ILogTransferService, LogTransferService>();
builder.Services.AddScoped<ISyncReportService, SyncReportService>();

var app = builder.Build();

// === Apply database migrations on startup ===
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // This will apply any pending migrations and create Hangfire tables if needed
        await dbContext.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying database migrations.");
        // Optional: throw; // if you want the app to crash on migration failure
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});

// Schedule recurring jobs — all times are Armenia (UTC+4), 5 minutes apart from 00:01
// UTC fallback = Armenia time minus 4 hours
try
{
    var timezone = TimeZoneInfo.FindSystemTimeZoneById("Caucasus Standard Time");

    RecurringJob.AddOrUpdate<PupilController>(
        "sync-pupils-daily",
        c => c.SyncAllRegions(),
        "1 0 * * *",   // 00:01 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<TeacherController>(
        "sync-teachers-daily",
        c => c.SyncAllRegions(),
        "6 0 * * *",   // 00:06 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<SchoolEmployeeController>(
        "sync-school-employees-daily",
        c => c.SyncAllRegions(),
        "11 0 * * *",  // 00:11 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<MmuhStudentController>(
        "sync-mmuh-students-daily",
        c => c.SyncAllRegions(),
        "16 0 * * *",  // 00:16 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<MmuhStaffController>(
        "sync-mmuh-staff-daily",
        c => c.SyncAllRegions(),
        "21 0 * * *",  // 00:21 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<NmuhStudentController>(
        "sync-nmuh-students-daily",
        c => c.SyncAllRegions(),
        "26 0 * * *",  // 00:26 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<NmuhStaffController>(
        "sync-nmuh-staff-daily",
        c => c.SyncAllRegions(),
        "31 0 * * *",  // 00:31 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<LogEmployeeController>(
        "process-log-employee-daily",
        c => c.ProcessAllRegions(),
        "36 0 * * *",  // 00:36 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<LogStudentController>(
        "process-log-student-daily",
        c => c.ProcessAllRegions(),
        "41 0 * * *",  // 00:41 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<LogMmuhEmployeeController>(
        "process-log-mmuh-employee-daily",
        c => c.ProcessAllRegions(),
        "46 0 * * *",  // 00:46 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<LogMmuhStudentController>(
        "process-log-mmuh-student-daily",
        c => c.ProcessAllRegions(),
        "51 0 * * *",  // 00:51 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<LogNmuhStudentController>(
        "process-log-nmuh-student-daily",
        c => c.ProcessAllRegions(),
        "56 0 * * *",  // 00:56 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<LogNmuhEmployeeController>(
        "process-log-nmuh-employee-daily",
        c => c.ProcessAllRegions(),
        "1 1 * * *",   // 01:01 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<MmuhInstitutionController>(
        "sync-mmuh-institutions-daily",
        c => c.SyncAllRegions(),
        "6 1 * * *",   // 01:06 Armenia
        new RecurringJobOptions { TimeZone = timezone });

    RecurringJob.AddOrUpdate<NmuhInstitutionController>(
        "sync-nmuh-institutions-daily",
        c => c.SyncAllRegions(),
        "11 1 * * *",  // 01:11 Armenia
        new RecurringJobOptions { TimeZone = timezone });
}
catch (TimeZoneNotFoundException)
{
    app.Logger.LogWarning("Caucasus Standard Time not found, falling back to UTC.");

    RecurringJob.AddOrUpdate<PupilController>(
        "sync-pupils-daily",
        c => c.SyncAllRegions(),
        "1 20 * * *",  // 20:01 UTC = 00:01 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<TeacherController>(
        "sync-teachers-daily",
        c => c.SyncAllRegions(),
        "6 20 * * *",  // 20:06 UTC = 00:06 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<SchoolEmployeeController>(
        "sync-school-employees-daily",
        c => c.SyncAllRegions(),
        "11 20 * * *", // 20:11 UTC = 00:11 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<MmuhStudentController>(
        "sync-mmuh-students-daily",
        c => c.SyncAllRegions(),
        "16 20 * * *", // 20:16 UTC = 00:16 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<MmuhStaffController>(
        "sync-mmuh-staff-daily",
        c => c.SyncAllRegions(),
        "21 20 * * *", // 20:21 UTC = 00:21 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<NmuhStudentController>(
        "sync-nmuh-students-daily",
        c => c.SyncAllRegions(),
        "26 20 * * *", // 20:26 UTC = 00:26 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<NmuhStaffController>(
        "sync-nmuh-staff-daily",
        c => c.SyncAllRegions(),
        "31 20 * * *", // 20:31 UTC = 00:31 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<LogEmployeeController>(
        "process-log-employee-daily",
        c => c.ProcessAllRegions(),
        "36 20 * * *", // 20:36 UTC = 00:36 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<LogStudentController>(
        "process-log-student-daily",
        c => c.ProcessAllRegions(),
        "41 20 * * *", // 20:41 UTC = 00:41 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<LogMmuhEmployeeController>(
        "process-log-mmuh-employee-daily",
        c => c.ProcessAllRegions(),
        "46 20 * * *", // 20:46 UTC = 00:46 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<LogMmuhStudentController>(
        "process-log-mmuh-student-daily",
        c => c.ProcessAllRegions(),
        "51 20 * * *", // 20:51 UTC = 00:51 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<LogNmuhStudentController>(
        "process-log-nmuh-student-daily",
        c => c.ProcessAllRegions(),
        "56 20 * * *", // 20:56 UTC = 00:56 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<LogNmuhEmployeeController>(
        "process-log-nmuh-employee-daily",
        c => c.ProcessAllRegions(),
        "1 21 * * *",  // 21:01 UTC = 01:01 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<MmuhInstitutionController>(
        "sync-mmuh-institutions-daily",
        c => c.SyncAllRegions(),
        "6 21 * * *",  // 21:06 UTC = 01:06 Armenia
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<NmuhInstitutionController>(
        "sync-nmuh-institutions-daily",
        c => c.SyncAllRegions(),
        "11 21 * * *", // 21:11 UTC = 01:11 Armenia
        TimeZoneInfo.Utc);
}

app.Run();
