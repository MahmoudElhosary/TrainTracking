using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainTracking.Domain.Entities;

namespace TrainTracking.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task CreateAsync(Notification notification);
        Task<List<Notification>> GetByRecipientAsync(string recipient);
        Task<List<Notification>> GetAllAsync();
    }
}
