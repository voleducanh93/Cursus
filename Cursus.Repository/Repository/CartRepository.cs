using Cursus.Data.Entities;
using Cursus.Data.Models;
using Cursus.RepositoryContract.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Repository.Repository
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        private readonly CursusDbContext _db;

        public CartRepository(CursusDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<bool> DeleteCart(Cart cart)
        {
            cart.IsPurchased = true;
            return await UpdateAsync(cart) != null;

        }

        public async Task UpdateIsPurchased(int cartId, bool isPurchased)
        {
            var cart = await _db.Cart.FindAsync(cartId);
            if (cart != null)
            {
                cart.IsPurchased = isPurchased;
                _db.Cart.Update(cart);
                cart.IsPurchased = true;
                await UpdateAsync(cart);
            }

        }
            

        public async Task<IEnumerable<Cart>> GetCart()
        {
           return await GetAllAsync(includeProperties: "CartItems,User");
        }

        public async Task<Cart> GetCartByID(int cartId)
        {
            return await GetAsync(filter: b => b.CartId == cartId, includeProperties: "CartItems,User");
        }
	}
}
