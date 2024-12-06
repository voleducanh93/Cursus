using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IStepCommentRepository : IRepository<StepComment>
    {
        Task<IEnumerable<StepComment>> GetCommentsByStepId(int stepId);

    }
}