using System;

namespace TrainTracking.Domain.Entities
{
    public enum NotificationType
    {
        SMS,
        Email
    }

    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Recipient { get; set; } = string.Empty; // Phone number or email
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset SentAt { get; set; } = DateTimeOffset.Now;
        public NotificationType Type { get; set; }
        public bool IsSent { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Link to Trip if relevant
        public Guid? TripId { get; set; }
        public Trip? Trip { get; set; }
        
        // Link to Booking if relevant
        public Guid? BookingId { get; set; }
        public Booking? Booking { get; set; }
    }
}
