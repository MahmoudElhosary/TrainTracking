using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTracking.Application.Interfaces;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ITripRepository _tripRepository;
        private readonly ITrainRepository _trainRepository;
        private readonly IStationRepository _stationRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ISmsService _smsService;
        private readonly INotificationRepository _notificationRepository;

        public AdminController(ITripRepository tripRepository, ITrainRepository trainRepository, 
            IStationRepository stationRepository, IBookingRepository bookingRepository, ISmsService smsService,
            INotificationRepository notificationRepository)
        {
            _tripRepository = tripRepository;
            _trainRepository = trainRepository;
            _stationRepository = stationRepository;
            _bookingRepository = bookingRepository;
            _smsService = smsService;
            _notificationRepository = notificationRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- Trips Management ---
        public async Task<IActionResult> Trips()
        {
            var trips = await _tripRepository.GetUpcomingTripsAsync();
            return View(trips);
        }

        public async Task<IActionResult> CreateTrip()
        {
            ViewBag.Trains = await _trainRepository.GetAllAsync();
            ViewBag.Stations = await _stationRepository.GetAllAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrip(Trip trip)
        {
            if (ModelState.IsValid)
            {
                // Adjust times to Kuwait (+3) if they don't have an offset
                // datetime-local sends "2025-12-28T11:27" which binds as local or UTC with 0 offset
                var offset = TimeSpan.FromHours(3);
                trip.DepartureTime = new DateTimeOffset(trip.DepartureTime.DateTime, offset);
                trip.ArrivalTime = new DateTimeOffset(trip.ArrivalTime.DateTime, offset);

                await _tripRepository.AddAsync(trip);
                return RedirectToAction(nameof(Trips));
            }
            ViewBag.Trains = await _trainRepository.GetAllAsync();
            ViewBag.Stations = await _stationRepository.GetAllAsync();
            return View(trip);
        }

        public async Task<IActionResult> EditTrip(Guid id)
        {
            var trip = await _tripRepository.GetTripWithStationsAsync(id);
            if (trip == null) return NotFound();

            ViewBag.Trains = await _trainRepository.GetAllAsync();
            ViewBag.Stations = await _stationRepository.GetAllAsync();
            return View("CreateTrip", trip);
        }

        [HttpPost]
        public async Task<IActionResult> EditTrip(Trip trip)
        {
            if (ModelState.IsValid)
            {
                var offset = TimeSpan.FromHours(3);
                trip.DepartureTime = new DateTimeOffset(trip.DepartureTime.DateTime, offset);
                trip.ArrivalTime = new DateTimeOffset(trip.ArrivalTime.DateTime, offset);

                await _tripRepository.UpdateAsync(trip);

                // Notification Logic for Delays
                if (trip.Status == TripStatus.Delayed)
                {
                    var bookings = await _bookingRepository.GetBookingsByTripIdAsync(trip.Id);
                    foreach (var booking in bookings)
                    {
                        var phoneNumber = booking.PassengerPhone;
                        if (!phoneNumber.StartsWith("+") && phoneNumber.Length == 8)
                        {
                            phoneNumber = "+965" + phoneNumber;
                        }

                        var delayMsg = $"تنبيه: رحلتك {trip.Id.ToString().Substring(0, 5)} متأخرة {trip.DelayMinutes} دقيقة. نعتذر عن الإزعاج. 🏛️🚅";
                        var smsResult = await _smsService.SendSmsAsync(phoneNumber, delayMsg);

                        // Save History
                        await _notificationRepository.CreateAsync(new Notification
                        {
                            Recipient = phoneNumber,
                            Message = delayMsg,
                            Type = NotificationType.SMS,
                            TripId = trip.Id,
                            BookingId = booking.Id,
                            IsSent = smsResult.Success,
                            ErrorMessage = smsResult.ErrorMessage
                        });
                    }
                }

                return RedirectToAction(nameof(Trips));
            }
            ViewBag.Trains = await _trainRepository.GetAllAsync();
            ViewBag.Stations = await _stationRepository.GetAllAsync();
            return View("CreateTrip", trip);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTrip(Guid id)
        {
            try
            {
                await _tripRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "تم حذف الرحلة بنجاح";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف الرحلة: " + ex.Message;
            }
            return RedirectToAction(nameof(Trips));
        }

        // --- Trains Management ---
        public async Task<IActionResult> Trains()
        {
            var trains = await _trainRepository.GetAllAsync();
            return View(trains);
        }

        public IActionResult CreateTrain()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrain(Train train)
        {
            if (ModelState.IsValid)
            {
                await _trainRepository.AddAsync(train);
                return RedirectToAction(nameof(Trains));
            }
            return View(train);
        }

        public async Task<IActionResult> EditTrain(Guid id)
        {
            var train = await _trainRepository.GetByIdAsync(id);
            if (train == null) return NotFound();
            return View("CreateTrain", train);
        }

        [HttpPost]
        public async Task<IActionResult> EditTrain(Train train)
        {
            if (ModelState.IsValid)
            {
                await _trainRepository.UpdateAsync(train);
                return RedirectToAction(nameof(Trains));
            }
            return View("CreateTrain", train);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTrain(Guid id)
        {
            // Check if train is used in any trips
            var trips = await _tripRepository.GetUpcomingTripsAsync();
            var isUsed = trips.Any(t => t.TrainId == id);
            
            if (isUsed)
            {
                TempData["ErrorMessage"] = "لا يمكن حذف هذا القطار لأنه مستخدم في رحلات موجودة. يرجى حذف الرحلات أولاً.";
                return RedirectToAction(nameof(Trains));
            }
            
            try
            {
                await _trainRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "تم حذف القطار بنجاح";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف القطار: " + ex.Message;
            }
            return RedirectToAction(nameof(Trains));
        }

        // --- Stations Management ---
        public async Task<IActionResult> Stations()
        {
            var stations = await _stationRepository.GetAllAsync();
            return View(stations);
        }

        public IActionResult CreateStation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateStation(Station station)
        {
            if (ModelState.IsValid)
            {
                await _stationRepository.AddAsync(station);
                return RedirectToAction(nameof(Stations));
            }
            return View(station);
        }

        public async Task<IActionResult> EditStation(Guid id)
        {
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null) return NotFound();
            return View("CreateStation", station);
        }

        [HttpPost]
        public async Task<IActionResult> EditStation(Station station)
        {
            if (ModelState.IsValid)
            {
                await _stationRepository.UpdateAsync(station);
                return RedirectToAction(nameof(Stations));
            }
            return View("CreateStation", station);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStation(Guid id)
        {
            // Check if station is used in any trips
            var trips = await _tripRepository.GetUpcomingTripsAsync();
            var isUsed = trips.Any(t => t.FromStationId == id || t.ToStationId == id);
            
            if (isUsed)
            {
                TempData["ErrorMessage"] = "لا يمكن حذف هذه المحطة لأنها مستخدمة في رحلات موجودة. يرجى حذف الرحلات أولاً.";
                return RedirectToAction(nameof(Stations));
            }
            
            try
            {
                await _stationRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "تم حذف المحطة بنجاح";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف المحطة: " + ex.Message;
            }
            return RedirectToAction(nameof(Stations));
        }

        public async Task<IActionResult> Simulator()
        {
            var trips = await _tripRepository.GetUpcomingTripsAsync();
            return View(trips);
        }

        public async Task<IActionResult> Notifications()
        {
            var notifications = await _notificationRepository.GetAllAsync();
            return View(notifications);
        }
    }
}
