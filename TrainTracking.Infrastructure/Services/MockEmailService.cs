using System.Diagnostics;
using System.Threading.Tasks;
using TrainTracking.Application.Interfaces;

namespace TrainTracking.Infrastructure.Services
{
    public class MockEmailService : IEmailService
    {
        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // Simulate sending email by logging to Debug/Console
            Debug.WriteLine("-----------------------------------------------");
            Debug.WriteLine($"SENDING MOCK EMAIL TO: {toEmail}");
            Debug.WriteLine($"SUBJECT: {subject}");
            Debug.WriteLine($"BODY: {message}");
            Debug.WriteLine("-----------------------------------------------");
            
            return Task.CompletedTask;
        }
    }
}
