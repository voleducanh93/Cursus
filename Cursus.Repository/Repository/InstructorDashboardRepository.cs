using Cursus.Data.DTO;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class InstructorDashboardRepository : IInstructorDashboardRepository
    {
        private readonly CursusDbContext _context; // Replace with your actual DbContext

        public InstructorDashboardRepository(CursusDbContext context)
        {
            _context = context;
        }

        // Method to get the instructor dashboard details
        public async Task<InstructorDashboardDTO> GetInstructorDashboardAsync(int instructorId)
        {
            var courses = await _context.Courses
                .Include(course => course.InstructorInfo)
                .Where(course => course.InstructorInfo.Id == instructorId && course.Status)
                .ToListAsync();

            var totalPotentialEarnings = courses.Sum(course => course.Price);
            var instructorInfo = await _context.InstructorInfos
            .Where(p => p.Id == instructorId)
            .Select(i => new
            {
             i.TotalEarning
            })
            .FirstOrDefaultAsync();

            var totalEarnings = instructorInfo?.TotalEarning ?? 0;

            return new InstructorDashboardDTO
            {
                TotalPotentialEarnings = totalPotentialEarnings,
                TotalCourses = courses.Count,
                TotalEarnings = totalEarnings
            };
        }
        private async Task<double> CalculateEarnings(int courseId)
        {
        
            var numberOfEnrollments = await _context.CourseProgresses
                .Where(cp => cp.CourseId == courseId)
                .Select(cp => cp.UserId)
                .Distinct()
                .CountAsync();

          
            var course = await _context.Courses.FindAsync(courseId);

            return course != null ? course.Price * numberOfEnrollments : 0;
        }

        public async Task<List<CourseEarningsDTO>> GetCourseEarningsAsync(int instructorId)
        {
            var courses = await _context.Courses
                .Include(course => course.InstructorInfo)
                .Where(course => course.InstructorInfo.Id == instructorId && course.Status)
                .ToListAsync();

            var courseEarningsList = new List<CourseEarningsDTO>();

            foreach (var course in courses)
            {
                
                var earnings = await CalculateEarnings(course.Id);

                courseEarningsList.Add(new CourseEarningsDTO
                {
                    Status = course.Status ? "Active" : "Deactive",
                    ShortSummary = course.Description.Length > 100 ? course.Description.Substring(0, 100) : course.Description,
                    Price = course.Price,
                    Earnings = earnings  
                });
            }

            return courseEarningsList;
        }

    }
}
