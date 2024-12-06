using AutoMapper;
using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Data.Enums;
using Cursus.Repository.Enum;
using Cursus.Repository.Repository;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Cursus.Service.Services
{
    public class CourseService : ICourseService
    {
       
        private readonly IUserService _userService;
        private readonly ICourseProgressService _progressService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService( ICourseProgressService progressService, IUserService userService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            
            _progressService = progressService;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public async Task<PageListResponse<CourseDTO>> GetCoursesAsync(string? searchTerm, string? sortColumn, string? sortOrder, int page = 1, int pageSize = 20)
        {

            IEnumerable<Course> coursesRepo = await _unitOfWork.CourseRepository.GetAllAsync(c => c.Status == true, includeProperties: "Category");
            var courses = coursesRepo.ToList();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                courses = courses.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||  // Tìm kiếm theo tên khóa học
                    (p.Category != null && p.Category.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))) // Tìm kiếm theo tên danh mục
                    .ToList();
            }



            if (sortOrder?.ToLower() == "desc")
            {
                courses = courses.OrderByDescending(course => GetSortProperty(sortColumn)(course)?.Length)
                                 .ThenByDescending(course => GetSortProperty(sortColumn)(course))
                                 .ToList();
            }
            else
            {
                courses = courses.OrderBy(course => GetSortProperty(sortColumn)(course)?.Length)
                                 .ThenBy(course => GetSortProperty(sortColumn)(course))
                                 .ToList();
            }



            var totalCount = courses.Count;


            var paginatedCourses = courses
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            return new PageListResponse<CourseDTO>
            {
                Items = MapCoursesToDTOs(paginatedCourses),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                HasNextPage = (page * pageSize) < totalCount,
                HasPreviousPage = page > 1
            };
        }


        private List<CourseDTO> MapCoursesToDTOs(List<Course> courses)
        {
            if (courses == null || !courses.Any())
            {
                return new List<CourseDTO>();
            }

            List<CourseDTO> courseDTOs = new List<CourseDTO>();

            foreach (var course in courses)
            {
                courseDTOs.Add(_mapper.Map<CourseDTO>(course));
            }

            return courseDTOs;
        }




        private static Func<Course, string> GetSortProperty(string SortColumn)
        {
            return SortColumn?.ToLower() switch
            {
                "name" => course => course.Name,
                "description" => course => course.Description,
                "categoryid" => course => course.CategoryId.ToString(), // Chuyển CategoryId thành chuỗi
                "datecreated" => course => course.DateCreated.ToString(), // Chuyển DateCreated thành chuỗi
                _ => course => course.Id.ToString() // Mặc định chuyển Id thành chuỗi
            };
        }



        public async Task<PageListResponse<CourseDTO>> GetRegisteredCoursesByUserIdAsync(string userId, int page = 1, int pageSize = 20)
        {

            var userExists = await _userService.CheckUserExistsAsync(userId);
            if (!userExists)
            {
                throw new Exception($"User with ID {userId} not found.");
            }


            var courseIds = await _progressService.GetRegisteredCourseIdsAsync(userId);


            if (!courseIds.Any())
            {
                return new PageListResponse<CourseDTO>
                {
                    Items = new List<CourseDTO>(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                };
            }


            var courseIdsSet = new HashSet<int>(courseIds);


            var courseList = await _unitOfWork.CourseRepository.GetAllAsync(p => p.Status);


            var filteredCourses = courseList.Where(c => courseIdsSet.Contains(c.Id));


            var totalCount = filteredCourses.Count();

            var paginatedCourses = filteredCourses
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            return new PageListResponse<CourseDTO>
            {
                Items = MapCoursesToDTOs(paginatedCourses),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                HasNextPage = (page * pageSize) < totalCount,
                HasPreviousPage = page > 1
            };

        }

        public async Task<CourseDTO> CreateCourseWithSteps(CourseCreateDTO courseCreateDTO)
        {
            var instructorID = await _unitOfWork.InstructorInfoRepository.GetAsync(x => x.Id == courseCreateDTO.InstructorInfoId);
            if (instructorID == null)
                throw new KeyNotFoundException("Instructor not found");
            // Check unique name
            bool courseExists = await _unitOfWork.CourseRepository.AnyAsync(c => c.Name == courseCreateDTO.Name);

            if (courseExists)
                throw new BadHttpRequestException("Course name must be unique.");

			var category = await _unitOfWork.CategoryRepository.GetAsync(c => c.Id == courseCreateDTO.CategoryId);
			if (category == null)
				throw new BadHttpRequestException("Category does not exist.");
			if (category.IsParent)
				throw new BadHttpRequestException("Cannot assign a parent category to a course.");

			if (courseCreateDTO.Steps == null || !courseCreateDTO.Steps.Any())
                throw new BadHttpRequestException("Steps cannot be empty.");

            var course = _mapper.Map<Course>(courseCreateDTO);
            course.IsApprove = ApproveStatus.Pending;
             
            course.InstructorInfo = await _unitOfWork.InstructorInfoRepository.GetAsync(i => i.Id == courseCreateDTO.InstructorInfoId);
            // Save course in db
            await _unitOfWork.CourseRepository.AddAsync(course);
            await _unitOfWork.SaveChanges();

            // Get back data of Course with steps in db
            var courseDB = await _unitOfWork.CourseRepository.GetAllIncludeStepsAsync(course.Id);

            // Map back to courseDTO
            var savedCourseDTO = _mapper.Map<CourseDTO>(courseDB);
            return savedCourseDTO;
        }


        public async Task<CourseDTO> UpdateCourse(CourseUpdateDTO courseUpdateDTO)
        {
            var existingCourse = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == courseUpdateDTO.Id);

            if (existingCourse == null)
                throw new KeyNotFoundException("Course not found.");

            bool UniqueName = await _unitOfWork.CourseRepository.AnyAsync(c => c.Name == courseUpdateDTO.Name);

            if (UniqueName)
                throw new BadHttpRequestException("Course name must be unique.");

            existingCourse.DateModified = DateTime.UtcNow;
            existingCourse.IsApprove = ApproveStatus.Pending;
            _mapper.Map(courseUpdateDTO, existingCourse);

            await _unitOfWork.CourseRepository.UpdateAsync(existingCourse);
            await _unitOfWork.SaveChanges();

			var courseDB = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == courseUpdateDTO.Id);

			var updatedCourseDTO = _mapper.Map<CourseDTO>(courseDB);
            return updatedCourseDTO;
        }


        public async Task<bool> DeleteCourse(int courseId)
        {
            var existingCourse = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == courseId);

            if (existingCourse == null)
                throw new KeyNotFoundException("Course not found.");



            existingCourse.Status = false;
            await _unitOfWork.SaveChanges();

            return true; 
        }

        public async Task<CourseDTO> GetCourseByIdAsync(int courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == courseId);

            if (course == null)
            {
                throw new KeyNotFoundException("Course not found");
            }

            var output = _mapper.Map<CourseDTO>(course);

            return output;
        }
        public async Task<CourseDTO> CourseApproval(int courseId, bool choice)
        {
            var course = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Course not found");
            }
            await _unitOfWork.CourseRepository.ApproveCourse(courseId, choice);
            await _unitOfWork.SaveChanges();
            var output = _mapper.Map<CourseDTO>(course);
            return output;
        }

        public async Task<APIResponse> UpdateCourseStatus(CourseUpdateStatusDTO courseUpdateStatusDTO)
        {
            var response = new APIResponse();

            var course = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == courseUpdateStatusDTO.Id);
            if (course == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages.Add("Course not found.");
                return response;
            }

            // Cập nhật trạng thái khóa học
            var previousStatus = course.Status; 
            course.Status = courseUpdateStatusDTO.Status; // Gán bool

            // Cập nhật trạng thái Reason dựa trên trạng thái mới của khóa học
            var reason = await _unitOfWork.ReasonRepository.GetByCourseIdAsync(course.Id);
            if (reason != null)
            {
                if (previousStatus == true && course.Status == false) 
                {
                    reason.Status = (int)ReasonStatus.Accepted;
                }
                else if (previousStatus == false && course.Status == true) 
                {
                    reason.Status = (int)ReasonStatus.Accepted; 
                }

                await _unitOfWork.ReasonRepository.UpdateAsync(reason);
            }

            await _unitOfWork.SaveChanges();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        public async Task<TotalEarningPotentitalDTO> CaculatePotentialEarnings(int courseId, int months)
        {
            var course = await _unitOfWork.CourseRepository.GetAsync(c => c.Id  == courseId);
            if (course == null)
                throw new KeyNotFoundException("Course Not Found");
            if (months < 1 || months > 12)
                throw new ArgumentOutOfRangeException(nameof(months), "Months must be between 1 and 12.");
            

            var totalRevenue = await _unitOfWork.TransactionRepository.
                    GetAllAsync(x => x.Description.Contains(course.Name.ToLower())).
                        ContinueWith(task => task.Result.Sum(t => t.Amount) ?? 0);

            var projectedRevenues = new List<double>();
            double initialRevenue = (double)totalRevenue;
            double monthlyGrowthRate = 0.25;

            for(int month = 1; month <= months; month++)
            {
                var projectedRevenue = initialRevenue * (double)Math.Pow(1 + (double)monthlyGrowthRate, month);
                projectedRevenues.Add(projectedRevenue);
            }
            return new TotalEarningPotentitalDTO
            {
                CourseName = course.Name,
                InitialRevenue = initialRevenue,
                ProjectedRevenues = projectedRevenues,
            };
                
        }

        }
    }

