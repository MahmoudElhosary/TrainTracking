using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task Seed(TrainTrackingDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        context.Database.Migrate();

        // Seed Kuwaiti Stations (Specific Areas)
        if (!context.Stations.Any(s => s.Name.Contains("الكويت") || s.Name.Contains("الأحمدي")))
        {
            var stations = new List<Station>
            {
                new Station { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "محطة الكويت المركزية", Latitude = 29.3759, Longitude = 47.9774 },
                new Station { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "محطة حولي", Latitude = 29.3333, Longitude = 48.0167 },
                new Station { Id = new Guid("33333333-3333-3333-3333-333333333333"), Name = "محطة الجهراء", Latitude = 29.3375, Longitude = 47.6581 },
                new Station { Id = new Guid("44444444-4444-4444-4444-444444444444"), Name = "محطة الفروانية", Latitude = 29.2833, Longitude = 47.9500 },
                new Station { Id = new Guid("55555555-5555-5555-5555-555555551111"), Name = "محطة السالمية", Latitude = 29.3333, Longitude = 48.0833 },
                new Station { Id = new Guid("55555555-5555-5555-5555-555555552222"), Name = "محطة الأحمدي", Latitude = 29.0769, Longitude = 48.0669 },
                new Station { Id = new Guid("66666666-6666-6666-6666-666666666666"), Name = "محطة مبارك الكبير", Latitude = 29.2125, Longitude = 48.0617 },
                new Station { Id = new Guid("77777777-7777-7777-7777-777777777777"), Name = "محطة الفحيحيل", Latitude = 29.0833, Longitude = 48.1333 }
            };

            foreach (var station in stations)
            {
                if (!context.Stations.Any(s => s.Id == station.Id))
                {
                    context.Stations.Add(station);
                }
            }
            context.SaveChanges();
        }

        if (!context.Trains.Any())
        {
            context.Trains.AddRange(
                new Train { Id = new Guid("aaaa1111-1111-1111-1111-111111111111"), TrainNumber = "KWT-101", Type = "سريع", TotalSeats = 200 },
                new Train { Id = new Guid("aaaa2222-2222-2222-2222-222222222222"), TrainNumber = "KWT-102", Type = "VIP", TotalSeats = 120 },
                new Train { Id = new Guid("aaaa3333-3333-3333-3333-333333333333"), TrainNumber = "KWT-103", Type = "محلي", TotalSeats = 300 }
            );
            context.SaveChanges();
        }

        if (!context.Trips.Any())
        {
            context.Trips.AddRange(
                new Trip
                {
                    Id = Guid.NewGuid(),
                    TrainId = new Guid("aaaa1111-1111-1111-1111-111111111111"),
                    FromStationId = new Guid("11111111-1111-1111-1111-111111111111"),
                    ToStationId = new Guid("33333333-3333-3333-3333-333333333333"),
                    DepartureTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddHours(1),
                    ArrivalTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddHours(2),
                    Status = TripStatus.OnTime
                },
                new Trip
                {
                    Id = Guid.NewGuid(),
                    TrainId = new Guid("aaaa2222-2222-2222-2222-222222222222"),
                    FromStationId = new Guid("11111111-1111-1111-1111-111111111111"),
                    ToStationId = new Guid("55555555-5555-5555-5555-555555555555"),
                    DepartureTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddHours(3),
                    ArrivalTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddHours(4),
                    Status = TripStatus.OnTime
                },
                 new Trip
                {
                    Id = Guid.NewGuid(),
                    TrainId = new Guid("aaaa3333-3333-3333-3333-333333333333"),
                    FromStationId = new Guid("22222222-2222-2222-2222-222222222222"),
                    ToStationId = new Guid("66666666-6666-6666-6666-666666666666"),
                    DepartureTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddHours(2),
                    ArrivalTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddHours(3),
                    Status = TripStatus.Delayed,
                    DelayMinutes = 15
                }
            );
            context.SaveChanges();
        }

        // Seed Roles
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed Admin User
        var adminEmail = "admin@train.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
