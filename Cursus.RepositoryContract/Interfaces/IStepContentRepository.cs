using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IStepContentRepository : IRepository<StepContent>
    {
        Task<StepContent> GetByIdAsync(int id);
        Task<StepContent> FirstOrDefaultAsync(Expression<Func<StepContent, bool>> predicate);
    }
}
