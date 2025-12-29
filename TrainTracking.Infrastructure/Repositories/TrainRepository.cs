using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrainTracking.Application.Interfaces;
using TrainTracking.Domain.Entities;
using TrainTracking.Infrastructure.Persistence;

namespace TrainTracking.Infrastructure.Repositories;

public class TrainRepository : ITrainRepository
{
    private readonly TrainTrackingDbContext _context;

    public TrainRepository(TrainTrackingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Train>> GetAllAsync()
    {
        return await _context.Trains.ToListAsync();
    }

    public async Task<Train?> GetByIdAsync(Guid id)
    {
        return await _context.Trains.FindAsync(id);
    }

    public async Task AddAsync(Train train)
    {
        _context.Trains.Add(train);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Train train)
    {
        _context.Trains.Update(train);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var train = await _context.Trains.FindAsync(id);
        if (train != null)
        {
            _context.Trains.Remove(train);
            await _context.SaveChangesAsync();
        }
    }
}
