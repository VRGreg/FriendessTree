﻿using backend.Managers;
using backend.Models;
using backend.Repositories;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Services
{
    public class InviteService : AppDbRepository, IInviteService
    {
        private readonly UserManager<AppUser> _userManager;

        public InviteService(AppDbContext dbContext, UserManager<AppUser> userManager) : base(dbContext)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<Invite>> GetAllInvites(ClaimsPrincipal curentUser)
        {
            var user = await _userManager.GetUserAsync(curentUser);
            return dbContext.Invites.Where(_ => _.RecipientId == user.Id);
        }

        public async Task<IEnumerable<InviteTable>> GetNotDecideInvites(ClaimsPrincipal curentUser)
        {
            var invites = (await GetAllInvites(curentUser)).Where(_ => _.Decide == Decide.NotDecide);
            var senders = dbContext.Users.Where(_ => invites.Select(i => i.SenderId).Contains(_.Id));
            return invites.Select(_ => new InviteTable() { InviteId = _.Id, WhenSend = _.WhenSend.Value.ToString("g"), SenderId = _.SenderId, FirstName = senders.FirstOrDefault(u => u.Id == _.SenderId).FirstName, LastName = senders.FirstOrDefault(u => u.Id == _.SenderId).LastName });
        }

        public async Task<int> GetNotDecideInvitesCount(ClaimsPrincipal curentUser) => (await GetAllInvites(curentUser)).Where(_ => _.Decide == Decide.NotDecide).Count();

        public async Task<ActionInviteResult> InviteRequestById(ClaimsPrincipal curentUser, string userId)
        {
            var newInvite = new Invite()
            {
                SenderId = (await _userManager.GetUserAsync(curentUser)).Id,
                RecipientId = (await dbContext.Users.FirstOrDefaultAsync(_ => _.Id == userId)).Id,
                WhenSend = DateTime.Now,
                WhenDecide = null,
                Decide = Decide.NotDecide
            };

            dbContext.Invites.Add(newInvite);
            dbContext.SaveChanges();

            return new ActionInviteResult(ActionStatus.Success, "Friendship request is sent");
        }
        public async Task<ActionInviteResult> InviteRequestByEmail(ClaimsPrincipal curentUser, string friendEmail)
        {
            var newInvite = new Invite()
            {
                SenderId = (await _userManager.GetUserAsync(curentUser)).Id,
                RecipientId = (await dbContext.Users.FirstOrDefaultAsync(_ => _.Email == friendEmail)).Id, 
                WhenSend = DateTime.Now,
                WhenDecide = null,
                Decide = Decide.NotDecide
            };

            dbContext.Invites.Add(newInvite);
            dbContext.SaveChanges();

            return new ActionInviteResult(ActionStatus.Success, "Friendship request is sent");
        }

        public async Task<ActionInviteResult> ConfirmInvite(ClaimsPrincipal curentUser, string inviteId, Decide decide)
        {
            var invite = await dbContext.Invites.FirstOrDefaultAsync(_ => _.Id == new Guid(inviteId));
            invite.Decide = decide;
            invite.WhenDecide = DateTime.Now;

            dbContext.Entry(invite).State = EntityState.Modified;

            if (decide == Decide.Accept)
            {
                var senderFriendship = new Friendship() { AppUserId = invite.SenderId, FriendId = invite.RecipientId };
                var RecipientFriendship = new Friendship() { AppUserId = invite.RecipientId, FriendId = invite.SenderId };
                dbContext.Friendships.Add(senderFriendship);
                dbContext.Friendships.Add(RecipientFriendship);
            }

            dbContext.SaveChanges();

            return new ActionInviteResult(ActionStatus.Success, decide == Decide.Accept ? "Friendship request is accept" : "Friendship request is denie");
        }
    }
}