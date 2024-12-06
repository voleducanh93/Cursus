using Cursus.Data.DTO;
using Cursus.Data.Entities;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IWalletService
    {
        public Task<WalletDTO> CreateWallet (string userId);
        public Task<WalletDTO> GetWalletByUserId (string userId);
        public Task AddMoneyToInstructorWallet (string userId, double amount);
        public Task CreatePayout (string userId, double amount);
    }
}
