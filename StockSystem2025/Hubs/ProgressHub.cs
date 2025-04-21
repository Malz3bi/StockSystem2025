using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace StockSystem2025.Hubs
{
    public class ProgressHub : Hub
    {
        public async Task SendProgress(string processId, double percentage, string message)
        {
            await Clients.Caller.SendAsync("ReceiveProgress", processId, percentage, message);
        }
    }
}