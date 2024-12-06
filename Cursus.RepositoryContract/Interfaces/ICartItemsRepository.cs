using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.RepositoryContract.Interfaces
{
    public interface ICartItemsRepository
    {
        public Task<bool> DeleteCartItems(CartItems cartItems);
        public Task<IEnumerable<CartItems>> GetAllItems(int id);
        public Task<CartItems> GetItemByID(int cartItemsId);
    }
}
