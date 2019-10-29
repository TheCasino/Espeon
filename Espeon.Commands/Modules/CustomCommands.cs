﻿using Disqord;
using Espeon.Core;
using Espeon.Core.Database;
using Espeon.Core.Services;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Espeon.Commands {
	[Name("Custom Commands")]
	[Group("cmd")]
	[RequireElevation(ElevationLevel.Mod)]
	[Description("Add some custom commands to your guild")]
	public class CustomCommands : EspeonModuleBase {
		public ICustomCommandsService Commands { get; set; }

		[Command("create")]
		[Name("Create Command")]
		[RunMode(RunMode.Parallel)]
		[Description("Creates a new command")]
		public async Task CreateCustomCommandAsync(string name = "", [Remainder] string value = "") {
			if (name == "") {
				await SendOkAsync(0);

				CachedUserMessage reply = await NextMessageAsync(new MultiCriteria<CachedUserMessage>(
					new UserCriteria(Member.Id.RawValue), new ChannelCriteria(Channel.Id.RawValue)));

				if (string.Equals(reply.Content, "cancel", StringComparison.InvariantCultureIgnoreCase)) {
					return;
				}

				name = reply.Content;
			}

			if (value == "") {
				await SendOkAsync(1);

				CachedUserMessage reply = await NextMessageAsync(
					new MultiCriteria<CachedUserMessage>(new UserCriteria(Member.Id), new ChannelCriteria(Channel.Id)));

				if (string.Equals(reply.Content, "cancel", StringComparison.InvariantCultureIgnoreCase)) {
					return;
				}

				value = reply.Content;
			}

			bool result = await Commands.TryCreateCommandAsync(Context.GuildStore, Guild, name, value);

			if (result) {
				await SendOkAsync(2, name);
				return;
			}

			await SendNotOkAsync(3);
		}

		[Command("delete")]
		[Name("Delete Command")]
		[RunMode(RunMode.Parallel)]
		[Description("Deletes a custom command")]
		public Task DeleteCustomCommandAsync([Remainder] CustomCommand command) {
			return Task.WhenAll(Commands.DeleteCommandAsync(Context.GuildStore, Guild, command), SendOkAsync(0, command.Name));
		}

		[Command("modify")]
		[Name("Modify Command")]
		[RunMode(RunMode.Parallel)]
		[Description("Modify a response of a custom command")]
		public async Task ModifyCommandAsync(CustomCommand command, [Remainder] string newValue = "") {
			if (newValue == "") {
				await SendOkAsync(0);

				CachedUserMessage reply = await NextMessageAsync(
					new MultiCriteria<CachedUserMessage>(new UserCriteria(Member.Id), new ChannelCriteria(Channel.Id)));

				if (string.Equals(reply.Content, "cancel", StringComparison.InvariantCultureIgnoreCase)) {
					return;
				}

				newValue = reply.Content;
			}

			await Task.WhenAll(Commands.ModifyCommandAsync(Context.GuildStore, command, newValue), SendOkAsync(1, command.Name));
		}
	}
}