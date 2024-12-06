using AutoMapper;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cursus.Data;
using Cursus.Data.DTO;
using Cursus.Repository.Repository;
using Microsoft.AspNetCore.Http;
using Cursus.Repository.Enum;

namespace Cursus.Service.Services
{
    public class ReasonService : IReasonService
    {
        private readonly IReasonRepository _reasonRepository;
        private readonly ICourseService _courseService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ReasonService(IReasonRepository reasonRepository, IMapper mapper, IUnitOfWork unitOfWork, ICourseService courseService)
        {
            _reasonRepository = reasonRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _courseService = courseService;
        }

        public async Task<Reason> CreateReason(CreateReasonDTO reasonDTO)
        {
            if (string.IsNullOrEmpty(reasonDTO.Description))
            {
                throw new BadHttpRequestException("Description is required.");
            }

            if (!int.TryParse(reasonDTO.CourseId.ToString(), out int courseId))
            {
                throw new BadHttpRequestException("CourseId must be a valid number.");
            }

            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                throw new BadHttpRequestException("Course does not exist.");
            }

            var reason = _mapper.Map<Reason>(reasonDTO);

            reason.Status = (int)ReasonStatus.Processing;

            if (reason.DateCancel == DateTime.MinValue)
            {
                reason.DateCancel = DateTime.UtcNow;
            }

            await _reasonRepository.AddAsync(reason);
            await _unitOfWork.SaveChanges();

            return reason;
        }

        public async Task<ReasonDTO> GetReasonByIdAsync(int id)
        {
            var reason = await _reasonRepository.GetByIdAsync(id);
            if (reason == null)
            {
                throw new KeyNotFoundException($"Reason with ID {id} not found.");
            }
            return _mapper.Map<ReasonDTO>(reason);
        }

        public async Task DeleteReasonAsync(int id)
        {
            var reason = await _reasonRepository.GetByIdAsync(id);
            if (reason == null)
            {
                throw new KeyNotFoundException("Reason not found.");
            }

            await _unitOfWork.ReasonRepository.DeleteAsync(reason);
            await _unitOfWork.SaveChanges();
        }

        public async Task<List<Reason>> GetByCourseIdAsync(int courseId)
        {
            if (courseId <= 0)
            {
                throw new BadHttpRequestException("Invalid CourseId.");
            }

            var reasons = await _reasonRepository.GetAllAsync(r => r.CourseId == courseId);

            if (reasons == null || !reasons.Any())
            {
                throw new BadHttpRequestException("No reasons found for this course.");
            }

            return reasons.ToList();
        }
    }
}
