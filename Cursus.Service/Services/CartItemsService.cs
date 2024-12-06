using AutoMapper;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Service.Services
{
    public class CartItemsService : ICartItemsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartItemsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> DeleteCartItem(int id)
        {
            var OldCartItem= await _unitOfWork.CartItemsRepository.GetItemByID(id);
            if (OldCartItem == null)
            {
                return false;
            }
            await _unitOfWork.CartItemsRepository.DeleteCartItems(OldCartItem);
            await _unitOfWork.SaveChanges();
            return true;
        }

        public async Task<IEnumerable<CartItems>> GetAllCartItems(int id)
        {
            return await _unitOfWork.CartItemsRepository.GetAllItems(id);
        }
    }
}
