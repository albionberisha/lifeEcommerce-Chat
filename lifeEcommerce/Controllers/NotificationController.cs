using lifeEcommerce.Hubs;
using lifeEcommerce.Models.Dtos.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Ecommerce.RealTimeCommunication.Controllers.V1._0
{
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        //private readonly INotificationService _notificationService;
        //private readonly ILogService _logService;

        public NotificationController(
                                       IHubContext<NotificationHub> hubContext
                                       //INotificationService notificationService,
                                       )
        {
            _hubContext = hubContext;
            //_notificationService = notificationService;
        }
        /// <summary>
        /// Push notifications from worker.
        /// This method is only for Worker and has annotation Authorize attribute rather then UserAuthorize
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpPost("PushNotification")]
        public async Task<IActionResult> PushNotification(NotificationRequestDto notification, CancellationToken cancellationToken)
        {
            try
            {
                await _hubContext.Clients.User(notification.ReceiverGuid.ToString()).SendAsync("ReceiveNotification", notification, cancellationToken);
                //var data = await _notificationService.UpdateSentInfoAsync(notification.Id, cancellationToken);

                return Ok(); // with any model
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }
    }
}