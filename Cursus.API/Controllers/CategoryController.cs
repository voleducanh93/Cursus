using Cursus.Common.Helper;
using Cursus.Data.DTO.Category;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace Cursus.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]
    public class CategoryController : ControllerBase

    {
        private readonly ICategoryService _categoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly APIResponse _response;
        public CategoryController(ICategoryService categoryService, APIResponse response, IUnitOfWork unitOfWork)
        {
            _categoryService = categoryService;
            _response = response;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get all categories with pagination
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortOrder"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin,Instructor")]
        public async Task<ActionResult<APIResponse>> GetAllCategories([FromQuery] string? searchTerm,
        [FromQuery] string? sortColumn,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            var categories = await _categoryService.GetCategoriesAsync(searchTerm, sortColumn, sortOrder, page, pageSize);
            if (categories.Items.Any())
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = categories;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.Add("No categories found");
            return BadRequest(_response);
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin,Instructor")]
        public async Task<ActionResult<APIResponse>> GetCategory(int id)
        {

            var categoryDto = await _categoryService.GetCategoryById(id);
            if (categoryDto != null)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = categoryDto; // Set the result to the found category
                return Ok(_response); // Return the response
            }

            // If no category is found, return a not found response
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.ErrorMessages.Add($"Category with ID {id} not found.");
            return NotFound(_response);

        }

        /// <summary>
        /// Create category
        /// </summary>
        /// <param name="createCategoryDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> CreateCategory([FromBody] CreateCategoryDTO createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(_response);
            }

            var category = await _categoryService.CreateCategory(createCategoryDto);

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.Created;
            _response.Result = category;
            return CreatedAtAction(nameof(CreateCategory), new { id = category.Id }, _response);

        }

        /// <summary>
        /// Update category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateCategoryDto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> UpdateCategory(int id, [FromBody] UpdateCategoryDTO updateCategoryDto)
        {

            var updatedCategory = await _categoryService.UpdateCategory(id, updateCategoryDto);

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = updatedCategory;
            return Ok(_response);

        }

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> DeleteCategory(int id)
        {

            var deletedCategory = await _categoryService.DeleteCategory(id);

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = deletedCategory;
            return Ok(_response);

        }

    }
}
