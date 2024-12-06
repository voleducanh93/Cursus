using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        public Task<bool> DeleteCart(Cart cart);
        public Task<IEnumerable<Cart>> GetCart();
        public Task<Cart> GetCartByID(int cartId);
        Task UpdateIsPurchased(int cartId, bool isPurchased);
	}
}
