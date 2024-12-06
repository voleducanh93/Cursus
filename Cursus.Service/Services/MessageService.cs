using AutoMapper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Cursus.Service.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public MessageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task AddMessage(Message message)
        {
            if (string.IsNullOrEmpty(message.Text))
            {
                throw new BadHttpRequestException("Message text cannot be empty");
            }

            await _unitOfWork.MessageRepository.AddAsync(message);

            await _unitOfWork.SaveChanges();
        }

        public async Task<List<MessageDTO>> GetMessage(string groupName)
        {
            var listMessage = await _unitOfWork.MessageRepository
                .GetAllAsync(m => m.GroupName == groupName);

            listMessage = listMessage.OrderBy(m => m.TimeStamp);

            var userIds = listMessage.Select(m => m.SenderId).Distinct();

            var userDictionary = (await _unitOfWork.UserRepository
                .GetAllAsync(u => userIds.Contains(u.Id)))
                .ToDictionary(u => u.Id, u => u.UserName);

            var output = listMessage.Select(m =>
            {
                var dto = _mapper.Map<MessageDTO>(m);
                dto.Username = userDictionary[m.SenderId];
                return dto;
            }).ToList();

            return output;
        }

    }
}
