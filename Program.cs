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

// Schedule recurring jobs with proper timezone handling
try
{
    var timezone = TimeZoneInfo.FindSystemTimeZoneById("Caucasus Standard Time"); // Armenia Standard Time
    
    // Pupil sync job
    RecurringJob.AddOrUpdate<PupilController>(
        "sync-all-regions-daily",
        controller => controller.SyncAllRegions(),
        "1 0 * * *", // 00:01 Armenia time
        new RecurringJobOptions { TimeZone = timezone });
    
    // MmuhStudent sync job
    RecurringJob.AddOrUpdate<MmuhStudentController>(
        "sync-mmuh-students-daily",
        controller => controller.SyncAllRegions(),
        "5 0 * * *", // 00:05 Armenia time (5 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // MmuhStaff sync job
    RecurringJob.AddOrUpdate<MmuhStaffController>(
        "sync-mmuh-staff-daily",
        controller => controller.SyncAllRegions(),
        "10 0 * * *", // 00:10 Armenia time (10 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // NmuhStudent sync job
    RecurringJob.AddOrUpdate<NmuhStudentController>(
        "sync-nmuh-students-daily",
        controller => controller.SyncAllRegions(),
        "15 0 * * *", // 00:15 Armenia time (15 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // NmuhStaff sync job
    RecurringJob.AddOrUpdate<NmuhStaffController>(
        "sync-nmuh-staff-daily",
        controller => controller.SyncAllRegions(),
        "20 0 * * *", // 00:20 Armenia time (20 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // LogEmployee processing job
    RecurringJob.AddOrUpdate<LogEmployeeController>(
        "process-log-employee-daily",
        controller => controller.ProcessAllRegions(),
        "25 0 * * *", // 00:25 Armenia time (25 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // LogStudent processing job
    RecurringJob.AddOrUpdate<LogStudentController>(
        "process-log-student-daily",
        controller => controller.ProcessAllRegions(),
        "30 0 * * *", // 00:30 Armenia time (30 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // LogMmuhEmployee processing job
    RecurringJob.AddOrUpdate<LogMmuhEmployeeController>(
        "process-log-mmuh-employee-daily",
        controller => controller.ProcessAllRegions(),
        "35 0 * * *", // 00:35 Armenia time (35 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // LogMmuhStudent processing job
    RecurringJob.AddOrUpdate<LogMmuhStudentController>(
        "process-log-mmuh-student-daily",
        controller => controller.ProcessAllRegions(),
        "40 0 * * *", // 00:40 Armenia time (40 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // LogNmuhStudent processing job
    RecurringJob.AddOrUpdate<LogNmuhStudentController>(
        "process-log-nmuh-student-daily",
        controller => controller.ProcessAllRegions(),
        "45 0 * * *", // 00:45 Armenia time (45 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
    
    // LogNmuhEmployee processing job
    RecurringJob.AddOrUpdate<LogNmuhEmployeeController>(
        "process-log-nmuh-employee-daily",
        controller => controller.ProcessAllRegions(),
        "50 0 * * *", // 00:50 Armenia time (50 minutes after pupil sync)
        new RecurringJobOptions { TimeZone = timezone });
}
catch (TimeZoneNotFoundException)
{
    app.Logger.LogWarning("Caucasus Standard Time not found, falling back to UTC.");
    
    // Pupil sync job
    RecurringJob.AddOrUpdate<PupilController>(
        "sync-all-regions-daily",
        controller => controller.SyncAllRegions(),
        "1 20 * * *", // 20:01 UTC = 00:01 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // MmuhStudent sync job
    RecurringJob.AddOrUpdate<MmuhStudentController>(
        "sync-mmuh-students-daily",
        controller => controller.SyncAllRegions(),
        "5 20 * * *", // 20:05 UTC = 00:05 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // MmuhStaff sync job
    RecurringJob.AddOrUpdate<MmuhStaffController>(
        "sync-mmuh-staff-daily",
        controller => controller.SyncAllRegions(),
        "10 20 * * *", // 20:10 UTC = 00:10 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // NmuhStudent sync job
    RecurringJob.AddOrUpdate<NmuhStudentController>(
        "sync-nmuh-students-daily",
        controller => controller.SyncAllRegions(),
        "15 20 * * *", // 20:15 UTC = 00:15 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // NmuhStaff sync job
    RecurringJob.AddOrUpdate<NmuhStaffController>(
        "sync-nmuh-staff-daily",
        controller => controller.SyncAllRegions(),
        "20 20 * * *", // 20:20 UTC = 00:20 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // LogEmployee processing job
    RecurringJob.AddOrUpdate<LogEmployeeController>(
        "process-log-employee-daily",
        controller => controller.ProcessAllRegions(),
        "25 20 * * *", // 20:25 UTC = 00:25 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // LogStudent processing job
    RecurringJob.AddOrUpdate<LogStudentController>(
        "process-log-student-daily",
        controller => controller.ProcessAllRegions(),
        "30 20 * * *", // 20:30 UTC = 00:30 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // LogMmuhEmployee processing job
    RecurringJob.AddOrUpdate<LogMmuhEmployeeController>(
        "process-log-mmuh-employee-daily",
        controller => controller.ProcessAllRegions(),
        "35 20 * * *", // 20:35 UTC = 00:35 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // LogMmuhStudent processing job
    RecurringJob.AddOrUpdate<LogMmuhStudentController>(
        "process-log-mmuh-student-daily",
        controller => controller.ProcessAllRegions(),
        "40 20 * * *", // 20:40 UTC = 00:40 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // LogNmuhStudent processing job
    RecurringJob.AddOrUpdate<LogNmuhStudentController>(
        "process-log-nmuh-student-daily",
        controller => controller.ProcessAllRegions(),
        "45 20 * * *", // 20:45 UTC = 00:45 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
    
    // LogNmuhEmployee processing job
    RecurringJob.AddOrUpdate<LogNmuhEmployeeController>(
        "process-log-nmuh-employee-daily",
        controller => controller.ProcessAllRegions(),
        "50 20 * * *", // 20:50 UTC = 00:50 Armenia time (UTC+4)
        TimeZoneInfo.Utc);
}

app.Run();

// Simple authorization filter for Hangfire Dashboard
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true; // Allow all (secure this in production!)
}