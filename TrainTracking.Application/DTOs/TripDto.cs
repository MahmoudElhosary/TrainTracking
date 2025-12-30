using System;

namespace TrainTracking.Application.DTOs;

public class TripDto
{
    public Guid Id { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string TrainType { get; set; } = string.Empty;
    public string FromStationName { get; set; } = string.Empty;
    public double FromStationLatitude { get; set; }
    public double FromStationLongitude { get; set; }
    public string ToStationName { get; set; } = string.Empty;
    public double ToStationLatitude { get; set; }
    public double ToStationLongitude { get; set; }
    public DateTimeOffset DepartureTime { get; set; }
    public DateTimeOffset ArrivalTime { get; set; }
    public TrainTracking.Domain.Entities.TripStatus Status { get; set; }
    public decimal Price { get; set; }
    public int DelayMinutes { get; set; }
}
