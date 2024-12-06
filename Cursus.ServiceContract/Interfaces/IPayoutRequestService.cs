using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IPayoutRequestService
    {
        Task<IEnumerable<PayoutRequestDisplayDTO>> GetPendingPayoutRequest();
        Task<IEnumerable<PayoutRequestDisplayDTO>> GetApprovedPayoutRequest();
        Task<IEnumerable<PayoutRequestDisplayDTO>> GetRejectPayoutRequest();
        Task<PayoutAcceptDTO> AcceptPayout(int id);
        Task<PayoutDenyDTO> DenyPayout(int id, string reason);


    }
}
