using Cursus.Common.Helper;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageService _messageService;
        private APIResponse _response;

        public MessageController(IMessageService messageService, IUnitOfWork unitOfWork, APIResponse response)
        {
            _messageService = messageService;
            _unitOfWork = unitOfWork;
            _response = response;
        }

        /// <summary>
        /// Get all message of a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetMessages(string groupName)
        {
            var messages = await _messageService.GetMessage(groupName);
            _response.Result = messages;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
    }
}
