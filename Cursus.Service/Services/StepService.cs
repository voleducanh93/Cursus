using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
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
    public class StepService : IStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStepRepository _stepRepository;
        private readonly ITrackingProgressRepository _trackingProgressRepository;

        public StepService(IStepRepository stepRepository, IMapper mapper, IUnitOfWork unitOfWork, ITrackingProgressRepository trackingProgressRepository)
        {
            _stepRepository = stepRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _trackingProgressRepository = trackingProgressRepository;
        }

        public async Task<StepDTO> CreateStep(StepCreateDTO dto)
        {
            //Validate....

            var stepEntity = _mapper.Map<Step>(dto); 
            if (stepEntity.DateCreated == DateTime.MinValue)
            {
                stepEntity.DateCreated = DateTime.UtcNow;
            }

            await _stepRepository.AddAsync(stepEntity);
            await _unitOfWork.SaveChanges();

            var createdStepDTO = _mapper.Map<StepDTO>(stepEntity);

            return createdStepDTO;
        }

        public async Task<StepDTO> GetStepByIdAsync(int id)
        {
            var step = await _unitOfWork.StepRepository.GetByIdAsync(id);

            if (step == null)
            {
                throw new KeyNotFoundException("Step not found.");
            }

            var stepDTO = _mapper.Map<StepDTO>(step);

            return stepDTO;
        }

        public async Task<bool> DeleteStep(int stepId)
        {
            var existingStep = await _unitOfWork.StepRepository.GetAsync(s => s.Id == stepId);
            if (existingStep == null)
                throw new KeyNotFoundException("Step not found.");

            await _unitOfWork.StepRepository.DeleteAsync(existingStep);
            await _unitOfWork.SaveChanges();
            return true;
        }

        public async Task<IEnumerable<StepDTO>> GetStepsByCoursId(int courseId)
        {
            var steps = await _unitOfWork.StepRepository.GetStepsByCoursId(courseId);

            if (steps == null || !steps.Any())
            {
                throw new KeyNotFoundException("No steps found for the specified course.");
            }

            var stepsDTO = _mapper.Map<IEnumerable<StepDTO>>(steps);

            return stepsDTO;
        }

        public async Task<StepDTO> UpdateStep(StepUpdateDTO updateStepDTO)
        {
            var step = await _unitOfWork.StepRepository.GetByIdAsync(updateStepDTO.Id);

            if (step == null)
            {
                throw new KeyNotFoundException("Step not found.");
            }

            _mapper.Map(updateStepDTO, step);

            await _unitOfWork.StepRepository.UpdateAsync(step);
            await _unitOfWork.SaveChanges();

            return _mapper.Map<StepDTO>(step);
        }

        public async Task<TrackingProgressDTO> StartStepAsync(string userId, int stepId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));
            }

            // Lấy CourseId từ step
            var courseId = (await _unitOfWork.StepRepository.GetAsync(cp => cp.Id == stepId)).CourseId;
            if(courseId == null)
            {
                throw new InvalidOperationException("CourseId not found for the specified StepId.");
            }

            // Kiểm tra xem CourseProgress có tồn tại hay không
            var courseProgress = await _unitOfWork.CourseProgressRepository.GetAsync(cp => cp.UserId == userId && courseId == cp.CourseId);
            if (courseProgress == null)
            {
                throw new InvalidOperationException("CourseProgress not found for the specified UserId.");
            }

            // Kiểm tra xem TrackingProgress đã tồn tại chưa
            var existingTrackingProgress = await _trackingProgressRepository.GetAsync(tp => tp.UserId == userId && tp.StepId == stepId);
            if (existingTrackingProgress != null)
            {
                throw new InvalidOperationException("TrackingProgress already exists for this UserId and StepId.");
            }

            var trackingProgress = new TrackingProgress
            {
                UserId = userId,
                StepId = stepId,
                ProgressId = courseProgress.ProgressId,
                Date = DateTime.Now
            };

            await _trackingProgressRepository.AddAsync(trackingProgress);
            await _unitOfWork.SaveChanges();

            return _mapper.Map<TrackingProgressDTO>(trackingProgress);
        }




        public async Task<double> GetPercentageTrackingProgress(string userId, int courseProgressId)
        {
            var courseProgress = await _unitOfWork.CourseProgressRepository.GetAsync(cp => cp.ProgressId == courseProgressId && cp.UserId == userId);
            if (courseProgress == null)
            {
                throw new InvalidOperationException("CourseProgress not found for the specified CourseProgressId.");
            }

            var totalSteps = await _stepRepository.GetToTalSteps(courseProgress.CourseId);
            var completedSteps = await _trackingProgressRepository.GetCompletedStepsCountByUserId(userId, courseProgressId);

            double completionPercentage = totalSteps > 0 ? (completedSteps / (double)totalSteps) * 100 : 0;

            if (completionPercentage == 100)
            {
                courseProgress.IsCompleted = true;
                await _unitOfWork.CourseProgressRepository.UpdateAsync(courseProgress);
                await _unitOfWork.SaveChanges();
            }

            return completionPercentage;
        }


    }
}
