using Cursus.Data.DTO;
using Cursus.Data.DTO.Category;
using Cursus.Data.Entities;
using System.Linq.Expressions;

namespace Cursus.ServiceContract.Interfaces
{
    public interface ICategoryService
    {
        Task<PageListResponse<CategoryDTO>> GetCategoriesAsync(string? searchTerm,
        string? sortColumn,
        string? sortOrder,
        int page = 1,
        int pageSize = 20);
        Task<CategoryDTO> GetCategoryById(int id);

        Task<CategoryDTO> UpdateCategory(int id, UpdateCategoryDTO dto);
        Task<CategoryDTO> CreateCategory(CreateCategoryDTO dto);
        Task<bool> DeleteCategory(int id);
    }
}
