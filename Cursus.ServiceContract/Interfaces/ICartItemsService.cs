using Cursus.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface ICartItemsService
    {
        Task<bool> DeleteCartItem(int id);
        Task<IEnumerable<CartItems>> GetAllCartItems(int id);
    }
}
