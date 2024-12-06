using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Cursus.Common.Middleware
{
    public class NotImplementExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is NotImplementedException)
            {
                var details = new ProblemDetails()
                {
                    Detail = $"An error occurred: {exception.Message}",
                    Instance = "API",
                    Status = StatusCodes.Status501NotImplemented,
                    Title = "Not Implement",
                    Type = "https://httpstatuses.com/501"
                };

                var response = JsonSerializer.Serialize(details);

                httpContext.Response.ContentType = "application/json";

                httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;

                await httpContext.Response.WriteAsync(response, cancellationToken);

                return true;
            }

            return false;
        }
    }
}
