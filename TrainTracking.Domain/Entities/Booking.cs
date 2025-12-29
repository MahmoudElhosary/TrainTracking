using System;
using TrainTracking.Domain.Enums;
namespace TrainTracking.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerPhone { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset BookingDate { get; set; } = DateTimeOffset.Now;

        public BookingStatus Status { get; set; }
        
        public string UserId { get; set; } = string.Empty;

        // Foreign Key
        public Guid TripId { get; set; }
        public Trip Trip { get; set; } = null!;
    }
}
