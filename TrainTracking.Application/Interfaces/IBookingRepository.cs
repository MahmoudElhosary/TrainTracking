using System;
using System.Threading.Tasks;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Application.Interfaces
{
    public interface IBookingRepository
    {
        Task CreateAsync(Booking booking);
        Task<bool> IsSeatTakenAsync(Guid tripId, int seatNumber);
        Task<Booking?> GetByIdAsync(Guid id);
        Task<List<Booking>> GetBookingsByUserIdAsync(string userId);
        Task<List<Booking>> GetBookingsByTripIdAsync(Guid tripId);
        Task<List<int>> GetTakenSeatsAsync(Guid tripId);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(Guid id);
        
        // Point Redemption
        Task CreateRedemptionAsync(PointRedemption redemption);
        Task<int> GetRedeemedPointsAsync(string userId);
    }
}
