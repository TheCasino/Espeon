﻿using Espeon.Attributes;
using Espeon.Commands;
using Espeon.Database;
using Espeon.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    //TODO add shit like custom summaries
    public class ModuleManager : IService
    {
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly CustomCommandsService _customCommands;
        [Inject] private readonly IServiceProvider _services;
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        public Task InitialiseAsync(DatabaseContext context, IServiceProvider services)
        {
            services.GetService<CommandService>().ModuleBuilding += OnBuildingAsync;
            return Task.CompletedTask;
        }
        
        private async Task OnBuildingAsync(ModuleBuilder moduleBuilder)
        {
            if(string.IsNullOrEmpty(moduleBuilder.Name))
                throw new NoNullAllowedException(nameof(moduleBuilder));

            using (var ctx = _services.GetService<DatabaseContext>())
            {
                var modules = ctx.Modules.Include(x => x.Commands);
                var foundModule = await modules.FirstOrDefaultAsync(x => x.Name == moduleBuilder.Name);

                var commandBuilders = moduleBuilder.Commands;

                if (foundModule is null)
                {
                    foundModule = new ModuleInfo
                    {
                        Name = moduleBuilder.Name
                    };

                    if(commandBuilders.Any(x => string.IsNullOrEmpty(x.Name)))
                        throw new NoNullAllowedException(nameof(commandBuilders));

                    foundModule.Commands = commandBuilders.Select(x => new CommandInfo
                    {
                        Name = $"{moduleBuilder.Name}{x.Name}"
                    }).ToList();

                    await ctx.Modules.AddAsync(foundModule);
                }
                else
                {
                    if(!(foundModule.Aliases is null) && foundModule.Aliases.Count > 0)
                        moduleBuilder.AddAliases(foundModule.Aliases.ToArray());

                    foreach (var commandBuilder in commandBuilders)
                    {
                        if(string.IsNullOrWhiteSpace(commandBuilder.Name))
                            throw new NoNullAllowedException(nameof(commandBuilder));

                        var commandKey = $"{moduleBuilder.Name}{commandBuilder.Name}";

                        var foundCommand = foundModule.Commands.FirstOrDefault(x => x.Name == commandKey);

                        if (foundCommand is null)
                        {
                            foundCommand = new CommandInfo
                            {
                                Name = commandKey,
                            };

                            foundModule.Commands.Add(foundCommand);
                            ctx.Modules.Update(foundModule);
                        }
                        else
                        {
                            if(!(foundCommand.Aliases is null) && foundCommand.Aliases.Count > 0)
                                commandBuilder.AddAliases(foundCommand.Aliases.ToArray());
                        }
                    }
                }

                await ctx.SaveChangesAsync();
            }
        }

        //TODO doesn't work
        public async Task<bool> AddAliasAsync(EspeonContext context, Module module, string alias)
        {
            var commands = _commands.GetAllCommands();

            if (!Utilities.AvailableName(commands, alias))
                return false;

            var foundModule = await context.Database.Modules.FindAsync(module.Name);

            if (foundModule is null)
                return false;

            foundModule.Aliases.Add(alias);
            context.Database.Modules.Update(foundModule);
            await context.Database.SaveChangesAsync();
            await UpdateAsync(module);

            return true;
        }

        public async Task<bool> AddAliasAsync(EspeonContext context, Module module, string command, string alias)
        {
            var commands = _commands.GetAllCommands();

            if (!Utilities.AvailableName(commands, alias))
                return false;

            var foundModule = await context.Database.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == module.Name);

            var foundCommand = foundModule?.Commands.SingleOrDefault(x => x.Name == $"{module.Name}{command}");

            if (foundCommand is null || foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Add(alias);

            context.Database.Commands.Update(foundCommand);
            await context.Database.SaveChangesAsync();
            await UpdateAsync(module);

            return true;
        }

        public async Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string alias)
        {
            var foundModule = await context.Database.Modules.FindAsync(module.Name);

            if (foundModule is null || !foundModule.Aliases.Contains(alias))
                return false;

            foundModule.Aliases.Remove(alias);

            context.Database.Modules.Update(foundModule);
            await context.Database.SaveChangesAsync();
            await UpdateAsync(module);

            return true;
        }

        public async Task<bool> RemoveAliasAsync(EspeonContext context, Module module, string command, string alias)
        {
            var foundModule = await context.Database.Modules.Include(x => x.Commands)
                .FirstOrDefaultAsync(x => x.Name == module.Name);

            var foundCommand = foundModule?.Commands.SingleOrDefault(x => x.Name == $"{module.Name}{command}");

            if (foundCommand is null || !foundCommand.Aliases.Contains(alias))
                return false;

            foundCommand.Aliases.Remove(alias);

            context.Database.Commands.Update(foundCommand);
            await context.Database.SaveChangesAsync();
            await UpdateAsync(module);
            return true;
        }

        private async Task UpdateAsync(Module module)
        {
            await _commands.RemoveModuleAsync(module);
            await _commands.AddModuleAsync(module.Type);
        }
    }
}
