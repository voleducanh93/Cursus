using Cursus.Data.DTO;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly CursusDbContext _context;

        public AdminDashboardRepository(CursusDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseCourseOverviewDTO>> GetTopPurchasedCourses(int year, string period)
        {
            var cartsQuery = _context.Cart
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course) 
                .Where(c => c.IsPurchased) 
                .Where(c => c.CartItems.Any()) 
                .AsQueryable();

          
            if (period.ToLower() == "month")
            {
                cartsQuery = cartsQuery.Where(c => c.CartItems
                    .Any(ci => ci.Course.DateCreated.Year == year &&
                               ci.Course.DateCreated.Month >= 1 && ci.Course.DateCreated.Month <= 12));
            }
            else if (period.ToLower() == "quarter")
            {
                cartsQuery = cartsQuery.Where(c => c.CartItems
                    .Any(ci => ci.Course.DateCreated.Year == year &&
                               ci.Course.DateCreated.Month >= 1 && ci.Course.DateCreated.Month <= 3));
            }

            var topCourses = await cartsQuery
                .SelectMany(c => c.CartItems)
                .GroupBy(ci => ci.Course.Id)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new PurchaseCourseOverviewDTO
                {
                    Id = g.Key,
                    CourseName = g.FirstOrDefault().Course.Name,
                    Summary = g.FirstOrDefault().Course.Description,
                    Price = g.FirstOrDefault().Course.Price,
                    StepCount = g.FirstOrDefault().Course.Steps.Count,
                    Rating = g.FirstOrDefault().Course.Rating
                })
                .ToListAsync();

            return topCourses;
        }
        public async Task<List<PurchaseCourseOverviewDTO>> GetWorstRatedCourses(int year, string period)
        {
            var coursesQuery = _context.Courses
                .Include(c => c.Steps)
                .Where(c => c.Rating > 0)
                .AsQueryable();

            if (period.ToLower() == "month")
            {
                coursesQuery = coursesQuery.Where(c => c.DateCreated.Year == year &&
                                                        c.DateCreated.Month >= 1 && c.DateCreated.Month <= 12);
            }
            else if (period.ToLower() == "quarter")
            {
                coursesQuery = coursesQuery.Where(c => c.DateCreated.Year == year &&
                                                        c.DateCreated.Month >= 1 && c.DateCreated.Month <= 3);
            }

            var worstCourses = await coursesQuery
                .Select(c => new
                {
                    Course = c,
                    AverageRating = c.Rating
                })
                .OrderBy(r => r.AverageRating)
                .Take(5)
                .Select(r => new PurchaseCourseOverviewDTO
                {
                    Id = r.Course.Id,
                    CourseName = r.Course.Name,
                    Summary = r.Course.Description,
                    Price = r.Course.Price,
                    StepCount = r.Course.Steps.Count,
                    Rating = r.AverageRating
                })
                .ToListAsync();

            return worstCourses;
        }
        public async Task<int> GetTotalUsersAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetTotalInstructorsAsync()
        {
            return await _context.InstructorInfos.CountAsync();
        }
    }
}
