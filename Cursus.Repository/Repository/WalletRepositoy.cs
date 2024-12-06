using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;

namespace Cursus.Repository.Repository
{
    public class WalletRepositoy : Repository<Wallet>, IWalletRepository
    {
        private readonly CursusDbContext _db;

        public WalletRepositoy(CursusDbContext db) : base(db)
        {
            _db = db;
        }            
       

    }
}
