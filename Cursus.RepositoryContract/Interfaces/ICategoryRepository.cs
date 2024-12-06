using Cursus.Data.DTO.Category;
using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate);
    }
}
