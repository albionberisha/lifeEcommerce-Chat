using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace lifeEcommerce.RealTimeCommunication.Hubs
{
    public class NotificationHub : Hub
	{
		#region Notification

		public Task SendNotification(string user, string message)
		{
			return Clients.User(user).SendAsync("ReceiveNotification", user, message);
		}

		#endregion Notification

		#region Connect-Disconnect

		[UserAuthorize]
		public override async Task OnConnectedAsync()
		{
			var currentCustomer = await CustomerHelper.CurrentCustomer();

			await Groups.AddToGroupAsync(Context.ConnectionId, currentCustomer.CustomerGuid.ToString());
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			var currentCustomer = await CustomerHelper.CurrentCustomer();

			await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentCustomer.CustomerGuid.ToString());
			await base.OnDisconnectedAsync(exception);
		}

		#endregion Connect-Disconnect
	}
}