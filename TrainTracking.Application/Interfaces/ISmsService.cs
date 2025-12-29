using System.Threading.Tasks;

namespace TrainTracking.Application.Interfaces
{
    public class SmsResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface ISmsService
    {
        Task<SmsResult> SendSmsAsync(string phoneNumber, string message);
    }
}
