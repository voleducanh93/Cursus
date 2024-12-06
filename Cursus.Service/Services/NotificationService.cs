using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;

namespace Cursus.Service.Services
{
	public class NotificationService : INotificationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task SendNotificationAsync(string userId, string message)
		{
			var notification = new Notification
			{
				UserId = userId,
				Message = message,
				DateCreated = DateTime.Now,
				IsRead = false,
				IsNew = true
			};

			await _unitOfWork.NotificationRepository.AddAsync(notification);
			await _unitOfWork.SaveChanges();
		}

		public async Task<IEnumerable<NotificationDTO>> FetchNotificationsAsync(string userId)
		{
			var notifications = await _unitOfWork.NotificationRepository.GetAllAsync(n => n.UserId == userId);

			var notifyDTO = _mapper.Map<IEnumerable<NotificationDTO>>(notifications);

			foreach (var notification in notifications)
			{
				if (notification.IsRead == false)
				{
					notification.IsRead = true;
					notification.IsNew = false;
				}
			}
			await _unitOfWork.SaveChanges();

			return notifyDTO;

		}
	}
}
