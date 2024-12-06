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
    public class StepCommentRepository : Repository<StepComment>, IStepCommentRepository
    {
        private readonly CursusDbContext _db;

        public StepCommentRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<IEnumerable<StepComment>> GetCommentsByStepId(int stepId)
        {
            return await _db.StepComments
                .Include(sc => sc.User) 
                .Include(sc => sc.Step)  
                .Where(sc => sc.StepId == stepId)
                .ToListAsync();
        }


    }
}