using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Cursus.Service.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IInstructorInfoRepository _instructorInfoRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IAdminRepository adminRepository, IInstructorInfoRepository instructorInfoRepository, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _adminRepository = adminRepository;
            _instructorInfoRepository = instructorInfoRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AcceptPayout(int transactionId)
        {
            var transaction = await _unitOfWork.TransactionRepository.GetAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                throw new KeyNotFoundException("Transaction not found");
            }

            if (!transaction.Description.Contains("payout"))
            {
                throw new BadHttpRequestException("Can not confirm this transaction!");
            }

            transaction.Status = Data.Enums.TransactionStatus.Completed;

            var instructorWallet = await _unitOfWork.WalletRepository.GetAsync(w => w.UserId == transaction.UserId);

            instructorWallet.Balance -= transaction.Amount;

            await _unitOfWork.SaveChanges();

            return true;
        }

        public async Task<bool> AdminComments(string userId, string comment)
        {
            return await _adminRepository.AdminComments(userId, comment);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUser()
        {
            return await _adminRepository.GetAllAsync();
        }

        public async Task<Dictionary<string, object>> GetInformationInstructor(int instructorId)
        {
            var userId = await _unitOfWork.InstructorInfoRepository.GetAsync(i => i.Id == instructorId);
            if (userId == null) return new Dictionary<string, object> { { "Error", "Instructor not found" } };

            var instructorWallet = await _unitOfWork.WalletRepository.GetAsync(w => w.UserId == userId.UserId);
            var instructor = await _adminRepository.GetInformationInstructorAsync(instructorId);

            var details = new Dictionary<string, object>
                {
                    { "UserName", instructor.UserName ?? string.Empty },
                    { "Email", instructor.Email ?? string.Empty },
                    { "PhoneNumber", instructor.PhoneNumber ?? string.Empty },
                    { "AdminComment", instructor.AdminComment ?? string.Empty },
                    { "TotalEarning", instructor.TotalEarning },
                    { "TotalCourses", await _instructorInfoRepository.TotalCourse(instructorId) },
                    { "TotalActiveCourses", await _instructorInfoRepository.TotalActiveCourse(instructorId) },
                    { "TotalPayout",  instructorWallet.Balance },
                    { "AverageRating", await _instructorInfoRepository.RatingNumber(instructorId) }
                };

            return details;
        }






        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            return await _adminRepository.ToggleUserStatusAsync(userId);
        }


    }
}
