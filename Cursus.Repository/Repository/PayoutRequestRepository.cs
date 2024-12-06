using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class PayoutRequestRepository : Repository<PayoutRequest>, IPayoutRequestRepository
    {

        private readonly CursusDbContext _db;
        public PayoutRequestRepository(CursusDbContext db) : base(db)
        {
            _db = db;   
        }
        public async Task<IEnumerable<PayoutRequest>> GetApprovedPayoutAsync()
        {
            return await _db.PayoutRequests.Where(x => x.PayoutRequestStatus == Data.Enums.PayoutRequestStatus.Approved).ToListAsync();
        }

        public async Task<IEnumerable<PayoutRequest>> GetPendingPayoutAsync()
        {
            return await _db.PayoutRequests.Where(x => x.PayoutRequestStatus == Data.Enums.PayoutRequestStatus.Pending).ToListAsync();

        }

        public async Task<IEnumerable<PayoutRequest>> GetRejectedPayoutAsync()
        {
            return await _db.PayoutRequests.Where(x => x.PayoutRequestStatus == Data.Enums.PayoutRequestStatus.Rejected).ToListAsync();
        }
        public async Task<PayoutRequest> GetPayoutByID(int id)
        {
            return await GetAsync(filter: b => b.Id == id,includeProperties: "Transaction");
        }
    }
}
