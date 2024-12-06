using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IAdminRepository : IRepository<ApplicationUser>
    {
        Task<bool> ToggleUserStatusAsync(string userId);
        
        Task<bool> AdminComments(string userId, string comment);

        Task<(string? UserName, string? Email, string? PhoneNumber, string? AdminComment, double TotalEarning )> GetInformationInstructorAsync(int instructorId);
    }
}
