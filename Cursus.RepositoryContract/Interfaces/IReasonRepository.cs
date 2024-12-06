using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IReasonRepository : IRepository<Reason>
    {
        Task<Reason> GetByIdAsync(int id);
        Task<Reason> GetByCourseIdAsync(int courseId);
    }
}
