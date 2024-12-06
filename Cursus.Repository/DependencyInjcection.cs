using Cursus.Repository.Repository;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Cursus.Repository
{
    public static class DependencyInjcection
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<IEmailRepository, EmailRepository>();
            services.AddTransient<IInstructorInfoRepository, InstructorRepository>();
            services.AddTransient<IAdminRepository, AdminRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IStepContentRepository, StepContentRepository>();
            services.AddTransient<ICourseRepository, CourseRepository>();
			services.AddTransient<IStepRepository, StepRepository>();
            services.AddTransient<ICourseCommentRepository, CourseCommentRepository>();
            services.AddTransient<ICourseRepository, CourseRepository>();
            services.AddTransient<IProgressRepository, ProgressRepository>();
            services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<IStepCommentRepository, StepCommentRepository>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IBookmarkRepository, BookmarkRepository>();
            services.AddTransient<IStepRepository, StepRepository>();
            services.AddTransient<IReasonRepository, ReasonRepository>();
			services.AddTransient<ICartRepository, CartRepository>();
			services.AddTransient<IOrderRepository, OrderRepository>();
			services.AddTransient<ICourseProgressRepository, CourseProgressRepository>();
            services.AddTransient<ICartItemsRepository, CartItemsRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<ICartRepository, CartRepository>();
            services.AddTransient<IWalletRepository, WalletRepositoy>();
            services.AddTransient<IPlatformWalletRepository, PlatformWalletRepository>();
            services.AddTransient<ICertificateRepository, CertificateRepository>();
            services.AddTransient<IPayoutRequestRepository, PayoutRequestRepository>();
            services.AddTransient<IWalletHistoryRepository, WalletHistoryRepository>();
            services.AddTransient<ITrackingProgressRepository, TrackingProgressRepository>();
            services.AddTransient<IInstructorDashboardRepository, InstructorDashboardRepository>();
            services.AddTransient<IArchivedTransactionRepository, ArchivedTransactionRepository>();
            services.AddTransient<IVoucherRepository, VoucherRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<IAdminDashboardRepository, AdminDashboardRepository>();
            services.AddTransient<ITermPolicyRepository, TermPolicyRepository>();
            services.AddTransient<IHomePageRepository, HomePageRepository>();
            services.AddTransient<IPrivacyPolicyRepository, PrivacyPolicyRepository>();
            services.AddTransient<IInstructorCertificateRepository, InstructorCertificateRepository>();
            services.AddTransient<IMessageRepository, MessageRepository>();
            // DI UnitOfWork
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
