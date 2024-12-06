using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class InstructorCertificateService : IInstructorCertificateService
    {
        private readonly IAzureBlobStorageService _blobStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInstructorCertificateRepository _instructorCertificateRepository;
        private readonly IMapper _mapper;

        public InstructorCertificateService(
            IAzureBlobStorageService blobStorageService, // Sử dụng AzureBlobStorageService
            IUnitOfWork unitOfWork,
            IInstructorCertificateRepository instructorCertificateRepository,
            IMapper mapper)
        {
            _blobStorageService = blobStorageService;
            _unitOfWork = unitOfWork;
            _instructorCertificateRepository = instructorCertificateRepository;
            _mapper = mapper;
        }

            public async Task<List<string>> UploadCertificatesAsync(Guid userId, List<IFormFile> files, string certificateType)
            {
                // Lấy InstructorId từ UserId
                var instructorId = await _unitOfWork.InstructorCertificateRepository.GetInstructorIdByUserIdAsync(userId);

                if (instructorId == null)
                    throw new Exception("Instructor not found for this user.");

                if (files == null || !files.Any())
                    throw new Exception("Please upload at least one file.");

                // Upload files lên Azure Blob Storage
                var uploadedFileUrls = new List<string>();
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileUrl = await _blobStorageService.UploadFileAsync(file);
                        uploadedFileUrls.Add(fileUrl);
                    }
                }

                // Tạo DTO từ dữ liệu
                var instructorCertificateDto = new InstructorCertificateDto
                {
                    InstructorId = instructorId.Value,
                    CertificateType = certificateType,
                    FileUrls = uploadedFileUrls
                };

                var instructorCertificate = _mapper.Map<InstructorCertificate>(instructorCertificateDto);

                await _instructorCertificateRepository.AddAsync(instructorCertificate);
                await _unitOfWork.SaveChanges();

                return uploadedFileUrls;
            }
        }

    }
