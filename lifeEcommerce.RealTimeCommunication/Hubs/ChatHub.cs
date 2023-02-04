﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace lifeEcommerce.RealTimeCommunication.Hubs
{
    public class ChatHub : Hub
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

        #region Connect-Disconnect

        //[Authorize]
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