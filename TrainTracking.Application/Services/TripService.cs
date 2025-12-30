using System;
using System.Linq;
using System.Threading.Tasks;
using TrainTracking.Application.Interfaces;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Application.Services
{
    public class TripService : ITripService
    {
        private readonly IStationRepository _stationRepository;
        private const double TrainSpeedKmh = 300.0;

        public TripService(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task<DateTimeOffset> CalculateArrivalTimeAsync(Guid fromStationId, Guid toStationId, DateTimeOffset departureTime)
        {
            var fromStation = await _stationRepository.GetByIdAsync(fromStationId);
            var toStation = await _stationRepository.GetByIdAsync(toStationId);

            if (fromStation == null || toStation == null)
                return departureTime.AddHours(1); // Fallback

            double distanceKm = CalculateDistance(fromStation.Latitude, fromStation.Longitude, toStation.Latitude, toStation.Longitude);
            
            // Time in hours = Distance / Speed
            double travelTimeHours = distanceKm / TrainSpeedKmh;
            double travelTimeMinutes = travelTimeHours * 60;

            // Calculate intermediate stations for stop time (10 mins each)
            int stopCount = await GetIntermediateStationCountAsync(fromStationId, toStationId);
            travelTimeMinutes += (stopCount * 10);

            return departureTime.AddMinutes(Math.Ceiling(travelTimeMinutes));
        }

        private async Task<int> GetIntermediateStationCountAsync(Guid fromId, Guid toId)
        {
            // For a production app, we would have "Routes" with ordered stations.
            // For this implementation, we'll define a logical "Main Line" sequence.
            var mainLineIds = new List<Guid>
            {
                new Guid("33333333-3333-3333-3333-333333333333"), // الجهراء
                new Guid("11111111-1111-1111-1111-111111111111"), // الكويت المركزية
                new Guid("44444444-4444-4444-4444-444444444444"), // الفروانية
                new Guid("22222222-2222-2222-2222-222222222222"), // حولي
                new Guid("55555555-5555-5555-5555-555555551111"), // السالمية
                new Guid("66666666-6666-6666-6666-666666666666"), // مبارك الكبير
                new Guid("55555555-5555-5555-5555-555555552222"), // الأحمدي
                new Guid("77777777-7777-7777-7777-777777777777")  // الفحيحيل
            };

            int fromIndex = mainLineIds.IndexOf(fromId);
            int toIndex = mainLineIds.IndexOf(toId);

            if (fromIndex == -1 || toIndex == -1) return 0;

            // Number of stations strictly BETWEEN from and to
            int diff = Math.Abs(fromIndex - toIndex);
            return diff > 1 ? diff - 1 : 0;
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // Radius of the earth in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = r * c; // Distance in km
            return d;
        }

        private double ToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
