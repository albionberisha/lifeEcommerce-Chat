﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace lifeEcommerce.Hubs
{
    public class EcommerceHub : Hub
	{
		#region Chat

		public Task SendMessage(string user, string message)
		{
			return Clients.All.SendAsync("ReceiveMessage", user, message);
		}

		public Task SendMessageToCaller(string user, string message)
		{
			return Clients.Caller.SendAsync("ReceiveMessage", user, message);
		}

		public async Task JoinChat(string groupName)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
		}

		public Task SendMessageToGroup(string sender, string receiver, string message)
		{
			return Clients.Group(receiver).SendAsync("ReceiveMessage", sender, message);
		}

		#endregion Chat

		#region Notification

		public Task SendNotificatione(string user, string message)
		{
			return Clients.All.SendAsync("ReceiveNotification", user, message);
		}

		#endregion Notification

		#region Connect-Disconnect

		[Authorize]
		public override async Task OnConnectedAsync()
		{
            var currentUserId = "";
            await Groups.AddToGroupAsync(Context.ConnectionId, currentUserId);
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
            var currentUserId = "";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentUserId);
			await base.OnDisconnectedAsync(exception);
		}

		#endregion Connect-Disconnect
	}
}