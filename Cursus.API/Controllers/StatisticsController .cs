using Cursus.API.Hubs;
using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticService _statisticService;
    private readonly IHubContext<StatisticsHub> _hubContext;
    private readonly APIResponse _response;

    public StatisticsController(IHubContext<StatisticsHub> hubContext, IStatisticService statisticService)
    {
        _hubContext = hubContext;
        _response = new APIResponse();
        _statisticService = statisticService;
    }

    [HttpGet("SalesAndRevenue-statistics")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> UpdateStatistics(DateTime? startDate, DateTime? endDate)
    {
        // Lấy số liệu từ Service
        
        var (totalSales, salesChange, totalRevenue, revenueChange)  = await _statisticService.GetStatisticsAsync(startDate,endDate);

        var result = new StatisticsResponseDTO
        {
            TotalSales = totalSales,
            TotalRevenues = (double)totalRevenue,
            ChangePercentageSales = salesChange,
            ChangePercentageRevenues = revenueChange,
        };

       

        _response.Result = result;
        _response.IsSuccess = true;
        _response.StatusCode = HttpStatusCode.OK;

        return Ok(_response);
    }

    [HttpGet("Course-statistics")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> CourseStatistics()
    {
        var (totalCourses, activeCourse, inactiveCourse) = await _statisticService.GetCourseStatisticsAsync();

        var result = new StatisticsCourseResponseDTO
        {
            TotalCourse = totalCourses,
            ActiveCourse = activeCourse,
            InActiveCourse = inactiveCourse
        };

       

        _response.Result = result;
        _response.IsSuccess = true;
        _response.StatusCode = HttpStatusCode.OK;

        return Ok(_response);
    }

    [HttpGet("Order-statistics")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> OrderStatistics(DateTime? startDate, DateTime? endDate)
    {
        var orderStatisticsDto = await _statisticService.GetOrderStatisticsAsync(startDate, endDate);

        await _hubContext.Clients.All.SendAsync("ReceiveStatisticsOrderUpdate", orderStatisticsDto);
        _response.Result = orderStatisticsDto;
        _response.IsSuccess = true;
        _response.StatusCode = HttpStatusCode.OK;

        return Ok(_response);
    }

    [HttpGet("monthly-overview")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> GetMonthlyOverview(DateTime? startDate)
    {
        var monthlyStatistics = await _statisticService.GetMonthlyStatisticsAsync(startDate);
        _response.Result = monthlyStatistics;
        _response.IsSuccess = true;
        _response.StatusCode = HttpStatusCode.OK;

        return Ok(_response);
    }
    [HttpGet("top-revenue")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> GetTopInstructorsByRevenue([FromQuery] int topN, DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 10)
    {
        var result = await _statisticService.GetTopInstructorsByRevenueAsync(topN, startDate, endDate, pageNumber, pageSize);
        _response.Result = result;
        _response.IsSuccess = true;
        _response.StatusCode = HttpStatusCode.OK;

        return Ok(_response);
    }

    [HttpGet("top-courses")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public async Task<IActionResult> GetTopInstructorsByCourses([FromQuery] int topN, DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 10)
    {
        var result = await _statisticService.GetTopInstructorsByCoursesAsync(topN, startDate, endDate, pageNumber, pageSize);
        _response.Result = result;
        _response.IsSuccess = true;
        _response.StatusCode = HttpStatusCode.OK;

        return Ok(_response);
    }
}
