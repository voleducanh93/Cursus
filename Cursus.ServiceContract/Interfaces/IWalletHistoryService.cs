using Cursus.Data.DTO.Category;
using Cursus.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cursus.Data.Entities;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IWalletHistoryService
    {
        Task<PageListResponse<WalletHistoryDTO>> GetWalletHistoriessAsync(string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        int page = 1,
        int pageSize = 20);
        Task<WalletHistoryDTO> GetByIdAsync(int id);
        Task<IEnumerable<WalletHistoryDTO>> GetByWalletId(int WalletId);
    }
}
