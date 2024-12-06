using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Cursus.Common.Middleware
{
    public class UnauthorizedExceptionHandler : IExceptionHandler
    {
        public UnauthorizedExceptionHandler()
        {

        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
           if (exception is UnauthorizedAccessException)
           {
                var details = new ProblemDetails()
                {
                    Detail = $"An error occurred: {exception.Message}",
                    Instance = "Request",
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Unauthorized Access",
                    Type = "https://httpstatuses.com/403"
                };

                var response = JsonSerializer.Serialize(details);

                httpContext.Response.ContentType = "application/json";

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

                await httpContext.Response.WriteAsync(response, cancellationToken);
                return true;
           }
            return false;
        }
    }
}
