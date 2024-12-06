using Cursus.Data.Entities;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IPlatformWalletRepository
    {
        Task<PlatformWallet> GetPlatformWallet();
    }
}
