using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class BookmarkRepository : Repository<Bookmark>, IBookmarkRepository
    {
        private readonly CursusDbContext _db;
        public BookmarkRepository(CursusDbContext db) : base(db) => _db = db;

        // Method to get bookmarks with filtering and sorting functionality
        public async Task<IEnumerable<Bookmark>> GetFilteredAndSortedBookmarksAsync(string userId, string? sortBy, string sortOrder)
        {
            IQueryable<Bookmark> query = dbSet
                .Include(b => b.Course) 
                .Where(b => b.UserId == userId); 

            switch (sortBy?.ToLower())
            {
                case "coursename":
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(b => b.Course.Name)
                        : query.OrderBy(b => b.Course.Name);
                    break;
                case "price":
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(b => b.Course.Price)
                        : query.OrderBy(b => b.Course.Price);
                    break;
                case "rating":
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(b => b.Course.Rating)
                        : query.OrderBy(b => b.Course.Rating);
                    break;
                default:
                    query = query.OrderBy(b => b.Id); 
                    break;
            }

            var bookmarkList = await query.ToListAsync();

            return bookmarkList;
        }
    }
}
