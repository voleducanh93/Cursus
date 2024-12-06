using Cursus.Data.DTO;
using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
	public interface ICartService
	{
        Task<Cart> GetCartByID(int cartId);
        Task<IEnumerable<Cart>> GetAllCart();
        Task<bool> DeleteCart(int id);
        public Task AddCourseToCartAsync(int courseId, string userId);

        public Task<CartDTO> GetCartByUserIdAsync(string userId);
    }
}
