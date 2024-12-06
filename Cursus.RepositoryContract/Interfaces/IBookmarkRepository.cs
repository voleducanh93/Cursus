using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface IBookmarkRepository : IRepository<Bookmark>
    {

        Task<IEnumerable<Bookmark>> GetFilteredAndSortedBookmarksAsync(string userId, string? sortBy, string sortOrder);
    }
}
