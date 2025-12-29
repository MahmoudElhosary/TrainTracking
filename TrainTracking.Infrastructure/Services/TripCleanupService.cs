using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TrainTracking.Infrastructure.Persistence;

namespace TrainTracking.Infrastructure.Services
{
    public class TripCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TripCleanupService> _logger;
        private readonly TimeSpan _executionInterval = TimeSpan.FromHours(1);

        public TripCleanupService(IServiceProvider serviceProvider, ILogger<TripCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Trip Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Trip Cleanup Service is running a cleanup task.");

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<TrainTrackingDbContext>();
                        
                        var kuwaitOffset = TimeSpan.FromHours(3);
                        var now = DateTimeOffset.UtcNow.ToOffset(kuwaitOffset);

                        // Delete trips that arrived more than 1 hour ago
                        var expiredTrips = await context.Trips
                            .Where(t => t.ArrivalTime < now.AddHours(-1))
                            .ToListAsync(stoppingToken);

                        if (expiredTrips.Any())
                        {
                            var expiredTripIds = expiredTrips.Select(t => t.Id).ToList();
                            _logger.LogInformation($"Found {expiredTrips.Count} expired trips to delete.");

                            // Delete related notifications
                            var relatedNotifications = await context.Notifications
                                .Where(n => n.TripId.HasValue && expiredTripIds.Contains(n.TripId.Value))
                                .ToListAsync(stoppingToken);
                            if (relatedNotifications.Any())
                            {
                                context.Notifications.RemoveRange(relatedNotifications);
                            }

                            // Delete related bookings
                            var relatedBookings = await context.Bookings
                                .Where(b => expiredTripIds.Contains(b.TripId))
                                .ToListAsync(stoppingToken);
                            if (relatedBookings.Any())
                            {
                                context.Bookings.RemoveRange(relatedBookings);
                            }

                            context.Trips.RemoveRange(expiredTrips);
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Successfully deleted expired trips and their related records.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during trip cleanup.");
                }

                await Task.Delay(_executionInterval, stoppingToken);
            }

            _logger.LogInformation("Trip Cleanup Service is stopping.");
        }
    }
}
