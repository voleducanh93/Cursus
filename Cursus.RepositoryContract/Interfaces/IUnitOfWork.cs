using Cursus.ServiceContract.Interfaces;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICategoryRepository CategoryRepository { get; }
        IUserRepository UserRepository { get; }
        IInstructorInfoRepository InstructorInfoRepository { get; }
        ICourseRepository CourseRepository { get; }
        IStepRepository StepRepository { get; }
        IStepContentRepository StepContentRepository { get; }
        ICourseCommentRepository CourseCommentRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }
        IProgressRepository ProgressRepository { get; }
        IStepCommentRepository StepCommentRepository { get; }
        ICartItemsRepository CartItemsRepository { get; }
        IBookmarkRepository BookmarkRepository { get; }
        ICartRepository CartRepository { get; }
        IOrderRepository OrderRepository { get; }
        ICourseProgressRepository CourseProgressRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        IReasonRepository ReasonRepository { get; }
        IWalletRepository WalletRepository { get; }
        IPlatformWalletRepository PlatformWalletRepository { get; }
        ICertificateRepository CertificateRepository { get; }
        IPayoutRequestRepository PayoutRequestRepository { get; }
        IWalletHistoryRepository WalletHistoryRepository { get; }
        IInstructorDashboardRepository InstructorDashboardRepository { get; }
        IArchivedTransactionRepository ArchivedTransactionRepository { get; }
        IVoucherRepository VoucherRepository { get; }
        INotificationRepository NotificationRepository { get; }
        IAdminDashboardRepository AdminDashboardRepository { get; }
        IHomePageRepository HomePageRepository { get; }
        ITermPolicyRepository TermPolicyRepository { get; }
        IPrivacyPolicyRepository PrivacyPolicyRepository { get; }
        IMessageRepository MessageRepository { get; }
        IInstructorCertificateRepository InstructorCertificateRepository { get; }   
        Task SaveChanges();
    }
}
