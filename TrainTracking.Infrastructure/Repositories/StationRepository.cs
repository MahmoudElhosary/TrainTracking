using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrainTracking.Application.Interfaces;
using TrainTracking.Domain.Entities;
using TrainTracking.Infrastructure.Persistence;

namespace TrainTracking.Infrastructure.Repositories;

public class StationRepository : IStationRepository
{
    private readonly TrainTrackingDbContext _context;

    public StationRepository(TrainTrackingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Station>> GetAllAsync()
    {
        return await _context.Stations.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<Station?> GetByIdAsync(Guid id)
    {
        return await _context.Stations.FindAsync(id);
    }

    public async Task AddAsync(Station station)
    {
        _context.Stations.Add(station);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Station station)
    {
        _context.Stations.Update(station);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var station = await _context.Stations.FindAsync(id);
        if (station != null)
        {
            _context.Stations.Remove(station);
            await _context.SaveChangesAsync();
        }
    }
}
