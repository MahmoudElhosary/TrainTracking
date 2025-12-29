using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrainTracking.Application.Interfaces;
using TrainTracking.Domain.Entities;
using TrainTracking.Infrastructure.Persistence;

namespace TrainTracking.Infrastructure.Repositories;

public class TripRepository : ITripRepository
{
    private readonly TrainTrackingDbContext _context;

    public TripRepository(TrainTrackingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Trip>> GetUpcomingTripsAsync(Guid? fromStationId = null, Guid? toStationId = null, DateTime? date = null)
    {
        var query = _context.Trips
            .Include(t => t.Train)
            .Include(t => t.FromStation)
            .Include(t => t.ToStation)
            .AsQueryable();

        if (fromStationId.HasValue)
        {
            query = query.Where(t => t.FromStationId == fromStationId.Value);
        }

        if (toStationId.HasValue)
        {
            query = query.Where(t => t.ToStationId == toStationId.Value);
        }

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            // For date only comparison, we can still use the property but we should be careful.
            // Since we converted to string, complex date logic in LINQ might still be tricky.
            // Let's use a range for the target date instead.
            var start = new DateTimeOffset(targetDate);
            var end = start.AddDays(1);
            query = query.Where(t => t.DepartureTime >= start && t.DepartureTime < end);
        }
        else
        {
            var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
            query = query.Where(t => t.DepartureTime >= now);
        }

        return await query
            .OrderBy(t => t.DepartureTime)
            .ToListAsync();
    }

    public async Task<Trip?> GetTripWithStationsAsync(Guid id)
    {
        return await _context.Trips
            .Include(t => t.Train)
            .Include(t => t.FromStation)
            .Include(t => t.ToStation)
            .FirstOrDefaultAsync(t => t.Id == id);
    }


    public async Task<Trip?> GetByIdAsync(Guid id)
    {
        return await _context.Trips.FindAsync(id);
    }

    public async Task AddAsync(Trip trip)
    {
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Trip trip)
    {
        _context.Trips.Update(trip);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip != null)
        {
            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
        }
    }
}
