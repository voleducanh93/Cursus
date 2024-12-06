using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;

namespace Cursus.Repository.Repository
{
	public class NotificationRepository : Repository<Notification>, INotificationRepository
	{
		private readonly CursusDbContext _db;

		public NotificationRepository(CursusDbContext db) : base(db)
		{
			_db = db;
		}

	}
}
