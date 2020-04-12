﻿using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Espeon {
    public class LocalisationService : IInitialisableService {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Localisation, ConcurrentDictionary<LocalisationKey, string>> _responses;

        private readonly ConcurrentDictionary<(ulong, ulong), Localisation> _userLocalisationCache;

        public LocalisationService(IServiceProvider services) {
            this._services = services;
            this._logger = services.GetService<ILogger>().ForContext("SourceContext", typeof(LocalisationService).Name);
            this._responses = new ConcurrentDictionary<Localisation, ConcurrentDictionary<LocalisationKey, string>>();
            this._userLocalisationCache = new ConcurrentDictionary<(ulong, ulong), Localisation>();
        }

        public async Task InitialiseAsync() {
            var config = this._services.GetService<Config>();
            var sw = Stopwatch.StartNew();
            this._logger.Information("Loading all localisation strings");
            
            if (config.Localisation?.Path is null) {
                throw new InvalidOperationException("Localisation config must be defined");
            }
            
            var fullPathFiles = Directory.GetFiles(config.Localisation.Path);
            foreach (var fullPath in fullPathFiles) {
                var fileName = Path.GetFileName(fullPath);
                if (!Enum.TryParse<Localisation>(fileName, true, out var localisation)) {
                    throw new InvalidOperationException($"{fileName} was not recognised as a valid localisation");
                }
                
                if (config.Localisation.ExcludedFiles?.Contains(fileName) == true) {
                    continue;
                }
                
                var responsesForFile = new ConcurrentDictionary<LocalisationKey, string>();
                var lines = await File.ReadAllLinesAsync(fullPath);
                foreach (var line in lines) {
                    var split = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2) {
                        throw new InvalidOperationException("Localisation string must have format \"key=value\"");
                    }

                    if (!Enum.TryParse<LocalisationKey>(split[0], out var key)) {
                        throw new InvalidOperationException($"{split[0]} was not recognised as a valid localisation key");
                    }
                    responsesForFile[key] = split[1];
                }

                this._responses[localisation] = responsesForFile;
            }
            sw.Stop();
            this._logger.Information("All localisation strings loaded in {Time}ms", sw.ElapsedMilliseconds);
        }
        
        public ValueTask<string> GetResponseAsync(IGuild guild, IUser user, LocalisationKey key) {
            this._logger.Debug("Getting response string {Key} for {User}", key, user.Id);
            return this._userLocalisationCache.TryGetValue((guild.Id, user.Id), out var localisation)
                ? new ValueTask<string>(GetResponse(localisation, key))
                : new ValueTask<string>(GetUserLocalisationFromDbAsync(guild, user, key));
        }
        
        private async Task<string> GetUserLocalisationFromDbAsync(IGuild guild, IUser user, LocalisationKey key) {
            this._logger.Debug("Getting response string {Key} for {User} from database", key, user.Id);
            await using var context = this._services.GetService<EspeonDbContext>();
            var localisation = await context.GetLocalisationAsync(guild, user);
            this._userLocalisationCache[(guild.Id, user.Id)] = localisation.Value;
            return GetResponse(localisation.Value, key);
        }
        
        private string GetResponse(Localisation localisation, LocalisationKey key) {
            return this._responses[localisation].GetValueOrDefault(key, this._responses[Localisation.Default][key]);
        }
    }
}