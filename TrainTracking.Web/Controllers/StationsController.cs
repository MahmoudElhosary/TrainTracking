using Microsoft.AspNetCore.Mvc;
using TrainTracking.Application.Interfaces;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Web.Controllers
{
    public class StationsController : Controller
    {
        private readonly IStationRepository _stationRepository;

        public StationsController(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetNearest(double lat, double lng)
        {
            var stations = await _stationRepository.GetAllAsync();
            
            Station? nearestStation = null;
            double minDistance = double.MaxValue;

            foreach (var station in stations)
            {
                var distance = CalculateDistance(lat, lng, station.Latitude, station.Longitude);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestStation = station;
                }
            }

            if (nearestStation != null && minDistance <= 50) // Only suggest if within 50km
            {
                return Json(new { 
                    success = true, 
                    stationId = nearestStation.Id, 
                    stationName = nearestStation.Name,
                    distance = Math.Round(minDistance, 1)
                });
            }

            return Json(new { success = false });
        }

        // Haversine Formula to calculate distance in KM
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1); 
            var a = 
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * 
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2); 
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)); 
            var d = R * c; // Distance in km
            return d;
        }

        private double ToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
