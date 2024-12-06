using Cursus.Common.Middleware.AuthorizeHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Cursus.Common.Middleware
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExceptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler<UnauthorizedExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>(); 
            services.AddExceptionHandler<KeyNotFoundExceptionHandler>();
            services.AddExceptionHandler<NotImplementExceptionHandler>();
            services.AddExceptionHandler<BadRequestExceptionHandler>();
            services.AddExceptionHandler<EmailNotConfirmedExceptionHandler>();
            services.AddSingleton<IAuthorizationHandler, IsFPTAdminHandler>();
            return services;
        }
    }
}
