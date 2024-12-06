using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{

    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly CursusDbContext _db;

        public OrderRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }
       

        public async Task<Order> GetOrderWithCartAndItemsAsync(int orderId)
            {
                return await _db.Order
                    .Include(o => o.Cart)
                    .ThenInclude(c => c.CartItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);
            }
        public async Task UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
           
            var order = await _db.Order.FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;
            }
        }
        public async Task<List<Order>> GetOrderHistory(string userId)
        {
            // Lấy tất cả các CartId của userId cụ thể
            var cartIds = await _db.Cart
                .Where(c => c.UserId == userId)
                .Select(c => c.CartId)
                .ToListAsync();

            // Lấy danh sách các đơn hàng có CartId khớp
            return await _db.Order
                .Where(o => cartIds.Contains(o.CartId))
                .ToListAsync();
        }
        public async Task<int> GetTotalSalesAsync(DateTime? startDate, DateTime? endDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("StartDate", startDate);
            parameters.Add("EndDate", endDate);

            await using var connection = _db.Database.GetDbConnection();
            try
            {
                // Kiểm tra và khởi tạo ConnectionString nếu cần
                if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                {
                    connection.ConnectionString = _db.Database.GetConnectionString()
                        ?? throw new InvalidOperationException("ConnectionString is not initialized.");
                }

                // Mở kết nối nếu chưa mở
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // Thực thi stored procedure
                return await connection.ExecuteScalarAsync<int>(
                    "GetTotalSales", parameters, commandType: CommandType.StoredProcedure);
            }
            finally
            {
                // Đảm bảo đóng kết nối sau khi sử dụng
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }
        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate, DateTime? endDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("StartDate", startDate);
            parameters.Add("EndDate", endDate);

            await using var connection = _db.Database.GetDbConnection();
            try
            {
                // Kiểm tra và khởi tạo ConnectionString nếu cần
                if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                {
                    connection.ConnectionString = _db.Database.GetConnectionString()
                        ?? throw new InvalidOperationException("ConnectionString is not initialized.");
                }

                // Mở kết nối nếu chưa mở
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // Thực thi stored procedure
                return await connection.ExecuteScalarAsync<decimal>(
                    "GetTotalRevenue", parameters, commandType: CommandType.StoredProcedure);
            }
            finally
            {
                // Đảm bảo đóng kết nối sau khi sử dụng
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }
        public async Task<(int totalSales, decimal totalRevenue)> GetDashboardMetricsAsync(DateTime? startDate, DateTime? endDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("StartDate", startDate);
            parameters.Add("EndDate", endDate);

            using var connection = new SqlConnection(_db.Database.GetConnectionString());
            return await connection.QueryFirstOrDefaultAsync<(int, decimal)>(
                "GetDashboardMetrics", parameters, commandType: CommandType.StoredProcedure);
        }
        public async Task<int> GetTotalOrderStatus(DateTime? startDate, DateTime? endDate, OrderStatus status)
        {
            var orders = _db.Order.Where(o => o.Status == status);
          
            if (startDate.HasValue)
                orders = orders.Where(o => o.DateCreated >= startDate.Value);

            if (endDate.HasValue)
                orders = orders.Where(o => o.DateCreated <= endDate.Value);

          
            return await orders.CountAsync();
        }

        public async Task<List<InstructorStatisticDTO>> GetTopInstructorsByRevenueAsync(int topN, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            var parameters = new DynamicParameters();
            parameters.Add("TopN", topN);
            parameters.Add("StartDate", startDate);
            parameters.Add("EndDate", endDate);
            parameters.Add("PageNumber", pageNumber);
            parameters.Add("PageSize", pageSize);

            await using var connection = _db.Database.GetDbConnection();
            try
            {
                if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                {
                    connection.ConnectionString = _db.Database.GetConnectionString()
                        ?? throw new InvalidOperationException("ConnectionString is not initialized.");
                }

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var result = await connection.QueryAsync<InstructorStatisticDTO>(
                    "GetTopInstructorsByRevenue", 
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<List<InstructorStatisticDTO>> GetTopInstructorsByCoursesAsync(int topN, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            var parameters = new DynamicParameters();
            parameters.Add("TopN", topN);
            parameters.Add("StartDate", startDate);
            parameters.Add("EndDate", endDate);
            parameters.Add("PageNumber", pageNumber);
            parameters.Add("PageSize", pageSize);

            await using var connection = _db.Database.GetDbConnection();
            try
            {
                if (string.IsNullOrWhiteSpace(connection.ConnectionString))
                {
                    connection.ConnectionString = _db.Database.GetConnectionString()
                        ?? throw new InvalidOperationException("ConnectionString is not initialized.");
                }

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var result = await connection.QueryAsync<InstructorStatisticDTO>(
                    "GetTopInstructorsByCourses",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<((int currentSales, decimal currentRevenue), (int previousSales, decimal previousRevenue))> GetDashboardMetricsAsync(DateTime? currentStartDate, DateTime? currentEndDate, DateTime? previousStartDate, DateTime? previousEndDate)
        {
            var parameters = new DynamicParameters();
            parameters.Add("CurrentStartDate", currentStartDate ?? (object)DBNull.Value);
            parameters.Add("CurrentEndDate", currentEndDate ?? (object)DBNull.Value);
            parameters.Add("PreviousStartDate", previousStartDate ?? (object)DBNull.Value);
            parameters.Add("PreviousEndDate", previousEndDate ?? (object)DBNull.Value);

            using var connection = new SqlConnection(_db.Database.GetConnectionString());

           
            var result = await connection.QueryFirstOrDefaultAsync<(
                int currentSales, decimal currentRevenue,
                int previousSales, decimal previousRevenue)>(
                "GetDashboardMetrics", parameters, commandType: CommandType.StoredProcedure);

           
            return ((result.currentSales, result.currentRevenue), (result.previousSales, result.previousRevenue));
        }
    }

}
