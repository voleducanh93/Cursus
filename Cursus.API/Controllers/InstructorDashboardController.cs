using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorDashboardController : ControllerBase
    {
        private readonly IInstructorDashboardService _instructorDashboardService;

        public InstructorDashboardController(IInstructorDashboardService instructorDashboardService)
        {
            _instructorDashboardService = instructorDashboardService;
        }
        /// <summary>
        /// GetInstructorDashboard
        /// </summary>
        /// <param name="instructorId"></param>
        /// <returns></returns>
        [HttpGet("{instructorId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorDashboard(int instructorId)
        {
            var dashboardData = await _instructorDashboardService.GetInstructorDashboardAsync(instructorId);
            if (dashboardData == null)
            {
                return NotFound(new { message = "Dashboard data not found for the specified instructor." });
            }
            return Ok(dashboardData);
        }
        /// <summary>
        /// GetCourseEarnings
        /// </summary>
        /// <param name="instructorId"></param>
        /// <returns></returns>
        [HttpGet("earnings/{instructorId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<IActionResult> GetCourseEarnings(int instructorId)
        {
            var earningsData = await _instructorDashboardService.GetCourseEarningsAsync(instructorId);
            return Ok(earningsData);
        }
    }

}
