using System;

namespace TrainTracking.Domain.Entities;

public enum TripStatus
{
    Scheduled,
    OnTime,
    Delayed,
    Cancelled,
    Completed
}

public class Trip
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TrainId { get; set; }
    public Train? Train { get; set; }
    public Guid FromStationId { get; set; }
    public Station? FromStation { get; set; }
    public Guid ToStationId { get; set; }
    public Station? ToStation { get; set; }
    public DateTimeOffset DepartureTime { get; set; }
    public DateTimeOffset ArrivalTime { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Scheduled;
    public int? DelayMinutes { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? PathPolyline { get; set; }
}
