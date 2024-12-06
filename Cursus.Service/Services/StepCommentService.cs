using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class StepCommentService : IStepCommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public StepCommentService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // Check if the user is enrolled in a course
        public async Task<bool> IsEnrolledCourse(string userId, int courseId)
        {
            return await _unitOfWork.ProgressRepository.GetAsync(u => u.UserId == userId && u.CourseId == courseId) != null;
        }

        // Post a new step comment
        public async Task<StepCommentDTO> PostStepComment(StepCommentCreateDTO stepComment)
        {
            var user = await _userManager.FindByIdAsync(stepComment.UserId);
            if (user == null) throw new UnauthorizedAccessException("User not found");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new UnauthorizedAccessException("Your email is not confirmed. Please verify your email before commenting.");
            }

            // Check if the user has taken the course
            if (!await IsEnrolledCourse(stepComment.UserId, stepComment.CourseId))
            {
                throw new UnauthorizedAccessException("You are not enrolled in this course, so you cannot comment on this step.");
            }

            // Map StepCommentCreateDTO to StepComment
            var comment = _mapper.Map<StepComment>(stepComment);
            comment.User = user;
            comment.DateCreated = DateTime.UtcNow; 

            // Add comments to the repository
            await _unitOfWork.StepCommentRepository.AddAsync(comment);
            await _unitOfWork.SaveChanges();

            // Find step name based on StepId
            var step = await _unitOfWork.StepRepository.GetByIdAsync(stepComment.StepId);

            // Create DTO to return with stepName
            var commentDto = _mapper.Map<StepCommentDTO>(comment);
            commentDto.StepName = step?.Name; 

            return commentDto;
        }


        // Get all comments for a step
        public async Task<IEnumerable<StepCommentDTO>> GetStepCommentsAsync(int stepId)
        {
            var comments = await _unitOfWork.StepCommentRepository.GetAllAsync(c => c.StepId == stepId, includeProperties: "User,Step");

            return _mapper.Map<IEnumerable<StepCommentDTO>>(comments);
        }

        // Delete a step comment
        public async Task<bool> DeleteStepComment(int commentId)
        {
            var comment = await _unitOfWork.StepCommentRepository.GetAsync(c => c.Id == commentId);
            if (comment == null) return false;

            await _unitOfWork.StepCommentRepository.DeleteAsync(comment);
            await _unitOfWork.SaveChanges();

            return true;
        }

        // Admin-only delete operation for step comment
        public async Task<bool> DeleteStepCommentIfAdmin(int commentId, string adminId)
        {
            if (string.IsNullOrEmpty(adminId))
                throw new UnauthorizedAccessException("You do not have permission to delete comments.");

            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null || !await _userManager.IsInRoleAsync(admin, "Admin"))
                throw new UnauthorizedAccessException("You do not have permission to delete comments.");

            var comment = await _unitOfWork.StepCommentRepository.GetAsync(c => c.Id == commentId);
            if (comment == null) return false;

            await _unitOfWork.StepCommentRepository.DeleteAsync(comment);
            await _unitOfWork.SaveChanges();

            return true;
        }
    }
}
