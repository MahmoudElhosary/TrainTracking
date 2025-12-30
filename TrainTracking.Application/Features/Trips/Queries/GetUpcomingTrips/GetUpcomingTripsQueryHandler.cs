using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrainTracking.Application.DTOs;
using TrainTracking.Application.Interfaces;

namespace TrainTracking.Application.Features.Trips.Queries.GetUpcomingTrips;

public class GetUpcomingTripsQueryHandler : IRequestHandler<GetUpcomingTripsQuery, List<TripDto>>
{
    private readonly ITripRepository _tripRepository;
    private readonly IMapper _mapper;

    public GetUpcomingTripsQueryHandler(ITripRepository tripRepository, IMapper mapper)
    {
        _tripRepository = tripRepository;
        _mapper = mapper;
    }

    public async Task<List<TripDto>> Handle(GetUpcomingTripsQuery request, CancellationToken cancellationToken)
    {
        var trips = await _tripRepository.GetUpcomingTripsAsync(
            request.FromStationId, 
            request.ToStationId, 
            request.Date);

        return _mapper.Map<List<TripDto>>(trips);
    }
}
