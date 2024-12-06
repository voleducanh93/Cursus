using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IWalletHistoryRepository : IRepository<WalletHistory>
    {
        Task<bool> AnyAsync(Expression<Func<WalletHistory, bool>> predicate);
        Task<WalletHistory> GetByIdAsync(int id);
        Task<IEnumerable<WalletHistory>> GetByWalletId(int WalletId);
    }
}
