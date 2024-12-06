using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IAdminService
    {
        Task<bool> ToggleUserStatusAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetAllUser();
        Task<bool> AdminComments(string userId, string comment);
        Task<Dictionary<string, object>> GetInformationInstructor(int instructorId);
        Task<bool> AcceptPayout(int transactionId);
    }
}
