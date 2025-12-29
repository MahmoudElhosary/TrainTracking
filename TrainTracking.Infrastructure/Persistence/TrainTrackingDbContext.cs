using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Infrastructure.Persistence
{
    public class TrainTrackingDbContext : IdentityDbContext<Microsoft.AspNetCore.Identity.IdentityUser>
    {
        public TrainTrackingDbContext(DbContextOptions<TrainTrackingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Train> Trains { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasOne(t => t.Train)
                      .WithMany()
                      .HasForeignKey(t => t.TrainId);

                entity.HasOne(t => t.FromStation)
                      .WithMany()
                      .HasForeignKey(t => t.FromStationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.ToStation)
                      .WithMany()
                      .HasForeignKey(t => t.ToStationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasOne(b => b.Trip)
                      .WithMany()
                      .HasForeignKey(b => b.TripId);

                entity.Property(b => b.Price)
                      .HasColumnType("decimal(10,2)");
            });

            // SQLite DateTimeOffset fix: convert to string to preserve offset
            var dateTimeOffsetConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, string>(
                v => v.ToString("O"),
                v => v.Contains("-") ? DateTimeOffset.Parse(v) : new DateTimeOffset(long.Parse(v), TimeSpan.Zero));

            var nullableDateTimeOffsetConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset?, string?>(
                v => v.HasValue ? v.Value.ToString("O") : (string?)null,
                v => string.IsNullOrEmpty(v) ? (DateTimeOffset?)null : 
                     (v.Contains("-") ? DateTimeOffset.Parse(v) : new DateTimeOffset(long.Parse(v), TimeSpan.Zero)));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(DateTimeOffset))
                    {
                        modelBuilder.Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(dateTimeOffsetConverter);
                    }
                    else
                    {
                        modelBuilder.Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(nullableDateTimeOffsetConverter);
                    }
                }
            }

        }
    }
}
