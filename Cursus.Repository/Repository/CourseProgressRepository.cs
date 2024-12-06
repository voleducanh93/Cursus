using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Repository.Repository
{
	public class CourseProgressRepository : Repository<CourseProgress>, ICourseProgressRepository
	{
		private readonly CursusDbContext _db;
		public CourseProgressRepository(CursusDbContext db) : base(db) => _db = db;

        public async Task<(double total, double completed)> TrackingProgress(string userId, int courseId)
        {
            var total = await _db.CourseProgresses
                .Where(x => x.UserId == userId.ToString() && x.CourseId == courseId)
                .CountAsync();

            var completed = await _db.CourseProgresses
                .Where(x => x.UserId == userId.ToString() && x.CourseId == courseId && x.IsCompleted == true)
                .CountAsync();

            return (total, completed);
        }

    }
}
