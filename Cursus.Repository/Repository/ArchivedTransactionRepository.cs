using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Cursus.Repository.Repository
{
    public class ArchivedTransactionRepository : Repository<ArchivedTransaction>, IArchivedTransactionRepository
    {
        private readonly CursusDbContext _db;

        public ArchivedTransactionRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
