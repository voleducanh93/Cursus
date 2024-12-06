using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class InstructorRepository : Repository<InstructorInfo>, IInstructorInfoRepository
    { 
        private readonly CursusDbContext _dbContext;

        public InstructorRepository(CursusDbContext dbContext ) :base (dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task DeleteAsync(int id)
        {
            var instructor = await _dbContext.InstructorInfos.FindAsync(id);
            if (instructor != null)
            {
                _dbContext.InstructorInfos.Remove(instructor);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<InstructorInfo>> GetAllInstructorsAsync()
        {
            return await _dbContext.InstructorInfos.Include(i => i.User).ToListAsync();
        }

        public async Task<InstructorInfo> GetByIDAsync(int id)
        {
           return await _dbContext.InstructorInfos.Include(i => i.User).FirstOrDefaultAsync(i => i.Id == id) ?? throw new KeyNotFoundException("Instructor not found");
        }

        public async Task<IEnumerable<InstructorInfo>> GettAllAsync()
        {
           return await _dbContext.InstructorInfos.Include(i => i.User).ToListAsync();
        }
        public async Task<IEnumerable<InstructorInfo>> GetAllInstructors()
        {
            return await _dbContext.InstructorInfos
                                   .Include(i => i.User)
                                   .ToListAsync();
        }

        public async Task UpdateAsync(InstructorInfo instructorInfo)
        {
            _dbContext.InstructorInfos.Update(instructorInfo);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddAsync(InstructorInfo instructorInfo)
        {
            await _dbContext.InstructorInfos.AddAsync(instructorInfo);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> TotalCourse(int instructorId)
        {
            return await _dbContext.Courses
            .Where(c => c.InstructorInfoId == instructorId)
            .CountAsync();
        }

        public async Task<int> TotalActiveCourse(int instructorId)
        {

            return await _dbContext.Courses
             .Where(c => c.InstructorInfoId == instructorId && c.Status == true)
             .CountAsync();
        }
        public async Task<double> TotalPayout(int id)
        {
            return await _dbContext.InstructorInfos
                .Where(i => i.Id == id) // lọc theo InstructorId nếu cần
                .SumAsync(i => i.TotalEarning * 0.7);
        }

        public async Task<double> RatingNumber(int id)
        {
                    var ratings = await _dbContext.Courses
                .Where(c => c.InstructorInfoId == id && c.Status == true)
                .Select(c => c.Rating)
                .ToListAsync();

            if (ratings.Count == 0)
                return 0;

            double averageRating = ratings.Average();
            return averageRating;
        }

    }
}
