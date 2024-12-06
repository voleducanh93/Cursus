using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface ICourseCommentService
    {
        public Task<IEnumerable<CourseCommentDTO>> GetCourseCommentsAsync(int courseId);
        public Task<CourseCommentDTO> PostComment (CourseCommentCreateDTO courseComment);
        public Task<CourseCommentDTO> DeleteComment (int commentId);
        public Task<bool> IsEnrolledCourse(string userId, int courseId);
    }
}
