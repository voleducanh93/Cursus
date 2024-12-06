using Cursus.Data.Entities;
using Cursus.Data.Enums;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        private readonly CursusDbContext _db;
        public CourseRepository(CursusDbContext db) : base(db) => _db = db;
        public async Task<bool> AnyAsync(Expression<Func<Course, bool>> predicate)
        {
            return await _db.Set<Course>().AnyAsync(predicate);
        }
        public async Task<Course> GetAllIncludeStepsAsync(int courseId)
        {
            var course = await _db.Set<Course>().FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Course not found");
            }
            return course;
        }

        public async Task UpdateCourseRating(int courseId)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Course not found");
            }

            var listRating = await _db.CourseComments.Where(c => c.CourseId == courseId && c.IsFlagged == false).Select(c => c.Rating).ToListAsync();

            if (listRating.Count == 0)
            {
                course.Rating = 0;
            }
            else
            {
                course.Rating = Math.Round(listRating.Average(),2);
            }

            _db.Update(course);
        }
        public async Task ApproveCourse(int courseid, bool choice)
        {
            var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == courseid);
            if (choice == true)
            {
                course.IsApprove = ApproveStatus.Approved;
            }
            else
            {
                course.IsApprove = ApproveStatus.Denied;
                course.Status = false;
            }
            _db.Update(course);
        }

        public async Task<int> CountAsync(Expression<Func<Course, bool>> predicate)
        {
            return await _db.Courses.CountAsync(predicate);
        }
    }
}
