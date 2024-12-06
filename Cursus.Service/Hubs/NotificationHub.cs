using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Cursus.Service.Hubs
{
	public class NotificationHub : Hub
	{
		private readonly INotificationService _notificationService;

		public NotificationHub(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public async Task SendNotificationToUser(string userId, string message)
		{
			await Clients.All.SendAsync("ReceiveNotification", userId, message);
		}
	}
}