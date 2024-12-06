using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Data.Enum;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
	public interface IOrderRepository : IRepository<Order>
	{
        Task<Order> GetOrderWithCartAndItemsAsync(int orderId);
        Task UpdateOrderStatus(int orderId,  OrderStatus newStatus);
        Task<List<Order>> GetOrderHistory(string userId);
        Task<int> GetTotalSalesAsync(DateTime? startDate, DateTime? endDate);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate, DateTime? endDate);
        Task<(int totalSales, decimal totalRevenue)> GetDashboardMetricsAsync(DateTime? startDate, DateTime? endDate);
        Task<((int currentSales, decimal currentRevenue), (int previousSales, decimal previousRevenue))>GetDashboardMetricsAsync(DateTime? currentStartDate, DateTime? currentEndDate, DateTime? previousStartDate, DateTime? previousEndDate);
       

        Task<int> GetTotalOrderStatus(DateTime? startDate, DateTime? endDate, OrderStatus status);
        Task<List<InstructorStatisticDTO>> GetTopInstructorsByRevenueAsync(int topN, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);
        Task<List<InstructorStatisticDTO>> GetTopInstructorsByCoursesAsync(int topN, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);

    }
}
