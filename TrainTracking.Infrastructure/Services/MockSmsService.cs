using System.Diagnostics;
using System.Threading.Tasks;
using TrainTracking.Application.Interfaces;

namespace TrainTracking.Infrastructure.Services
{
    public class MockSmsService : ISmsService
    {
        public Task<SmsResult> SendSmsAsync(string phoneNumber, string message)
        {
            // Simulate sending SMS by logging to Debug/Console
            Debug.WriteLine("===============================================");
            Debug.WriteLine($"📱 MOCK SMS TO: {phoneNumber}");
            Debug.WriteLine($"💬 MESSAGE: {message}");
            Debug.WriteLine("===============================================");
            
            return Task.FromResult(new SmsResult { Success = true });
        }
    }
}
