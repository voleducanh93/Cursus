using Cursus.Data.Entities;

namespace Cursus.RepositoryContract.Interfaces
{
	public interface ICourseProgressRepository : IRepository<CourseProgress>
	{
        Task<(double total, double completed)> TrackingProgress(string userId, int courseId);

    }
}
