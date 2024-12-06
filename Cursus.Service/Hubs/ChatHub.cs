using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Cursus.Service.Hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, int> UserNumbers = new();
        private static int NextUserNumber = 1;
        private readonly IMessageService _messageService;
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IMessageService messageService, IUnitOfWork unitOfWork)
        {
            _messageService = messageService;
            _unitOfWork = unitOfWork;
        }

        public override async Task OnConnectedAsync()
        {
            // Assign a user number if this is a new connection
            if (!UserNumbers.ContainsKey(Context.ConnectionId))
            {
                UserNumbers[Context.ConnectionId] = NextUserNumber++;
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessageToGroup(string groupName, string message, string userId)
        {
            Message msg = new Message()
            {
                Text = message,
                SenderId = userId,
                GroupName = groupName,
                TimeStamp = DateTime.Now
            };
            await _messageService.AddMessage(msg);
            var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id == userId);
            var userName = user.UserName.Split("@");
            await Clients.Group(groupName).SendAsync("ReceiveMessage", $"{userId}", $"@{userName[0]}: {message}");
        }

        public async Task JoinGroup(string groupName, string username)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (UserNumbers.TryGetValue(Context.ConnectionId, out int userNumber))
            {
                await Clients.Group(groupName).SendAsync("SystemMessage", $"{username.Split("@")[0]} has joined the group.");
            }
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            if (UserNumbers.TryGetValue(Context.ConnectionId, out int userNumber))
            {
                await Clients.Group(groupName).SendAsync("SystemMessage", $"User {userNumber} has left the group.");
            }
        }
    }
}
