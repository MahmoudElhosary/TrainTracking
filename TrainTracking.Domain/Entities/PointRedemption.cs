using System;

namespace TrainTracking.Domain.Entities
{
    public class PointRedemption
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PointsRedeemed { get; set; }
        public DateTimeOffset RedemptionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
