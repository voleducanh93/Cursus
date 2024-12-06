using Cursus.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IVoucherService
    {
        Task<VoucherDTO> CreateVoucher(VoucherDTO voucherDTO); 
        Task<VoucherDTO> UpdateVoucher(int id, VoucherDTO voucherDTO);
        Task<VoucherDTO> DeleteVoucher(int id);
        Task<VoucherDTO> GetVoucherByID(int id);
        Task<VoucherDTO> GetVoucherByCode(string code); 
        Task<bool> ReceiveVoucher( int voucherID);
        Task<bool> GiveVoucher( string ReceiverID, int voucherID);

    }
}
