using lifeEcommerce.RealTimeCommunication.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.RealTimeCommunication.Controllers.V1._0
{
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        //private readonly INotificationService _notificationService;

        public NotificationController(
                                       ILanguageService languageService,
                                       IHubContext<NotificationHub> hubContext,
                                       INotificationService notificationService,
                                       ILogService logService) : base(languageService)
        {
            _hubContext = hubContext;
            _notificationService = notificationService;
            _logService = logService;
        }
        /// <summary>
        /// Push notifications from worker.
        /// This method is only for Worker and has annotation Authorize attribute rather then UserAuthorize
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> PushNotification(NotificationRequestDto notification, CancellationToken cancellationToken)
        {
            try
            {
                await _hubContext.Clients.User(notification.ReceiverGuid.ToString()).SendAsync("ReceiveNotification", notification, cancellationToken);
                var data = await _notificationService.UpdateSentInfoAsync(notification.Id, cancellationToken);

                //await _logService.AddLogAsync(LogLevelConstants.Debug, $"Notification with NotificationId: {notification.NotificationId} was pushed now at: {DateTime.Now}", notification.Message);
                return Ok(data.Succeeded);
            }
            catch (Exception ex)
            {
                await _logService.AddLogAsync(LogLevelConstants.Error, shortMessage: "Error sending notification", fullMessage: $"Error sending notification in PushNotification method " +
                                                    $"Exception Message: {ex.Message} /n " + $"Inner Exception: {ex.InnerException?.Message}",
                                                    cancellationToken: cancellationToken);
                return NotFound();
            }
        }

        /// <summary>
        /// Get Notification List by Authorized User
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UserAuthorize()]
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] PaginationFilterRequest request, CancellationToken cancellationToken)
        {
            var data = await _notificationService.GetUserNotifications(request, (await CurrentCustomer()).Id, cancellationToken);
            if (data == null)
                return BadRequest();

            if (!data.Succeeded)
            {
                return NotFound();
            }

            return Ok(data.Ok(data.Data));
        }

        [UserAuthorize]
        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkNotificationsAsRead(CancellationToken cancellationToken)
        {
            var data = await _notificationService.MarkNotificationsAsRead((await CurrentCustomer()).Id, cancellationToken);
            if (data == null)
                return BadRequest();

            if (!data.Succeeded)
            {
                return BadRequest(data.Data);
            }

            return Ok(data.Ok(data.Data));
        }


        [UserAuthorize]
        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead(int notificationId, CancellationToken cancellationToken)
        {

            var data = await _notificationService.MarksAsRead(notificationId, (await CurrentCustomer()).Id, cancellationToken);
            if (data == null)
                return BadRequest();

            if (!data.Succeeded)
            {
                return BadRequest(data.Data);
            }

            return Ok(data.Ok(data.Data));
        }

    }
}