using lifeEcommerce.Helpers;
using lifeEcommerce.Hubs;
using lifeEcommerce.Models.Dtos.NotificationAndMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace Ecommerce.RealTimeCommunication.Controllers.V1._0
{
    [ApiController]
    public class ChatController : Controller
    {
        #region Properties

        private readonly IHubContext<ChatHub> _hubContext;
        //private readonly IMessageService _messageService;

        #endregion Properties

        #region Ctor

        public ChatController(
                               IHubContext<ChatHub> hubContext
                               //IMessageService messageService
                              ) 
        {
            //_messageService = messageService;
            _hubContext = hubContext;
        }

        #endregion Ctor

        #region Methods
        
        [HttpPost("SendMessage")]
        public async Task<IActionResult> CreateMessage([FromBody] MessageDtoModel request, CancellationToken cancellationToken)
        {
            var response = new Response<MessageDto>();
            var getConnectionId = Request.Headers["connection-id"].ToString() ?? "";

            if (request == null)
            {
                return BadRequest(response.BadRequest());
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            //response = await _messageService.CreateMessage((await userId, request, cancellationToken);
            //if (response == null)
            //    return BadRequest();

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                if (response.StatusCode == (int)HttpStatusCode.Forbidden)
                {
                    return BadRequest(response.Forbidden(response.Message));
                }
                else if (response.StatusCode == (int)HttpStatusCode.InternalServerError)
                {
                    return BadRequest(response.InternalServerError(response.Message));
                }
                else if (response.StatusCode == (int)HttpStatusCode.BadRequest)
                {
                    return BadRequest(response.BadRequest(response.Message));
                }
            }

            if (response.Data.FromUserGuid != response.Data.ToUserGuid)
            {
                await _hubContext.Clients.GroupExcept(response.Data.FromUserGuid.ToString(), getConnectionId).SendAsync("ReceiveMessage", response.Data, cancellationToken);
                await _hubContext.Clients.Group(response.Data.ToUserGuid.ToString()).SendAsync("ReceiveMessage", response.Data, cancellationToken);
            }

            return Ok(response.Ok(response.Data));
        }

        [HttpGet("ReadMessages/{conversationGuid}")]
        public async Task<IActionResult> ReadMessages(Guid conversationGuid, CancellationToken cancellationToken)
        {
            //var response = await _messageService.ReadMessages(( userId, conversationGuid, cancellationToken);

            await _hubContext.Clients.Group("idOfUserWhereToSendSeen").SendAsync("SeenMessage", conversationGuid.ToString(), cancellationToken);

            return Ok();
        }

        #endregion Methods
    }
}