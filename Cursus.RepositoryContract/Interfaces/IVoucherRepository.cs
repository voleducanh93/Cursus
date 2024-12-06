using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IVoucherRepository: IRepository<Voucher>
    {
        Task<Voucher> GetByVourcherIdAsync(int voucherId);
        Task<Voucher> GetByCodeAsync(string code);

    }
}
