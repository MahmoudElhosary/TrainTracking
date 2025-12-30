using System;
using System.Threading.Tasks;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Application.Interfaces
{
    public interface ITripService
    {
        Task<DateTimeOffset> CalculateArrivalTimeAsync(Guid fromStationId, Guid toStationId, DateTimeOffset departureTime);
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    }
}
