using System;

namespace TrainTracking.Application.Interfaces;

public interface IDateTimeService
{
    DateTimeOffset Now { get; }
    DateTimeOffset GetKuwaitTime();
}
