using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Cursus.Service.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WalletService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddMoneyToInstructorWallet(string userId, double amount)
        {
            var wallet = _unitOfWork.WalletRepository.GetAsync(w => w.UserId == userId).Result ?? throw new KeyNotFoundException("Wallet not found");

            wallet.Balance += amount;

            await _unitOfWork.WalletRepository.UpdateAsync(wallet);

            await _unitOfWork.SaveChanges();
        }

        public async Task CreatePayout(string userId, double amount)
        {
            Transaction transaction = new()
            {
                TransactionId = await _unitOfWork.TransactionRepository.GetNextTransactionId(),
                UserId = userId,
                Amount = amount,
                DateCreated = DateTime.Now,
                Status = Data.Enums.TransactionStatus.Pending,
                Description = $"User {userId} payout"
            };

            var wallet = await _unitOfWork.WalletRepository.GetAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                throw new KeyNotFoundException("Wallet not found");
            }

            if (wallet.Balance < amount)
            {
                throw new BadHttpRequestException($"Your wallet do not have enough money for a {amount} payout");
            }



            await _unitOfWork.TransactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChanges();

            var instructor = await _unitOfWork.InstructorInfoRepository.GetAsync(x => x.UserId == userId);
            if (instructor == null)
                throw new KeyNotFoundException("Instructor Not Found");

            PayoutRequest payoutRequest = new()
            {
                InstructorId = instructor.Id,
                TransactionId = transaction.TransactionId,
                CreatedDate = DateTime.Now,
                PayoutRequestStatus = Data.Enums.PayoutRequestStatus.Pending,
            };

            await _unitOfWork.PayoutRequestRepository.AddAsync(payoutRequest);
            await _unitOfWork.SaveChanges();

        }

        public async Task<WalletDTO> CreateWallet(string userId)
        {

            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (await _unitOfWork.WalletRepository.GetAsync(w => w.UserId == userId) != null)
            {
                throw new InvalidOperationException("Wallet already exists");
            }

            var wallet = new Wallet
            {
                UserId = userId,
                User = user,
                Balance = 0,
                DateCreated = DateTime.Now
            };

            await _unitOfWork.WalletRepository.AddAsync(wallet);

            await _unitOfWork.SaveChanges();

            var walletDTO = _mapper.Map<WalletDTO>(wallet);

            return walletDTO;
          
        }


        public async Task<WalletDTO> GetWalletByUserId(string userId)
        {
            var wallet = await _unitOfWork.WalletRepository.GetAsync(w => w.UserId == userId) ?? throw new KeyNotFoundException("Wallet not found");

            var walletDTO = _mapper.Map<WalletDTO>(wallet);

            return walletDTO;
        }
    }
}
