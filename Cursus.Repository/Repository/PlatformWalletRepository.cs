using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class PlatformWalletRepository : IPlatformWalletRepository
    {

        private readonly CursusDbContext _db;

        public PlatformWalletRepository(CursusDbContext db)
        {
            _db = db;
        }


        public async Task<PlatformWallet> GetPlatformWallet()
        {
            return await _db.PlatformWallets.FirstAsync();
        }
    }
}
