using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookmarkService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookmarkDTO>> GetFilteredAndSortedBookmarksAsync(string userId, string? sortBy, string sortOrder)
        {
            var bookmarks = await _unitOfWork.BookmarkRepository.GetFilteredAndSortedBookmarksAsync(userId, sortBy, sortOrder);
            return _mapper.Map<IEnumerable<BookmarkDTO>>(bookmarks);
        }

        public async Task<CourseDTO> GetCourseDetailsAsync(int courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == courseId, "Steps");
            var courseDTO = _mapper.Map<CourseDTO>(course);
            return courseDTO;
        }

        public async Task CreateBookmarkAsync(string userId, int courseId)
        {
            var user = await _unitOfWork.UserRepository.ExiProfile(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var course = await _unitOfWork.CourseRepository.GetAsync(c => c.Id == courseId);
            if (course == null)
                throw new KeyNotFoundException("Course not found.");

            var courseExisted = await _unitOfWork.BookmarkRepository.GetAsync(b => b.UserId == userId && b.Course.Id == courseId);
            if (courseExisted != null)
                throw new BadHttpRequestException("You have already bookmarked this course!");

            var bookmark = new Bookmark
            {
                UserId = userId,
                Course = course
            };

            await _unitOfWork.BookmarkRepository.AddAsync(bookmark);
            await _unitOfWork.SaveChanges();
        }

    }
}
