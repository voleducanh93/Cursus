using Cursus.Data.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IBookmarkService
    {
        Task<IEnumerable<BookmarkDTO>> GetFilteredAndSortedBookmarksAsync(string userId, string? sortBy, string sortOrder);

        Task<CourseDTO> GetCourseDetailsAsync(int courseId);

        Task CreateBookmarkAsync(string userId, int courseId);
    }
}
