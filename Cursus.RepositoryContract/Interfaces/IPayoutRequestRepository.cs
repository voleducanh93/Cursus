using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IPayoutRequestRepository : IRepository<PayoutRequest>
    {
        Task<IEnumerable<PayoutRequest>> GetPendingPayoutAsync();
        Task<IEnumerable<PayoutRequest>> GetApprovedPayoutAsync();
        Task<IEnumerable<PayoutRequest>> GetRejectedPayoutAsync();
        Task<PayoutRequest> GetPayoutByID(int id);

    }
}
