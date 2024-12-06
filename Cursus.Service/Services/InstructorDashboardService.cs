using AutoMapper;
using Cursus.Data.DTO;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class InstructorDashboardService : IInstructorDashboardService
    {
        private readonly IInstructorDashboardRepository _instructorDashboardRepository;
        private readonly IMapper _mapper;
        public InstructorDashboardService(IInstructorDashboardRepository instructorDashboardRepository, IMapper mapper)
        {
            _instructorDashboardRepository = instructorDashboardRepository;
            _mapper = mapper;
        }

        public async Task<InstructorDashboardDTO> GetInstructorDashboardAsync(int instructorId)
        {
            var dashboardData = await _instructorDashboardRepository.GetInstructorDashboardAsync(instructorId);
            return dashboardData;
        }

        public async Task<List<CourseEarningsDTO>> GetCourseEarningsAsync(int instructorId)
        {
            var courseEarnings = await _instructorDashboardRepository.GetCourseEarningsAsync(instructorId);
            courseEarnings.ForEach(e => e.PotentialEarnings = (e.Price * 0.475) * 12);
            return _mapper.Map<List<CourseEarningsDTO>>(courseEarnings);
        }
    }

}
