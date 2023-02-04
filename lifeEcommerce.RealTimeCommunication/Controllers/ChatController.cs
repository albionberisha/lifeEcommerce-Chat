using Ecommerce.Core.Enum;
using Ecommerce.Core.Extensions;
using Ecommerce.Core.Shared;
using Ecommerce.Models.DTOs.Request.Customers;
using Ecommerce.Models.DTOs.Request.Messages;
using Ecommerce.Models.Response;
using Ecommerce.RealTimeCommunication.Hubs;
using Ecommerce.Services.Interfaces;
using Ecommerce.Web.Framework.Controllers;
using Ecommerce.Web.Framework.Extensions;
using Ecommerce.Web.Framework.Security.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.RealTimeCommunication.Controllers.V1._0
{
    [ApiController]
    public class ChatController : Controller
    {
        #region Properties

        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IMessageService _messageService;

        #endregion Properties

        #region Ctor

        public ChatController(
                               ILanguageService languageService,
                               IHubContext<ChatHub> hubContext,
                               IMessageService messageService
                              ) : base(languageService)
        {
            _messageService = messageService;
            _hubContext = hubContext;
        }

        #endregion Ctor

        #region Methods

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationFilterRequest request, CancellationToken cancellationToken)
        {
            var response = await _messageService.GetAll((await CurrentCustomer()).Id, null, request, cancellationToken);
            if (response == null)
                return BadRequest();

            return Ok(response.Ok(response.Data));
        }

        [HttpGet("GetConversationClassifiedAdDetail")]
        public async Task<IActionResult> GetConversationClassifiedAdDetail(Guid conversationGuid, CancellationToken cancellationToken)
        {
            var response = await _messageService.GetConversationClassifiedAdDetail((await CurrentCustomer()).Id, conversationGuid, cancellationToken);
            if (response == null)
                return BadRequest();

            if (response.StatusCode != (int)HttpStatusCode.OK)
            {
                if (response.StatusCode == (int)HttpStatusCode.NotFound)
                    return NotFound(response.NotFound(response.Message));
                else
                    return BadRequest(response.BadRequest(response.Message));
            }

            return Ok(response.Ok(response.Data));
        }

        [HttpGet("{conversationGuid}")]
        public async Task<IActionResult> GetMessagesByConversation(Guid conversationGuid, [FromQuery] PaginationFilterRequest request, CancellationToken cancellationToken)
        {
            var response = await _messageService.GetMessagesByConversation((await CurrentCustomer()).CustomerGuid, conversationGuid, request, cancellationToken);
            if (response == null)
                return BadRequest();

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                if (response.StatusCode == (int)HttpStatusCode.NotFound)
                    return NotFound(response.NotFound(response.Message));
                else
                    return BadRequest(response.BadRequest(response.Message));
            }

            return Ok(response.Ok(response.Data));
        }

        [RestrictedRolePermission()]
        [HttpPost("CreateGroup/{classifiedAdId}")]
        public async Task<IActionResult> CreateConversation(int classifiedAdId, CancellationToken cancellationToken)
        {
            var response = new Response<ConversationDto>();

            if (classifiedAdId == 0)
            {
                return BadRequest(response.BadRequest());
            }
            var currentCustomer = await CurrentCustomer();
            response = await _messageService.CreateConversation(currentCustomer.Id, currentCustomer.CustomerGuid, classifiedAdId, cancellationToken).ConfigureAwait(true);
            if (response == null)
                return BadRequest();

            if (response.StatusCode != (int)HttpStatusCode.OK)
            {
                if (response.StatusCode == (int)HttpStatusCode.InternalServerError)
                    return BadRequest(response.InternalServerError(response.Message));
                else
                    return BadRequest(response.BadRequest());
            }
            return Ok(response.Ok(response.Data));
        }

        [RestrictedRolePermission()]
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
                var modelStateErrors = ModelState.GetSerializedErrorMessages();
                response.AddErrors(modelStateErrors);
                return BadRequest(response.BadRequest());
            }

            response = await _messageService.CreateMessage((await CurrentCustomer()).Id, request, cancellationToken);
            if (response == null)
                return BadRequest();

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

            var pagination = new PaginationFilterRequest()
            {
                PageNumber = 1,
                PageSize = 10,
                Filter = new List<int?>(),
                OrderBy = SortByValueEnum.None,
                Published = null
            };

            var getClassifiedAdForCurrentUser = await _messageService.GetAll(response.Data.FromUserId, response.Data.ConversationGuid, pagination, cancellationToken);

            getClassifiedAdForCurrentUser.Data.FirstOrDefault().ConversationGuid = response.Data.ConversationGuid;

            var getClassifiedAdForAnotherUser = await _messageService.GetAll(response.Data.ToUserId, response.Data.ConversationGuid, pagination, cancellationToken);

            getClassifiedAdForAnotherUser.Data.FirstOrDefault().ConversationGuid = response.Data.ConversationGuid;
            //getClassifiedAdForAnotherUser.Data.FirstOrDefault().MessageList = new List<MessageDto>() { new MessageDto { Value = request.Value } };

            if (response.Data.FromUserGuid != response.Data.ToUserGuid)
            {
                await _hubContext.Clients.GroupExcept(response.Data.FromUserGuid.ToString(), getConnectionId).SendAsync("ReceiveMessage", response.Data, cancellationToken);
                await _hubContext.Clients.Group(response.Data.ToUserGuid.ToString()).SendAsync("ReceiveMessage", response.Data, cancellationToken);


                /// TODO REFACTOR THIS SECTION OF SERIALIZE AND DESERIALIZE
                /// YLL
                /// GURUR
                var serializedModel = JsonExtension.Serialize(getClassifiedAdForCurrentUser.Data);
                var deserializedModel = JsonExtension.Deserialize<List<CustomerInfoDto>>(serializedModel);

                /// TODO REFACTOR THIS SECTION OF SERIALIZE AND DESERIALIZE
                /// YLL
                /// GURUR
                var serializedModelOtherUser = JsonExtension.Serialize(getClassifiedAdForAnotherUser.Data);
                var deserializedModelOtherUser = JsonExtension.Deserialize<List<CustomerInfoDto>>(serializedModelOtherUser);

                await _hubContext.Clients.GroupExcept(response.Data.FromUserGuid.ToString(), getConnectionId).SendAsync("ReceiveChat", deserializedModel, cancellationToken);
                await _hubContext.Clients.Group(response.Data.ToUserGuid.ToString()).SendAsync("ReceiveChat", deserializedModelOtherUser, cancellationToken);
            }

            return Ok(response.Ok(response.Data));
        }

        [HttpGet("ReadMessages/{conversationGuid}")]
        public async Task<IActionResult> ReadMessages(Guid conversationGuid, CancellationToken cancellationToken)
        {
            var response = await _messageService.ReadMessages((await CurrentCustomer()).Id, conversationGuid, cancellationToken);
            if (response == null)
                return BadRequest();

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                return NotFound(response.NotFound(response.Message));
            }

            await _hubContext.Clients.Group(response.Data.ToUserGuid.ToString()).SendAsync("SeenMessage", conversationGuid.ToString(), cancellationToken);

            return Ok(response.Ok(response.Data));
        }

        [RestrictedRolePermission()]
        [HttpPut("message/{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] MessageDtoModel request, CancellationToken cancellationToken)
        {
            var response = new Response<MessageDto>();
            var getConnectionId = Request.Headers["connection-id"].ToString() ?? "";

            if (request == null)
            {
                return BadRequest(response.BadRequest());
            }

            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.GetSerializedErrorMessages();
                response.AddErrors(modelStateErrors);
                return BadRequest(response.BadRequest());
            }
            var currentCustomer = await CurrentCustomer();
            response = await _messageService.UpdateAsync(currentCustomer.Id, messageId: messageId, request, cancellationToken);
            if (response == null)
                return BadRequest();

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
                else
                {
                    return BadRequest(response.BadRequest(response.Message));
                }
            }

            var pagination = new PaginationFilterRequest()
            {
                PageNumber = 1,
                PageSize = 10,
                Filter = new List<int?>(),
                OrderBy = SortByValueEnum.None,
                Published = null
            };


            var offerResponse = await _messageService.GetOfferResponse(currentCustomer.Id, response.Data.ConversationId, cancellationToken);

            var dataForClassiefiedAd = await _messageService.GetAll(currentCustomer.Id, response.Data.ConversationGuid, pagination, cancellationToken);

            dataForClassiefiedAd.Data.FirstOrDefault().ConversationGuid = response.Data.ConversationGuid;

            await _hubContext.Clients.GroupExcept(response.Data.FromUserGuid.ToString(), getConnectionId).SendAsync("EditReceiveMessage", response.Data, cancellationToken);
            await _hubContext.Clients.Group(response.Data.ToUserGuid.ToString()).SendAsync("EditReceiveMessage", response.Data, cancellationToken);

            await _hubContext.Clients.GroupExcept(offerResponse.Data.FromUserGuid.ToString(), getConnectionId).SendAsync("ReceiveMessage", response.Data, cancellationToken);
            await _hubContext.Clients.Group(offerResponse.Data.ToUserGuid.ToString()).SendAsync("ReceiveMessage", offerResponse.Data, cancellationToken);

            await _hubContext.Clients.GroupExcept(offerResponse.Data.FromUserGuid.ToString(), getConnectionId).SendAsync("ReceiveChat", dataForClassiefiedAd.Data, cancellationToken);
            await _hubContext.Clients.Group(offerResponse.Data.ToUserGuid.ToString()).SendAsync("ReceiveChat", dataForClassiefiedAd.Data, cancellationToken);

            return Ok(offerResponse.Ok(offerResponse.Data));
        }

        [HttpDelete("delete/{conversationGuid}")]
        public async Task<IActionResult> DeleteConversation(Guid conversationGuid, CancellationToken cancellationToken)
        {
            var response = await _messageService.DeleteConversation(conversationGuid, (await CurrentCustomer()).Id, cancellationToken);
            if (response == null)
                return BadRequest();

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                return NotFound(response.NotFound(response.Message));
            }

            return Ok(response.Ok(response.Data));
        }

        [HttpGet("CountUnreadConversation")]
        public async Task<IActionResult> GetCountUnreadConversation(CancellationToken cancellationToken)
        {
            var response = await _messageService.GetCountUnreadConversation((await CurrentCustomer()).Id, cancellationToken);
            if (response == null)
                return BadRequest();

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                if (response.StatusCode == (int)HttpStatusCode.NotFound)
                    return NotFound(response.NotFound(response.Message));
                else
                    return BadRequest(response.BadRequest(response.Message));
            }

            return Ok(response.Ok(response.Data));
        }

        #endregion Methods
    }
}