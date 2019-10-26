﻿using Casino.Discord;
using Discord;
using Discord.WebSocket;
using Espeon.Core;
using Espeon.Core.Commands;
using Espeon.Core.Databases;
using Humanizer;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PermissionTarget = Espeon.Core.PermissionTarget;

namespace Espeon.Commands {
	/*
	* Kick
	* Ban
	* Warn
	* Revoke
	* View Warnings
	* Remove Reactions
	* Block
	* Blacklist
	*/

	[Name("Moderation")]
	[RequireElevation(ElevationLevel.Mod)]
	[Description("Commands for moderation of your guild")]
	public class Moderation : EspeonModuleBase {
		[Command("Kick")]
		[Name("Kick User")]
		[RequirePermissions(PermissionTarget.Bot, GuildPermission.KickMembers)]
		[Description("Kicks a user from the guild")]
		public Task KickUserAsync([RequireHierarchy] IGuildUser user, [Remainder] string reason = null) {
			return Task.WhenAll(user.KickAsync(reason), SendOkAsync(0, user.GetDisplayName()));
		}

		[Command("Ban")]
		[Name("Ban User")]
		[RequirePermissions(PermissionTarget.Bot, GuildPermission.BanMembers)]
		[Description("Bans a user from your guild")]
		public Task BanUserAsync([RequireHierarchy] IGuildUser user, [RequireRange(-1, 7)] int pruneDays = 0,
			[Remainder] string reason = null) {
			return Task.WhenAll(user.BanAsync(pruneDays, reason), SendOkAsync(0, user.GetDisplayName()));
		}

		[Command("warn")]
		[Name("Warn User")]
		[Description("Adds a warning to the specified user")]
		public async Task WarnUserAsync([RequireHierarchy] IGuildUser targetUser,
			[RequireSpecificLength(200)] [Remainder] string reason) {
			Guild currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

			int currentCount = currentGuild.Warnings.Count(x => x.TargetUser == targetUser.Id) + 1;

			if (currentCount >= currentGuild.WarningLimit) {
				await SendNotOkAsync(0, targetUser.GetDisplayName(), currentCount);
			}

			currentGuild.Warnings.Add(new Warning {
				TargetUser = targetUser.Id,
				Issuer = Context.User.Id,
				Reason = reason
			});

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1, targetUser.GetDisplayName()));
		}

		[Command("revoke")]
		[Name("Revoke Warning")]
		[Description("Revokes the warning corresponding to the specified id")]
		public async Task RevokeWarningAsync(string warningId) {
			Guild currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

			Warning warning = currentGuild.Warnings.FirstOrDefault(x => x.Id == warningId);

			if (warning is null) {
				await SendNotOkAsync(0);
				return;
			}

			currentGuild.Warnings.Remove(warning);

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
		}

		[Command("warnings")]
		[Name("View Warnings")]
		[Description("View a users warnings")]
		public async Task ViewWarningsAsync([Remainder] IGuildUser targetUser) {
			Guild currentGuild = await Context.GuildStore.GetOrCreateGuildAsync(Context.Guild, x => x.Warnings);

			Warning[] foundWarnings = currentGuild.Warnings.Where(x => x.TargetUser == targetUser.Id).ToArray();

			if (foundWarnings.Length == 0) {
				await SendOkAsync(0);
				return;
			}

			var sb = new StringBuilder();

			foreach (Warning warning in foundWarnings) {
				sb.AppendLine($"**Id**: {warning.Id}, ");

				IGuildUser issuer = Context.Guild.GetUser(warning.Issuer) as IGuildUser ??
				                    await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, warning.Issuer);

				sb.Append("**Issuer**: ").Append(issuer?.GetDisplayName() ?? "Not Found").AppendLine(", ");

				sb.Append("**Issued On**: ").AppendLine(DateTimeOffset.FromUnixTimeMilliseconds(warning.IssuedOn)
					.Humanize(culture: CultureInfo.InvariantCulture));

				sb.Append("**Reason**: ").AppendLine(warning.Reason);

				sb.AppendLine();
			}

			await SendOkAsync(1, sb.ToString());
		}

		[Command("noreactions")]
		[Name("Revoke Reactions")]
		[RequirePermissions(PermissionTarget.Bot, GuildPermission.ManageRoles)]
		[Description("Adds the no reactions role to the specified user")]
		public async Task RevokeReactionsAsync([RequireHierarchy] [Remainder] IGuildUser user) {
			Guild currentGuild = Context.CurrentGuild;

			SocketRole role = Context.Guild.GetRole(currentGuild.NoReactions);

			if (role is null) {
				await SendNotOkAsync(0);
				return;
			}

			await Task.WhenAll(
				user.AddRoleAsync(role, new RequestOptions { AuditLogReason = "Reaction rights revoked" }),
				SendOkAsync(1));
		}

		[Command("restorereactions")]
		[Name("Restore Reactions")]
		[RequirePermissions(PermissionTarget.Bot, GuildPermission.ManageRoles)]
		[Description("Removes the no reactions role from the specified user")]
		public async Task RestoreReactionsAsync([RequireHierarchy] [Remainder] IGuildUser user) {
			Guild currentGuild = Context.CurrentGuild;

			SocketRole role = Context.Guild.GetRole(currentGuild.NoReactions);

			if (role is null) {
				await SendNotOkAsync(0);
				return;
			}

			await Task.WhenAll(
				user.RemoveRoleAsync(role, new RequestOptions { AuditLogReason = "Reaction rights restored" }),
				SendOkAsync(1));
		}

		[Command("block")]
		[Name("Block User")]
		[RequirePermissions(PermissionTarget.Bot, ChannelPermission.ManageChannels)]
		[Description("Stops the specified user from talking in this channel")]
		public Task BlockUserAsync([RequireHierarchy] [Remainder] IGuildUser user) {
			return Task.WhenAll(
				Context.Channel.AddPermissionOverwriteAsync(user,
					new OverwritePermissions(sendMessages: PermValue.Deny),
					new RequestOptions { AuditLogReason = "User blocked from channel" }), SendOkAsync(0));
		}

		[Command("blacklist")]
		[Name("Blacklist User")]
		[Description("Blacklists a user from using the bot")]
		public async Task BlacklistAsync([RequireHierarchy] [Remainder] IGuildUser user) {
			Guild currentGuild = Context.CurrentGuild;

			if (currentGuild.RestrictedUsers.Contains(user.Id)) {
				await SendNotOkAsync(0);
				return;
			}

			currentGuild.RestrictedUsers.Add(user.Id);

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
		}

		[Command("unblacklist")]
		[Name("Unblacklist")]
		[Description("Removes a user from the bots blacklist")]
		public async Task UnblacklistAsync([RequireHierarchy] [Remainder] IGuildUser user) {
			Guild currentGuild = Context.CurrentGuild;

			if (!currentGuild.RestrictedUsers.Contains(user.Id)) {
				await SendNotOkAsync(0);
				return;
			}

			currentGuild.RestrictedUsers.Remove(user.Id);

			Context.GuildStore.Update(currentGuild);

			await Task.WhenAll(Context.GuildStore.SaveChangesAsync(), SendOkAsync(1));
		}
	}
}