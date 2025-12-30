using MediatR;
using System;
using System.Collections.Generic;
using TrainTracking.Application.DTOs;

namespace TrainTracking.Application.Features.Trips.Queries.GetUpcomingTrips;

public class GetUpcomingTripsQuery : IRequest<List<TripDto>>
{
    public Guid? FromStationId { get; set; }
    public Guid? ToStationId { get; set; }
    public DateTime? Date { get; set; }
}
