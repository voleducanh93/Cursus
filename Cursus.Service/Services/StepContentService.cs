using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class StepContentService : IStepContentService
    {
        private readonly IStepContentRepository _stepContentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StepContentService(IStepContentRepository stepContentRepo, IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _stepContentRepository = stepContentRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<StepContentDTO> CreateStepContent(StepContentDTO stepContentDTO)
        {
            if (string.IsNullOrEmpty(stepContentDTO.ContentType))
                throw new ArgumentException("ContentType is required.");

            var existingStepContent = await _unitOfWork.StepContentRepository
                                                       .FirstOrDefaultAsync(sc => sc.StepId == stepContentDTO.StepId);

            if (existingStepContent != null)
            {
                throw new InvalidOperationException("StepContent already exists for this Step.");
            }

            // log debug
            Console.WriteLine("Creating new StepContent for StepId: " + stepContentDTO.StepId);

            var stepContentEntity = _mapper.Map<StepContent>(stepContentDTO);

            if (stepContentEntity.DateCreated == DateTime.MinValue)
            {
                stepContentEntity.DateCreated = DateTime.UtcNow;
            }

            await _unitOfWork.StepContentRepository.AddAsync(stepContentEntity);
            await _unitOfWork.SaveChanges();

            var createdStepContentDTO = _mapper.Map<StepContentDTO>(stepContentEntity);
            return createdStepContentDTO;
        }


        public async Task<StepContentDTO> GetStepContentByIdAsync(int id)
        {
            var stepContent = await _unitOfWork.StepContentRepository.GetByIdAsync(id);

            if (stepContent == null)
            {
                throw new KeyNotFoundException("Step Content not found.");
            }

            var stepContentDTO = _mapper.Map<StepContentDTO>(stepContent);

            return stepContentDTO;
        }

        public async Task<StepContentDTO> CreateStepContentWithFileAsync(StepContentDTO stepContentDTO, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded....");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))

            {
                await file.CopyToAsync(fileStream);

            }

            stepContentDTO.ContentURL = "/uploads/" + uniqueFileName;

            var stepContent = _mapper.Map<StepContent>(stepContentDTO);
            await _stepContentRepository.AddAsync(stepContent);
            await _unitOfWork.SaveChanges();

            return _mapper.Map<StepContentDTO>(stepContent);
        }

    }
}
