using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class CourseCommentService : ICourseCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public CourseCommentService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<CourseCommentDTO> DeleteComment(int commentId)
        {
            var courseComment = await _unitOfWork.CourseCommentRepository.GetAsync(c => c.Id == commentId, includeProperties: "User,Course");

            if (courseComment == null)
            {
                throw new KeyNotFoundException("Comment not found");
            }

            courseComment.IsFlagged = true;

            await _unitOfWork.CourseCommentRepository.UpdateAsync(courseComment);

            await _unitOfWork.SaveChanges();

            return _mapper.Map<CourseCommentDTO>(courseComment);
        }

        public async Task<IEnumerable<CourseCommentDTO>> GetCourseCommentsAsync(int courseId)
        {
            var comments = await _unitOfWork.CourseCommentRepository.GetAllAsync(c => c.CourseId == courseId && c.IsFlagged == false, includeProperties: "User,Course");
             
            return _mapper.Map<IEnumerable<CourseCommentDTO>>(comments);
        }

        public async Task<bool> IsEnrolledCourse(string userId, int courseId)
        {
            bool isEnrolled = await _unitOfWork.ProgressRepository.GetAsync(u => u.UserId == userId && u.CourseId == courseId) != null;
            return isEnrolled;
        }

        public async Task<CourseCommentDTO> PostComment(CourseCommentCreateDTO courseComment)
        {
            var user = await _userManager.FindByIdAsync(courseComment.UserId);

            if (!await IsEnrolledCourse(courseComment.UserId, courseComment.CourseId))
            {
                throw new UnauthorizedAccessException("You must enrolled this course to giving feedback");
            }
            
            if (_userManager.IsEmailConfirmedAsync(user).Result == false)
            {
                throw new UnauthorizedAccessException("Your email is not confirmed");
            }

            var comment = _mapper.Map<CourseComment>(courseComment);

            comment.Course = await _unitOfWork.CourseRepository.GetAsync(u => u.Id == courseComment.CourseId);           

            comment.User = user;

            await _unitOfWork.CourseCommentRepository.AddAsync(comment);

            await _unitOfWork.SaveChanges();

            var commentForReturn = _mapper.Map<CourseCommentDTO>(comment);

            await _unitOfWork.CourseRepository.UpdateCourseRating(courseComment.CourseId);

            await _unitOfWork.SaveChanges();          

            return commentForReturn;
        }

    }
}
