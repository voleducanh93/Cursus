using Cursus.Common.Helper;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Demo_PayPal.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Cursus.Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            // DI Services
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ICourseService, CourseService>();
            services.AddTransient<ICourseService, CourseService>();
            services.AddTransient<ICourseProgressService, CourseProgressService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IInstructorService, InstructorService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddScoped<APIResponse>();
            services.AddTransient<IAdminService, AdminService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IStepContentService, StepContentService>();
            services.AddTransient<IAzureBlobStorageService, AzureBlobStorageService>();
            services.AddTransient<IStatisticService, StatisticService>();
            services.AddTransient<IStepCommentService, StepCommentService>();
            services.AddTransient<IBookmarkService, BookmarkService>();
            services.AddTransient<IStepService, StepService>();
            services.AddTransient<IReasonService, ReasonService>();
            services.AddTransient<ICartService, CartService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<ICartItemsService, CartItemsService>();
            services.AddTransient<ICertificateService, CertificateService>();
            services.AddTransient<ICourseCommentService, CourseCommentService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IWalletService, WalletService>();
            services.AddTransient<IPayoutRequestService, PayoutRequestService>();
            services.AddTransient<IWalletHistoryService, WalletHistoryService>();
            services.AddTransient<IInstructorDashboardService, InstructorDashboardService>();
            services.AddTransient<IArchivedTransactionService, ArchivedTransactionService>();
            services.AddTransient<IStatisticsNotificationService, StatisticsNotificationService>();
            services.AddTransient<IVoucherService, VoucherService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IAdminDashboardService, AdminDashboardService>();
            services.AddTransient<IHomePageService, HomePageService>();
            services.AddTransient<ITermPolicyService, TermPolicyService>();
            services.AddTransient<IPrivacyPolicyService, PrivacyPolicyService>();
            services.AddTransient<IMessageService, MessageService>();
            services.AddTransient<IInstructorCertificateService, InstructorCertificateService>();
            return services;
        }
    }
}
