﻿using Discord.WebSocket;
using Espeon.Core.Commands;
using Espeon.Core.Services;

namespace Espeon.Commands {
	public class DefaultPaginator : PaginatorBase {
		public override EspeonContext Context { get; }
		public override IInteractiveService Interactive { get; }
		public override IMessageService MessageService { get; }
		public override PaginatorOptions Options { get; }
		public override ICriterion<SocketReaction> Criterion { get; }

		public DefaultPaginator(EspeonContext context, IInteractiveService interactive, IMessageService messageService,
			PaginatorOptions options, ICriterion<SocketReaction> criterion) {
			Context = context;
			Interactive = interactive;
			MessageService = messageService;
			Options = options;
			Criterion = criterion;
		}
	}
}