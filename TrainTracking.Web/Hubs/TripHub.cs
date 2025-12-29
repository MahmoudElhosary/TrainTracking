using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TrainTracking.Web.Hubs
{
    public class TripHub : Hub
    {
        public async Task BroadcastTripUpdate(object trip)
        {
            await Clients.All.SendAsync("TripUpdated", trip);
        }

        public async Task SendLocation(Guid tripId, double lat, double lng)
        {
            await Clients.All.SendAsync("ReceiveLocation", new { tripId, lat, lng });
        }
    }
}
