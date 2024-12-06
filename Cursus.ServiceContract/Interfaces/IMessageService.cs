using Cursus.Data.DTO;
using Cursus.Data.Entities;

namespace Cursus.ServiceContract.Interfaces
{
    public interface IMessageService
    {
        Task<List<MessageDTO>> GetMessage(string groupName);
        Task AddMessage(Message message);
    }
}
