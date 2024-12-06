using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.DTO.Category;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class WalletHistoryService : IWalletHistoryService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WalletHistoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<PageListResponse<WalletHistoryDTO>> GetWalletHistoriessAsync(string? searchTerm, string? sortColumn, string? sortOrder, int page = 1, int pageSize = 20)
        {
            var WalletHistories = await _unitOfWork.WalletHistoryRepository.GetAllAsync();

            if(!WalletHistories.Any()) { throw new KeyNotFoundException("There is no Wallet History."); }
            // Apply search filter if searchTerm is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                WalletHistories = WalletHistories.Where(x => x.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting if sortOrder is provided
            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                if (sortOrder?.ToLower() == "desc")
                {
                    WalletHistories = WalletHistories.OrderByDescending(GetSortProperty(sortColumn));
                }
                else
                {
                    WalletHistories = WalletHistories.OrderBy(GetSortProperty(sortColumn)).ToList();
                }
            }
            var TotalCount = WalletHistories.Count();
            
            var PaginatedWalletHistories = WalletHistories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var WalletHistoryDTOs = _mapper.Map<IEnumerable<WalletHistoryDTO>>(PaginatedWalletHistories);

            return new PageListResponse<WalletHistoryDTO>
            {
                Items = (List<WalletHistoryDTO>)WalletHistoryDTOs,//map to dto
                Page = page,
                PageSize = pageSize,
                TotalCount = TotalCount,
                HasNextPage = (page * pageSize) < TotalCount,
                HasPreviousPage = page > 1
            };

        }
        public async Task<WalletHistoryDTO> GetByIdAsync(int id)
        {
            var WalletHistory = await _unitOfWork.WalletHistoryRepository.GetByIdAsync(id);
            return WalletHistory != null ? _mapper.Map<WalletHistoryDTO>(WalletHistory) : throw new KeyNotFoundException("There is no Wallet History");
        }

        public async Task<IEnumerable<WalletHistoryDTO>> GetByWalletId(int WalletId)
        {
            var WalletHistories = await _unitOfWork.WalletHistoryRepository.GetByWalletId(WalletId);
            if (WalletHistories == null || !WalletHistories.Any())
            {
                throw new KeyNotFoundException("There is no Wallet History for this Wallet ID");
            }

            // Map the collection to a collection of DTOs
            return _mapper.Map<IEnumerable<WalletHistoryDTO>>(WalletHistories);
        }
        private static Func<WalletHistory, object> GetSortProperty(string SortColumn)
        {
            return SortColumn?.ToLower() switch
            {
                "AmountChanged" => WalletHistory => WalletHistory.AmountChanged,
                "Description" => WalletHistory => WalletHistory.Description,
                "DateLogged" => WalletHistory => WalletHistory.DateLogged,
                _ => WalletHistory => WalletHistory.Id

            };
        }

    }
}
