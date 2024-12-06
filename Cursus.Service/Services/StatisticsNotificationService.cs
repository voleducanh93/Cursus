
using Microsoft.AspNetCore.SignalR;
using Cursus.API.Hubs;
using Cursus.ServiceContract.Interfaces;

namespace Cursus.Service.Services
{
    public class StatisticsNotificationService : IStatisticsNotificationService
    {
        private readonly IHubContext<StatisticsHub> _hubContext;

        public StatisticsNotificationService(IHubContext<StatisticsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifySalesAndRevenueUpdate()
        {
            await _hubContext.Clients.All.SendAsync("ReceiveSalesAndRevenueUpdate");
        }

        public async Task NotifyOrderStatisticsUpdate()
        {
            await _hubContext.Clients.All.SendAsync("ReceiveOrderStatisticsUpdate");
        }
    }
}
