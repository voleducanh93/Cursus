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
    public class RefreshTokenRepository : Repository<RefreshToken>,IRefreshTokenRepository
    {
        private readonly CursusDbContext _context;
        public RefreshTokenRepository(CursusDbContext db) : base(db)
        {
            _context = db;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string refreshToken)
        {
            return await _context.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshToken);
        }

        
    }
}
