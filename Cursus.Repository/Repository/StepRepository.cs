using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Repository.Repository
{
	public class StepRepository : Repository<Step>, IStepRepository
	{
		private readonly CursusDbContext _db;
		public StepRepository(CursusDbContext db) : base(db) => _db = db;

        public async Task<Step> GetByIdAsync(int id)
        {
            // Sử dụng phương thức FindAsync để tìm StepContent theo Id
            return await _db.Steps
                                   .FirstOrDefaultAsync(s => s.Id == id) ?? throw new KeyNotFoundException("Step is not found");
        }

        public async Task<Step> GetByCoursId(int id)
        {
            return await _db.Steps
                .FirstOrDefaultAsync(s=> s.CourseId == id) ?? throw new KeyNotFoundException("Step is not found");
        }

        public async Task<IEnumerable<Step>> GetStepsByCoursId(int courseId)
        {
            return await _db.Steps
                            .Where(s => s.CourseId == courseId)
                            .ToListAsync(); 
        }
        public async Task<double> GetToTalSteps(int couressId)
        {
            return await _db.Steps.Where(sc => sc.CourseId == couressId)
                            .CountAsync();
        }

    }
}
