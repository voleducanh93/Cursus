using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class CartItemsRepository : Repository<CartItems>, ICartItemsRepository
    {
        private readonly CursusDbContext _db;
        public CartItemsRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<bool> DeleteCartItems(CartItems cartItems)
        {
            return await DeleteAsync(cartItems) != null;
        }

        public async Task<IEnumerable<CartItems>> GetAllItems(int id)
        {
            return await GetAllAsync(filter:b=>b.CartId == id,includeProperties: "Course");
        }
        public async Task<CartItems> GetItemByID(int cartItemsId)
        {
            return await GetAsync(filter: b => b.CartItemsId == cartItemsId, includeProperties: "Course");
        }
    }
}
