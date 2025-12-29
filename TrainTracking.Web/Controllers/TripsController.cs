using Microsoft.AspNetCore.Mvc;
using TrainTracking.Application.Interfaces;
using TrainTracking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Web.Controllers
{
    public class TripsController : Controller
    {
        private readonly ITripRepository _tripRepository;
        private readonly IStationRepository _stationRepository;
        private readonly TrainTrackingDbContext _context;

        public TripsController(ITripRepository tripRepository, IStationRepository stationRepository, TrainTrackingDbContext context)
        {
            _tripRepository = tripRepository;
            _stationRepository = stationRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(Guid? fromStationId, Guid? toStationId, DateTime? date, string? searchStation)
        {
            var stations = await _stationRepository.GetAllAsync();

            // Handle "Book from here" feature (Nearest Station)
            if (!string.IsNullOrEmpty(searchStation) && fromStationId == null)
            {
                var station = stations.FirstOrDefault(s => s.Name == searchStation);
                if (station != null)
                {
                    fromStationId = station.Id;
                }
            }

            var trips = await _tripRepository.GetUpcomingTripsAsync(fromStationId, toStationId, date);
            
            ViewBag.Stations = stations;
            ViewBag.FromStationId = fromStationId;
            ViewBag.ToStationId = toStationId;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");

            return View(trips);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var trip = await _tripRepository.GetTripWithStationsAsync(id);
            if (trip == null)
            {
                return NotFound();
            }
            return View(trip);
        }

        public IActionResult Live()
        {
            try
            {
                var kuwaitOffset = TimeSpan.FromHours(3);
                var now = DateTimeOffset.UtcNow.ToOffset(kuwaitOffset);
                var todayStart = new DateTimeOffset(now.Date, kuwaitOffset);
                
                // Get trips that haven't arrived yet
                var liveTrips = _context.Trips
                    .Include(t => t.Train)
                    .Include(t => t.FromStation)
                    .Include(t => t.ToStation)
                    .Where(t => t.ArrivalTime >= now)
                    .OrderBy(t => t.DepartureTime)
                    .ToList();
                
                ViewBag.DebugInfo = $"Current (Kuwait): {now:yyyy-MM-dd HH:mm:ss} | Trips from today: {liveTrips.Count}";
                
                return View(liveTrips);
            }
            catch (Exception ex)
            {
                ViewBag.DebugInfo = $"ERROR: {ex.Message}";
                return View(new List<Trip>());
            }
        }
    }
}
