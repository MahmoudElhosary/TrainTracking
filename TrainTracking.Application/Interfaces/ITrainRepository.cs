using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Application.Interfaces
{
    public interface ITrainRepository
    {
        Task<List<Train>> GetAllAsync();
        Task<Train?> GetByIdAsync(Guid id);
        Task AddAsync(Train train);
        Task UpdateAsync(Train train);
        Task DeleteAsync(Guid id);
    }
}
