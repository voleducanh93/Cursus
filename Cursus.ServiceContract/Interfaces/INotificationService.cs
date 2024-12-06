using Cursus.Data.DTO;

namespace Cursus.ServiceContract.Interfaces
{
	public interface INotificationService
	{
		Task SendNotificationAsync(string userId, string message);
		Task<IEnumerable<NotificationDTO>> FetchNotificationsAsync(string userId);
	}
}
