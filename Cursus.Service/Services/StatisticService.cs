using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Data.Enums;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;

namespace Cursus.Service.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;
        private readonly IInstructorService _instructorService;
        public StatisticService(IMapper mapper, IUnitOfWork unitOfWork, IOrderService orderService, IInstructorService instructorService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _instructorService = instructorService;
        }

        public async Task<List<MonthlyStatisticsDTO>> GetMonthlyStatisticsAsync(DateTime? endDate = null)
        {
          
            DateTime finalEndDate = (endDate ?? DateTime.Now).AddMonths(-1);
            DateTime currentMonthStart = new DateTime(finalEndDate.Year, finalEndDate.Month, 1);

            List<MonthlyStatisticsDTO> monthlyStatistics = new List<MonthlyStatisticsDTO>();

            
            for (int i = 11; i >= 0; i--)
            {
                DateTime monthStart = currentMonthStart.AddMonths(-i);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);           
                var (totalSales, _, totalRevenue, _) = await GetStatisticsAsync(monthStart, monthEnd);

                // Thêm dữ liệu vào danh sách
                monthlyStatistics.Add(new MonthlyStatisticsDTO
                {
                    Month = monthStart.ToString("MMM yyyy"),
                    TotalSales = totalSales,
                    TotalRevenue = (double)totalRevenue
                });
            }
            for (int i = 1; i < monthlyStatistics.Count; i++)
            {
                var currentMonth = monthlyStatistics[i];
                var previousMonth = monthlyStatistics[i - 1];
                currentMonth.SalesGrowth = CalculateGrowthPercentage(currentMonth.TotalSales, previousMonth.TotalSales);
                currentMonth.RevenueGrowth = CalculateGrowthPercentage((int)currentMonth.TotalRevenue, (int)previousMonth.TotalRevenue);
            }

            return monthlyStatistics;
        }
        private double CalculateGrowthPercentage(int currentSales, int previousSales)
        {
            if (previousSales == 0)
            {
                return currentSales > 0 ? 100 : 0;
            }

            if (currentSales == previousSales)
            {
                return 0;
            }

           
            double growthPercentage = ((double)(currentSales - previousSales) / previousSales) * 100;

           
            return double.IsInfinity(growthPercentage) || double.IsNaN(growthPercentage) ? 0 : growthPercentage;
        }

        
       

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            int orderTotalPaided = await _unitOfWork.OrderRepository.GetTotalOrderStatus(startDate, endDate, OrderStatus.Paid);
            int orderTotalFailed = await _unitOfWork.OrderRepository.GetTotalOrderStatus(startDate, endDate, OrderStatus.Failed);
            int orderTotalPending = await _unitOfWork.OrderRepository.GetTotalOrderStatus(startDate, endDate, OrderStatus.PendingPayment);
            int total = orderTotalFailed + orderTotalPending + orderTotalPaided;

            // Tạo và trả về đối tượng DTO
            var result = new OrderStatisticsDto
            {
                TotalOrder = total,
                PendingOrder = orderTotalPending,
                FailedOrder = orderTotalFailed,
                PaidedOrder = orderTotalPaided
            };

            return result;
        }
        public async Task<(int totalCourses, int activeCourses, int inactiveCourses)> GetCourseStatisticsAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync(p => p.IsApprove == ApproveStatus.Approved);

            int totalCourses = courses.Count();
            int activeCourses = courses.Where(p => p.Status == true).Count();
            int inactiveCourses = courses.Where(p => p.Status == false).Count(); ;

            return (totalCourses, activeCourses, inactiveCourses);

        }

        public async Task<List<InstructorStatisticDTO>> GetTopInstructorsByRevenueAsync(int topN, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            return await _unitOfWork.OrderRepository.GetTopInstructorsByRevenueAsync(topN, startDate, endDate, pageNumber, pageSize);
        }

        public async Task<List<InstructorStatisticDTO>> GetTopInstructorsByCoursesAsync(int topN, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            return await _unitOfWork.OrderRepository.GetTopInstructorsByCoursesAsync(topN, startDate, endDate, pageNumber, pageSize);
        }

        public async Task<(int totalSales, double salesChangePercentage, decimal totalRevenue, double revenueChangePercentage)>
    GetStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            DateTime currentStartDate = startDate ?? DateTime.Now.AddMonths(-1);
            DateTime currentEndDate = endDate ?? DateTime.Now;
            DateTime previousStartDate = currentStartDate.AddMonths(-1);
            DateTime previousEndDate = currentEndDate.AddMonths(-1);

            
            var ((currentSales, currentRevenue), (previousSales, previousRevenue)) =
                await _unitOfWork.OrderRepository.GetDashboardMetricsAsync(currentStartDate, currentEndDate, previousStartDate, previousEndDate);

            
            double salesChangePercentage = previousSales == 0 ? (currentSales > 0 ? 100 : 0) : ((double)(currentSales - previousSales) / previousSales) * 100;

           
            double revenueChangePercentage = previousRevenue == 0 ? (currentRevenue > 0 ? 100 : 0) : ((double)(currentRevenue - previousRevenue) / (double)previousRevenue) * 100;

           
            if (double.IsNaN(salesChangePercentage) || double.IsInfinity(salesChangePercentage))
                salesChangePercentage = 0;

            if (double.IsNaN(revenueChangePercentage) || double.IsInfinity(revenueChangePercentage))
                revenueChangePercentage = 0;

            return (
                currentSales,
                Math.Round(salesChangePercentage, 2),
                currentRevenue,
                Math.Round(revenueChangePercentage, 2)
            );
        }

    }
}
