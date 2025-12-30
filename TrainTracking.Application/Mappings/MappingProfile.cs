using AutoMapper;
using TrainTracking.Application.DTOs;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Trip, TripDto>()
            .ForMember(dest => dest.TrainNumber, opt => opt.MapFrom(src => src.Train != null ? src.Train.TrainNumber : string.Empty))
            .ForMember(dest => dest.TrainType, opt => opt.MapFrom(src => src.Train != null ? src.Train.Type : string.Empty))
            .ForMember(dest => dest.FromStationName, opt => opt.MapFrom(src => src.FromStation != null ? src.FromStation.Name : string.Empty))
            .ForMember(dest => dest.FromStationLatitude, opt => opt.MapFrom(src => src.FromStation != null ? src.FromStation.Latitude : 0))
            .ForMember(dest => dest.FromStationLongitude, opt => opt.MapFrom(src => src.FromStation != null ? src.FromStation.Longitude : 0))
            .ForMember(dest => dest.ToStationName, opt => opt.MapFrom(src => src.ToStation != null ? src.ToStation.Name : string.Empty))
            .ForMember(dest => dest.ToStationLatitude, opt => opt.MapFrom(src => src.ToStation != null ? src.ToStation.Latitude : 0))
            .ForMember(dest => dest.ToStationLongitude, opt => opt.MapFrom(src => src.ToStation != null ? src.ToStation.Longitude : 0));
    }
}
