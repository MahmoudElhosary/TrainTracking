using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrainTracking.Application.Interfaces;
using TrainTracking.Domain.Entities;
using TrainTracking.Infrastructure.Persistence;

namespace TrainTracking.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly TrainTrackingDbContext _context;

        public NotificationRepository(TrainTrackingDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetByRecipientAsync(string recipient)
        {
            return await _context.Notifications
                .Where(n => n.Recipient == recipient)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            return await _context.Notifications
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }
    }
}
