using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly CursusDbContext _db;

        public ICategoryRepository CategoryRepository { get; }
        public IInstructorInfoRepository InstructorInfoRepository { get; private set; }
        public ICourseRepository CourseRepository { get; }
        public IStepRepository StepRepository { get; }
        public IUserRepository UserRepository { get; }
        public IStepContentRepository StepContentRepository { get; }
        public UserManager<ApplicationUser> UserManager { get; }
        public ICourseCommentRepository CourseCommentRepository { get; }
        public ITransactionRepository TransactionRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public IStepCommentRepository StepCommentRepository { get; }
        public IRefreshTokenRepository RefreshTokenRepository { get; }
        public IProgressRepository ProgressRepository { get; }
        public ICartRepository CartRepository { get; }
        public ICourseProgressRepository CourseProgressRepository { get; }
        public ICartItemsRepository CartItemsRepository { get; }
        public IBookmarkRepository BookmarkRepository { get; }
        public IReasonRepository ReasonRepository{ get; }
        public IWalletRepository WalletRepository { get;}
        public ICertificateRepository CertificateRepository { get; }
        public IPlatformWalletRepository PlatformWalletRepository { get; }
        public IPayoutRequestRepository PayoutRequestRepository { get; }
        public IWalletHistoryRepository WalletHistoryRepository { get; }
        public IArchivedTransactionRepository ArchivedTransactionRepository { get; }
        public IInstructorDashboardRepository InstructorDashboardRepository { get; }
        public IVoucherRepository VoucherRepository { get; }

        public INotificationRepository NotificationRepository { get; }
        public IAdminDashboardRepository AdminDashboardRepository { get; }
        public IInstructorCertificateRepository InstructorCertificateRepository { get; }

        public ITermPolicyRepository TermPolicyRepository { get; }
        public IHomePageRepository HomePageRepository { get; }
        public IPrivacyPolicyRepository PrivacyPolicyRepository { get; }

        public IMessageRepository MessageRepository { get; }

        public UnitOfWork(CursusDbContext db, ICategoryRepository categoryRepository, ICourseRepository courseRepository, IStepRepository stepRepository, IUserRepository userRepository, IStepContentRepository stepContentRepository, IInstructorInfoRepository instructorInfoRepository, UserManager<ApplicationUser> userManager, ICourseCommentRepository courseCommentRepository, IRefreshTokenRepository refreshTokenRepository, IStepCommentRepository stepCommentRepository, IProgressRepository progressRepository, ICartRepository cartRepository, IOrderRepository orderRepository, ICourseProgressRepository courseProgressRepository, IBookmarkRepository bookmarkRepository, ICartItemsRepository cartItemsRepository, ITransactionRepository transactionRepository, IReasonRepository reasonRepository, IWalletRepository walletRepository, IPlatformWalletRepository platformWalletRepository, IPayoutRequestRepository payoutRequestRepository, IWalletHistoryRepository walletHistoryRepository, IInstructorDashboardRepository instructorDashboardRepository, IArchivedTransactionRepository archivedTransactionRepository, ICertificateRepository certificateRepository, INotificationRepository notificationRepository, IAdminDashboardRepository adminDashboardRepository, ITermPolicyRepository termPolicyRepository, IHomePageRepository homePageRepository, IPrivacyPolicyRepository privacyPolicyRepository, IVoucherRepository voucherRepository, IMessageRepository messageRepository, IInstructorCertificateRepository instructorCertificateRepository)
        {
            _db = db;
            CategoryRepository = categoryRepository;
            CourseRepository = courseRepository;
            StepRepository = stepRepository;
            UserRepository = userRepository;
            InstructorInfoRepository = instructorInfoRepository;
            UserManager = userManager;
            StepContentRepository = stepContentRepository;
            CourseCommentRepository = courseCommentRepository;
            RefreshTokenRepository = refreshTokenRepository;
            StepCommentRepository = stepCommentRepository;
            RefreshTokenRepository = refreshTokenRepository;
            ProgressRepository = progressRepository;
            TransactionRepository = transactionRepository;
            OrderRepository = orderRepository;
            CartRepository = cartRepository;
            CourseProgressRepository = courseProgressRepository;
            CartItemsRepository = cartItemsRepository;
            BookmarkRepository = bookmarkRepository;
            CartItemsRepository = cartItemsRepository;
            TransactionRepository = transactionRepository;
            ReasonRepository = reasonRepository;
            WalletRepository = walletRepository;
            PlatformWalletRepository = platformWalletRepository;
            CertificateRepository = certificateRepository;
            PayoutRequestRepository = payoutRequestRepository;
            WalletHistoryRepository = walletHistoryRepository;
            InstructorDashboardRepository = instructorDashboardRepository;
            ArchivedTransactionRepository = archivedTransactionRepository;
            AdminDashboardRepository = adminDashboardRepository;
            NotificationRepository = notificationRepository;
            TermPolicyRepository = termPolicyRepository;
            HomePageRepository = homePageRepository;
            PrivacyPolicyRepository = privacyPolicyRepository;
            VoucherRepository = voucherRepository;
            InstructorCertificateRepository = instructorCertificateRepository;
            MessageRepository = messageRepository;
        }


        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task SaveChanges()
        {
            await _db.SaveChangesAsync();
        }
    }
}

