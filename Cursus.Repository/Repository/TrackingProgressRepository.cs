using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Cursus.Repository.Repository
{
    public class TrackingProgressRepository : Repository<TrackingProgress>, ITrackingProgressRepository
    {
        private readonly CursusDbContext _db;
        public TrackingProgressRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<int> GetCompletedStepsCountByUserId(string userId, int ProgressID)
        {
            return await _db.TrackingProgresses.CountAsync(tp => tp.UserId == userId && tp.ProgressId == ProgressID );
        }
    }
}
