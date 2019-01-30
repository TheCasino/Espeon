﻿using Espeon.Entities;
using System.Collections.Generic;

namespace Espeon.Database.Entities
{
    public class User : DatabaseEntity
    {
        public override ulong Id { get; set; }
        
        public string ResponsePack { get; set; }

        public List<Reminder> Reminders { get; set; }

        public int CandyAmount { get; set; }
        public int HighestCandies { get; set; }
        public long LastClaimedCandies { get; set; }
    }

    public class Reminder : IRemovable
    {
        public string Id { get; set; }

        public string TheReminder { get; set; }
        public string JumpUrl { get; set; }
        
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public int ReminderId { get; set; }

        public string TaskKey { get; set; }
        public long WhenToRemove { get; set; }
    }
}
