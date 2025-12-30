using Microsoft.EntityFrameworkCore;
using TrainTracking.Infrastructure.Persistence;
using TrainTracking.Application.Interfaces;
using TrainTracking.Infrastructure.Repositories;
using TrainTracking.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Infrastructure;
using TrainTracking.Web.Services;
using TrainTracking.Infrastructure.Services;
using TrainTracking.Infrastructure.Configuration;
using TrainTracking.Application.Services;

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"[KuwGo] Starting on port {port}");
builder.WebHost.UseUrls($"http://*:{port}");
Console.WriteLine($"[KuwGo] Running in {builder.Environment.EnvironmentName} environment");

// Identity Configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<TrainTrackingDbContext>()
.AddDefaultTokenProviders();


// Configure SQLite with proper path for Railway/Docker
var dbPath = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? "traintracking.db";
builder.Services.AddDbContext<TrainTrackingDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<ITrainRepository, TrainRepository>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IStationRepository, StationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(TrainTracking.Application.Features.Trips.Queries.GetUpcomingTrips.GetUpcomingTripsQuery).Assembly));
builder.Services.AddAutoMapper(typeof(TrainTracking.Application.Mappings.MappingProfile).Assembly);
builder.Services.AddHostedService<TripCleanupService>();
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
builder.Services.AddHttpClient<ISmsService, TwilioSmsService>();
builder.Services.AddScoped<TicketGenerator>();
builder.Services.AddScoped<IEmailService, MockEmailService>();
builder.Services.AddScoped<ITripService, TripService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Disabled to allow mobile access via HTTP without SSL errors
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<TripHub>("/tripHub");

// Seed Database (only in Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<TrainTrackingDbContext>();
            var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
            var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
            await DbInitializer.Seed(context, userManager, roleManager);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
else
{
    // In Production, just ensure the database is created
    Console.WriteLine("[KuwGo] Ensuring database is created...");
    using (var scope = app.Services.CreateScope())
    {
        try 
        {
            var context = scope.ServiceProvider.GetRequiredService<TrainTrackingDbContext>();
            context.Database.EnsureCreated();
            Console.WriteLine("[KuwGo] Database is ready.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KuwGo] ERROR during database creation: {ex.Message}");
        }
    }
}

Console.WriteLine("[KuwGo] Application starting up...");
app.Run();
