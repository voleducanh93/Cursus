using Cursus.Data.DTO.Category;
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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly CursusDbContext _db;
        public CategoryRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate)
        {
            return await _db.Set<Category>().AnyAsync(predicate);
        }

    }
}
