﻿using backend.Helpers.Interfaces;
using backend.Models;
using backend.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Managers.SignalR
{
    [Authorize]
    public class ChatHub : Hub, IChatHub
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAuthorizeHelper _authorize;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatHub(UserManager<AppUser> userManager, IAuthorizeHelper authorize, IHubContext<ChatHub> hubContext)
        {
            _userManager = userManager;
            _authorize = authorize;
            _hubContext = hubContext;
        }

        public HubCallerContext HubContext() => Context;

        public async Task SendMessage(Message newMessage, IEnumerable<string> usersId)
        {
            await _hubContext.Clients.Users(usersId).SendAsync("RefreshMessage", new JsonResult(new
            {
                chatId = newMessage.ChatId,
                messageId = newMessage.Id,
                text = newMessage.Text,
                date = newMessage.DateSend.ToString(DateFormat.FullShort),
            }));
        }

        public async Task AddToChat(string userId, ChatData chatData)
        {
            await _hubContext.Clients.User(userId).SendAsync("RefreshChat", new JsonResult(chatData));
        }

        public async Task RenameChatName(IEnumerable<string> usersId, Chat chat)
        {
            await _hubContext.Clients.Users(usersId).SendAsync("RefreshChatName", new JsonResult(new
            {
                chatId = chat.Id,
                chatName = chat.Name
            }));
        }

        // TODO: send OnConnected data only after open first page of this web site
        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.FindByIdAsync(Context.UserIdentifier);
            await Clients.All.SendAsync("OnConnectedAsync", $"{user.FirstName} come to chat");
            await base.OnConnectedAsync();
        }

        // TODO: after close all pages of this web site send data of OnDisconnected
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = await _userManager.FindByIdAsync(Context.UserIdentifier);
            if (!_authorize.OnAuthorization())
            {
                await Clients.All.SendAsync("OnDisconnectedAsync", $"{user.FirstName} leave from chat");
                await base.OnDisconnectedAsync(exception);
            }
        }
    }
}
