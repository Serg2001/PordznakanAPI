using PordznakanAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PordznakanAPI.Data;
using Hangfire;
using Hangfire.SqlServer;
using PordznakanAPI.Controllers;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure OpenAPI (Swagger) for development
builder.Services.AddOpenApi();

// Configure DbContext with SQL Server (60 second command timeout)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(200) // 60 seconds timeout
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

// Add Hangfire server
builder.Services.AddHangfireServer();

builder.Services.AddHttpClient("ktakapi", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://crmapi.dshh.am/api/Integration/");
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    };
});

// Register SchoolService as scoped
//builder.Services.AddScoped<SchoolService>();


// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Enable OpenAPI in development
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Configure Hangfire Dashboard (optional - useful for monitoring jobs)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() } // Allow all in development
});

// Schedule the recurring job to sync all regions at 00:01 daily
// Note: Cron expression "1 0 * * *" means 00:01 in the specified timezone
// For Armenia (UTC+4), if you want 00:01 local time, use "1 0 * * *" with Armenia timezone
// Or use UTC: "1 20 * * *" (20:01 UTC = 00:01 next day in Armenia)
try
{
    // Try to use Armenia timezone, fallback to UTC if not found
    var timezone = TimeZoneInfo.FindSystemTimeZoneById("Caucasus Standard Time");
    RecurringJob.AddOrUpdate<PupilController>(
        "sync-all-regions-daily",
        controller => controller.SyncAllRegions(),
        "1 00 * * *", // Cron expression: At 00:01 every day
        timezone);
}
catch (TimeZoneNotFoundException)
{
    // Fallback to UTC (20:01 UTC = 00:01 next day in Armenia UTC+4)
    // You can adjust this to your preferred timezone
    RecurringJob.AddOrUpdate<PupilController>(
        "sync-all-regions-daily",
        controller => controller.SyncAllRegions(),
        "1 00 * * *", // 20:01 UTC = 00:01 next day in Armenia
        TimeZoneInfo.Utc);
}

app.Run();

// Simple authorization filter for Hangfire Dashboard (allow all for now)
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In production, you should implement proper authorization
        // For now, allowing all requests
        return true;
    }
}