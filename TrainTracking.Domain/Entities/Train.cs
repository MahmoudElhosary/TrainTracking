using System;
namespace TrainTracking.Domain.Entities;
public class Train
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TrainNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int TotalSeats { get; set; }
}
