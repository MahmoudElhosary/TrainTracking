using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TrainTracking.Application.Interfaces;
using TrainTracking.Infrastructure.Configuration;

namespace TrainTracking.Infrastructure.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly TwilioSettings _settings;

        public TwilioSmsService(HttpClient httpClient, IOptions<TwilioSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<SmsResult> SendSmsAsync(string phoneNumber, string message)
        {
            // Auto-format Kuwaiti numbers if they are 8 digits
            if (!phoneNumber.StartsWith("+") && phoneNumber.Length == 8)
            {
                phoneNumber = "+965" + phoneNumber;
            }

            if (string.IsNullOrEmpty(_settings.AccountSid) || string.IsNullOrEmpty(_settings.AuthToken))
            {
                return new SmsResult { Success = false, ErrorMessage = "Twilio credentials are not configured." };
            }

            try
            {
                var url = $"https://api.twilio.com/2010-04-01/Accounts/{_settings.AccountSid}/Messages.json";
                
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);

                var parameters = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("To", phoneNumber),
                    new KeyValuePair<string, string>("Body", message)
                };

                if (_settings.FromPhoneNumber.StartsWith("MG"))
                {
                    parameters.Add(new KeyValuePair<string, string>("MessagingServiceSid", _settings.FromPhoneNumber));
                }
                else
                {
                    parameters.Add(new KeyValuePair<string, string>("From", _settings.FromPhoneNumber));
                }

                var content = new FormUrlEncodedContent(parameters);

                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return new SmsResult { Success = true };
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Twilio Error: {responseBody}");
                    return new SmsResult { Success = false, ErrorMessage = responseBody };
                }
            }
            catch (Exception ex)
            {
                return new SmsResult { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}
