using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Common.Middleware.AuthorizeHandler
{
    public class IsFPTAdminHandler : AuthorizationHandler<IsFPTAdminRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsFPTAdminRequirement requirement)
        {
            
            var subClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            if (subClaim != null && subClaim.Value.EndsWith("@fpt.edu.vn") && context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
