using System;
using TrainTracking.Application.Interfaces;

namespace TrainTracking.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    private static readonly TimeSpan KuwaitOffset = TimeSpan.FromHours(3);

    public DateTimeOffset Now => DateTimeOffset.UtcNow.ToOffset(KuwaitOffset);

    public DateTimeOffset GetKuwaitTime() => Now;
}
