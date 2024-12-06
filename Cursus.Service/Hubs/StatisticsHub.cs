using Cursus.Data.DTO;
using Microsoft.AspNetCore.SignalR;


namespace Cursus.API.Hubs
{
    public class StatisticsHub : Hub
    {
       
        public async Task SendSalesAndRevenueUpdate()
        {
            await Clients.All.SendAsync("ReceiveSalesAndRevenueUpdate");
        }

        public async Task SendCourseStatisticsUpdate()
        {
            await Clients.All.SendAsync("ReceiveCourseStatisticsUpdate");
        }
       
        public async Task SendOrderStatisticsUpdate()
        {
            await Clients.All.SendAsync("ReceiveOrderStatisticsUpdate");
        }

    }
}
