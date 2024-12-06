using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class ReasonRepository : Repository<Reason>, IReasonRepository
    {
        private readonly CursusDbContext _db;
        public ReasonRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Reason> GetByIdAsync(int id)
        {
            return await _db.Reason.FirstOrDefaultAsync(r => r.Id == id) ?? throw new KeyNotFoundException("Reason content is not found");
        }

        public async Task<Reason> GetByCourseIdAsync(int courseId)
        {
            return await _db.Reason.FirstOrDefaultAsync(r => r.CourseId == courseId)
                   ?? throw new KeyNotFoundException("Reason for the specified course not found.");
        }
    }
}
