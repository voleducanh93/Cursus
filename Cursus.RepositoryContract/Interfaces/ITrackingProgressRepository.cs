using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface ITrackingProgressRepository : IRepository<TrackingProgress>
    {
        Task<int> GetCompletedStepsCountByUserId(string userId, int ProgressID);

    }
}
