using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class WalletHistoryRepository : Repository<WalletHistory>, IWalletHistoryRepository
    {
        private readonly CursusDbContext _db;

        public WalletHistoryRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<bool> AnyAsync(Expression<Func<WalletHistory, bool>> predicate)
        {
            return await _db.Set<WalletHistory>().AnyAsync(predicate);
        }

        public async Task<WalletHistory> GetByIdAsync(int id)
        {
            return await _db.WalletHistories.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<WalletHistory>> GetByWalletId(int WalletId)
        {
            return await _db.WalletHistories.Where(x => x.WalletId == WalletId).ToListAsync();
        }
    }
}
