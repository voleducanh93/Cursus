using Cursus.Common.Helper;
using Cursus.Data.DTO;

namespace Cursus.ServiceContract.Interfaces
{
	public interface ICourseService
	{
        Task<PageListResponse<CourseDTO>> GetCoursesAsync(string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        int page = 1,
        int pageSize = 20);
        

        Task<PageListResponse<CourseDTO>> GetRegisteredCoursesByUserIdAsync(string userId, int page = 1, int pageSize = 20);
		Task<CourseDTO> CreateCourseWithSteps(CourseCreateDTO courseDTO);

        Task<CourseDTO> UpdateCourse(CourseUpdateDTO courseUpdateDTO);

        Task<bool> DeleteCourse(int courseId);

        Task<CourseDTO> GetCourseByIdAsync(int courseId);
        Task <CourseDTO> CourseApproval(int courseId, bool choice);

        Task<APIResponse> UpdateCourseStatus(CourseUpdateStatusDTO courseUpdateStatusDTO);

        Task<TotalEarningPotentitalDTO> CaculatePotentialEarnings(int courseId, int months);
       

    }
}
