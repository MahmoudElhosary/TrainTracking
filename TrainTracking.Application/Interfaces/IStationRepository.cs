using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Application.Interfaces
{
    public interface IStationRepository
    {
        Task<List<Station>> GetAllAsync();
        Task<Station?> GetByIdAsync(Guid id);
        Task AddAsync(Station station);
        Task UpdateAsync(Station station);
        Task DeleteAsync(Guid id);
    }
}
