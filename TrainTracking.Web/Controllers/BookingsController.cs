using Microsoft.AspNetCore.Mvc;
using TrainTracking.Application.Interfaces;
using TrainTracking.Domain.Entities;
using TrainTracking.Domain.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TrainTracking.Web.Controllers
{
    public class BookingsController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ITripRepository _tripRepository;
        private readonly Services.TicketGenerator _ticketGenerator;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly INotificationRepository _notificationRepository;

        public BookingsController(IBookingRepository bookingRepository, ITripRepository tripRepository, 
            Services.TicketGenerator ticketGenerator, IEmailService emailService, ISmsService smsService,
            INotificationRepository notificationRepository)
        {
            _bookingRepository = bookingRepository;
            _tripRepository = tripRepository;
            _ticketGenerator = ticketGenerator;
            _emailService = emailService;
            _smsService = smsService;
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid? id, Guid? tripId)
        {
            var targetId = id ?? tripId;
            if (targetId == null || targetId == Guid.Empty)
            {
                return BadRequest("Trip ID is required.");
            }

            var trip = await _tripRepository.GetTripWithStationsAsync(targetId.Value);
            if (trip == null)
            {
                return NotFound("لم يتم العثور على الرحلة المطلوبة.");
            }

            ViewBag.TakenSeats = await _bookingRepository.GetTakenSeatsAsync(targetId.Value);

            var booking = new Booking
            {
                TripId = targetId.Value,
                Trip = trip,
                Price = 50 
            };

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
             ModelState.Remove("Trip");
             ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                if (await _bookingRepository.IsSeatTakenAsync(booking.TripId, booking.SeatNumber))
                { 
                    ModelState.AddModelError("SeatNumber", "هذا المقعد محجوز بالفعل.");
                }
                else
                {
                    booking.Id = Guid.NewGuid();
                    booking.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
                    booking.Status = BookingStatus.PendingPayment;
                    booking.BookingDate = DateTimeOffset.Now;
                    await _bookingRepository.CreateAsync(booking);

                    // Redirect to Payment
                    return RedirectToAction(nameof(Payment), new { id = booking.Id });
                }
            }

            var trip = await _tripRepository.GetTripWithStationsAsync(booking.TripId);
            if (trip != null)
            {
                booking.Trip = trip;
            }
            ViewBag.TakenSeats = await _bookingRepository.GetTakenSeatsAsync(booking.TripId);
            return View(booking);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Payment(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != userId) return Forbid();

            if (booking.Status != BookingStatus.PendingPayment)
            {
                return RedirectToAction(nameof(MyBookings));
            }

            return View(booking);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(Guid id, string? bank, string? cardNumber, string? expiryDate, string? pin, string paymentMethod = "KNET")
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != userId) return Forbid();

            if (booking.Status != BookingStatus.PendingPayment)
            {
                return RedirectToAction(nameof(MyBookings));
            }

            // Mock Payment Processing
            await Task.Delay(1500); // Simulate network delay

            // For Apple Pay, we assume successful authentication via device
            // For K-NET, we mock validation
            if (paymentMethod == "KNET" && string.IsNullOrEmpty(pin))
            {
                 ModelState.AddModelError("pin", "يرجى إدخال الرقم السري");
                 return View("Payment", booking);
            }

            // Update Status to Confirmed
            booking.Status = BookingStatus.Confirmed;
            await _bookingRepository.UpdateAsync(booking);

            // Send Confirmation
            await _emailService.SendEmailAsync("user@example.com", "تأكيد حجز القطار", 
                $"عزيزي {booking.PassengerName}، تم تأكيد حجزك للرحلة رقم {booking.TripId} بعد الدفع عبر {paymentMethod}. شكراً لك!");

            // Send SMS
            var phoneNumber = booking.PassengerPhone;
            if (!phoneNumber.StartsWith("+") && phoneNumber.Length == 8)
            {
                phoneNumber = "+965" + phoneNumber;
            }

            var smsMessage = $"✅ تم الدفع بنجاح عبر {paymentMethod}! حجزك رقم {booking.Id.ToString().Substring(0, 8)} مؤكد الآن. رحلة سعيدة! 🚂💳";
            var smsResult = await _smsService.SendSmsAsync(phoneNumber, smsMessage);

            // Save Notification History
            await _notificationRepository.CreateAsync(new Notification
            {
                Recipient = phoneNumber,
                Message = smsMessage,
                Type = NotificationType.SMS,
                BookingId = booking.Id,
                TripId = booking.TripId,
                IsSent = smsResult.Success,
                ErrorMessage = smsResult.ErrorMessage
            });

            return RedirectToAction(nameof(Success), new { bookingId = booking.Id });
        }

        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Forbid();

            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
            return View(bookings);
        }

        public IActionResult Success(Guid? bookingId)
        {
            ViewBag.BookingId = bookingId;
            return View();
        }

        public async Task<IActionResult> DownloadTicket(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return NotFound();

            var request = HttpContext.Request;
            var host = request.Host.Value;
            var scheme = request.Scheme;
            
            // Smart Fix: If running on localhost, try to find the actual LAN IP so mobile devices can connect
            if (host.Contains("localhost") || host.Contains("127.0.0.1"))
            {
                try
                {
                    var localIp = GetLocalIpAddress();
                    if (!string.IsNullOrEmpty(localIp))
                    {
                        // FORCE Port 5244 (HTTP) because 7164 is HTTPS and usually rejects external non-cert connections
                        host = $"{localIp}:5244";
                        scheme = "http"; 
                    }
                }
                catch 
                {
                    // Fallback to original host if detection fails
                }
            }

            var baseUrl = $"{scheme}://{host}";
            var qrUrl = $"{baseUrl}/Bookings/TicketDetails/{booking.Id}";

            var pdf = _ticketGenerator.GenerateTicketPdf(booking, qrUrl);
            return File(pdf, "application/pdf", $"Ticket-{booking.Id.ToString().Substring(0, 8)}.pdf");
        }

        private string GetLocalIpAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            
            // First check for 192.168.x.x (Most common home network)
            var homeIp = host.AddressList.FirstOrDefault(ip => 
                ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && 
                !System.Net.IPAddress.IsLoopback(ip) && 
                ip.ToString().StartsWith("192.168."));

            if (homeIp != null) return homeIp.ToString();

            // Then check for 10.x.x.x or 172.x.x.x (Enterprise/Other)
            var otherIp = host.AddressList.FirstOrDefault(ip => 
                ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && 
                !System.Net.IPAddress.IsLoopback(ip));

            return otherIp?.ToString();
        }

        [AllowAnonymous]
        public async Task<IActionResult> TicketDetails(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        [Authorize]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != userId) return Forbid();

            if (booking.Status == BookingStatus.Cancelled)
            {
                return BadRequest("هذا الحجز ملغي بالفعل.");
            }

            var timeToDeparture = booking.Trip.DepartureTime - DateTimeOffset.Now;
            if (timeToDeparture.TotalSeconds <= 0)
            {
                return BadRequest("لا يمكن إلغاء حجز لرحلة قد بدأت بالفعل.");
            }

            decimal deductionPercentage = timeToDeparture.TotalHours <= 24 ? 25 : 10;
            decimal refundAmount = booking.Price * (1 - deductionPercentage / 100);

            ViewBag.DeductionPercentage = deductionPercentage;
            ViewBag.RefundAmount = refundAmount;

            return View(booking);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != userId) return Forbid();

            if (booking.Status == BookingStatus.Cancelled)
            {
                return RedirectToAction(nameof(MyBookings));
            }

            var timeToDeparture = booking.Trip.DepartureTime - DateTimeOffset.Now;
            if (timeToDeparture.TotalSeconds <= 0)
            {
                return BadRequest("لا يمكن إلغاء حجز لرحلة قد بدأت بالفعل.");
            }

            booking.Status = BookingStatus.Cancelled;
            await _bookingRepository.UpdateAsync(booking);

            // Calculate refund details
            decimal deductionPercentage = timeToDeparture.TotalHours <= 24 ? 25 : 10;
            decimal refundAmount = booking.Price * (1 - deductionPercentage / 100);

            var cancelMsg = $"تم إلغاء حجزك رقم {booking.Id.ToString().Substring(0, 8)} بنجاح. تم خصم {deductionPercentage}% وسيتم استرداد {refundAmount:F2} د.ك خلال أيام. شكراً لك! 🚂";
            await _smsService.SendSmsAsync(booking.PassengerPhone, cancelMsg);

            return RedirectToAction(nameof(MyBookings));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != userId) return Forbid();

            // Allow deletion of Cancelled OR PendingPayment bookings
            if (booking.Status != BookingStatus.Cancelled && booking.Status != BookingStatus.PendingPayment)
            {
                return BadRequest("يمكن حذف الحجوزات الملغية أو التي بانتظار الدفع فقط.");
            }

            await _bookingRepository.DeleteAsync(id);
            return RedirectToAction(nameof(MyBookings));
        }
    }
}
