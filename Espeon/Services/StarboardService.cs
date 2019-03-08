﻿using Discord;
using Discord.WebSocket;
using Espeon.Databases;
using Espeon.Databases.CommandStore;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class StarboardService : BaseService
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IServiceProvider _services;

        private Emoji Star => Utilities.Star;

        public override Task InitialiseAsync(UserStore userStore, GuildStore guildStore, CommandStore commandStore, IServiceProvider services)
        {
            _client.ReactionAdded += ReactionAddedAsync;
            _client.ReactionRemoved += ReactionRemovedAsync;

            return Task.CompletedTask;
        }

        private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!(channel is SocketTextChannel textChannel))
                return;

            if (!reaction.Emote.Equals(Star))
                return;

            using var guildStore = _services.GetService<GuildStore>();
            var guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

            if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is SocketTextChannel starChannel))
                return;

            var message = await msg.GetOrDownloadAsync();

            var count = message.Reactions[Star].ReactionCount;

            if (count < guild.StarLimit)
                return;

            var foundMessage = guild.StarredMessages
                .FirstOrDefault(x => x.Id == message.Id || x.StarboardMessageId == message.Id);

            var m = $"{Star} **{count}** - {(message.Author as IGuildUser).GetDisplayName()} in <#{message.Channel.Id}>";

            if (foundMessage is null)
            {
                var users = await message.GetReactionUsersAsync(Star, count).FlattenAsync();

                var embed = Utilities.BuildStarMessage(message);

                var newStar = await starChannel.SendMessageAsync(m, embed: embed);

                guild.StarredMessages.Add(new StarredMessage
                {
                    AuthorId = message.Author.Id,
                    ChannelId = message.Channel.Id,
                    Id = message.Id,
                    StarboardMessageId = newStar.Id,
                    ReactionUsers = users.Select(x => x.Id).ToList(),
                    ImageUrl = embed.Image?.Url,
                    Content = message.Content
                });

                await guildStore.SaveChangesAsync();
            }
            else
            {
                if (foundMessage.ReactionUsers.Contains(reaction.UserId))
                    return;

                foundMessage.ReactionUsers.Add(reaction.UserId);

                var fetchedMessage = await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) as IUserMessage;

                await fetchedMessage.ModifyAsync(x => x.Content = m);

                await guildStore.SaveChangesAsync();
            }
        }

        private async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!(channel is SocketTextChannel textChannel))
                return;

            if (!reaction.Emote.Equals(Star))
                return;

            using var guildStore = _services.GetService<GuildStore>();
            var guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

            if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is SocketTextChannel starChannel))
                return;

            var message = await msg.GetOrDownloadAsync();

            var foundMessage = guild.StarredMessages
                .FirstOrDefault(x => x.Id == message.Id || x.StarboardMessageId == message.Id);

            if (foundMessage is null)
                return;

            if (!foundMessage.ReactionUsers.Remove(reaction.UserId))
                return;

            var count = message.Reactions.ContainsKey(Star) ? message.Reactions[Star].ReactionCount : 0;

            var starMessage = await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) as IUserMessage;

            if (starMessage is null || count < guild.StarLimit)
            {
                _ = starMessage?.DeleteAsync();

                guild.StarredMessages.Remove(foundMessage);
            }
            else
            {
                var m = $"{Star} **{count}** - {(message.Author as IGuildUser).GetDisplayName()} in <#{message.Channel.Id}>";

                await starMessage.ModifyAsync(x => x.Content = m);
            }

            await guildStore.SaveChangesAsync();
        }
    }
}
