using Cursus.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDTO>> GetListTransaction(int page = 1, int pageSize = 20);
        Task<IEnumerable<TransactionDTO>> GetListTransactionByUserId(string userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<TransactionDTO>> GetAllPendingPayOutRequest();
    }
}
