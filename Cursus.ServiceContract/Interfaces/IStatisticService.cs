using Cursus.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IStatisticService
    {
        Task<List<MonthlyStatisticsDTO>> GetMonthlyStatisticsAsync(DateTime? startDate);
    
        Task<(int totalSales, double salesChangePercentage, decimal totalRevenue, double revenueChangePercentage)>
             GetStatisticsAsync(DateTime? startDate, DateTime? endDate);
        

        Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? startDate, DateTime? endDate);
        Task<(int totalCourses, int activeCourses, int inactiveCourses)> GetCourseStatisticsAsync();
        Task<List<InstructorStatisticDTO>> GetTopInstructorsByRevenueAsync(int topN, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);
        Task<List<InstructorStatisticDTO>> GetTopInstructorsByCoursesAsync(int topN, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);

    }
}
