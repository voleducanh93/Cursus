using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class CourseProgressService : ICourseProgressService
    {
        private readonly IProgressRepository _progressRepository;
        private readonly ICourseProgressRepository _courseProgressRepository;
        public CourseProgressService(IProgressRepository progressRepository, ICourseProgressRepository courseProgressRepository)
        {
            _progressRepository = progressRepository;
            _courseProgressRepository = courseProgressRepository;
        }

      
        public async Task<IEnumerable<int>> GetRegisteredCourseIdsAsync(string userId)
        {
            var courseProgressList = (await _progressRepository.GetAllAsync()).AsQueryable();
            return courseProgressList
                .Where(p => p.UserId == userId)
                .Select(p => p.CourseId)
                .ToList();
        }

        public async Task<double> TrackingProgressAsync(string userId, int courseId)
        {
            var (total, completed) = await _courseProgressRepository.TrackingProgress(userId, courseId);
            return (completed / total) * 100;

        }
    }
    
}
