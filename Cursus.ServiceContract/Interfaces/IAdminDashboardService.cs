using Cursus.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<List<PurchaseCourseOverviewDTO>> GetTopPurchasedCourses(int year, string period);
        Task<List<PurchaseCourseOverviewDTO>> GetWorstRatedCourses(int year, string period);

        Task<int> GetTotalUsersAsync();
        Task<int> GetTotalInstructorsAsync();
    }
}
