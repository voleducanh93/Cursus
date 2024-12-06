using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
	public interface ICourseRepository : IRepository<Course>
    {
		Task<bool> AnyAsync(Expression<Func<Course, bool>> predicate); 
		Task<Course> GetAllIncludeStepsAsync(int courseId);
		Task UpdateCourseRating(int courseId);
		Task ApproveCourse(int courseid, bool choice);

		Task<int> CountAsync(Expression<Func<Course, bool>> predicate);
    }
}
