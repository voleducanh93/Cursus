using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;

namespace Cursus.Repository.Repository
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        private readonly CursusDbContext _db;

        public MessageRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
