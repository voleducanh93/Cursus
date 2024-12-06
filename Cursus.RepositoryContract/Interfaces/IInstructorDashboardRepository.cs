using Cursus.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IInstructorDashboardRepository
    {
        Task<InstructorDashboardDTO> GetInstructorDashboardAsync(int instructorId);
        Task<List<CourseEarningsDTO>> GetCourseEarningsAsync(int instructorId);
    }

}
