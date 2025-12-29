using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Application.Interfaces
{
    public interface ITripRepository
    {
        Task<List<Trip>> GetUpcomingTripsAsync(Guid? fromStationId = null, Guid? toStationId = null, DateTime? date = null);
        Task<Trip?> GetTripWithStationsAsync(Guid id);
        Task<Trip?> GetByIdAsync(Guid id);
        Task AddAsync(Trip trip);
        Task UpdateAsync(Trip trip);
        Task DeleteAsync(Guid id);
    }
}
