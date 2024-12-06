using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Common.Middleware.AuthorizeHandler
{
    public class IsFPTAdminRequirement : IAuthorizationRequirement
    {
        public string RequiredEmailDomain { get; }

        public IsFPTAdminRequirement(string requiredEmailDomain)
        {
            RequiredEmailDomain = requiredEmailDomain;
        }
    }
}
