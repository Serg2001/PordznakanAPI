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
    httpClient.BaseAddress = new Uri("https://crmapi.dshh.am/api/Integration/");
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    };
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

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
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Schedule recurring job with proper timezone handling
try
{
    var timezone = TimeZoneInfo.FindSystemTimeZoneById("Caucasus Standard Time"); // Armenia Standard Time
    RecurringJob.AddOrUpdate<PupilController>(
        "sync-all-regions-daily",
        controller => controller.SyncAllRegions(),
        "1 0 * * *", // 00:01 Armenia time
        new RecurringJobOptions { TimeZone = timezone });
}
catch (TimeZoneNotFoundException)
{
    app.Logger.LogWarning("Caucasus Standard Time not found, falling back to UTC.");
    RecurringJob.AddOrUpdate<PupilController>(
        "sync-all-regions-daily",
        controller => controller.SyncAllRegions(),
        "1 20 * * *", // 20:01 UTC = 00:01 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
}

app.Run();

// Simple authorization filter for Hangfire Dashboard
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true; // Allow all (secure this in production!)
}