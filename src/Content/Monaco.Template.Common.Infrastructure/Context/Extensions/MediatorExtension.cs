﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Monaco.Template.Common.Domain.Model;

namespace Monaco.Template.Common.Infrastructure.Context.Extensions;

public static class MediatorExtension
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, DbContext ctx)
    {
        while (true)
        {
            var domainEntities = ctx.ChangeTracker.Entries<Entity>()
                                    .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                                    .ToList();

            var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToList();

            domainEntities.ToList()
                          .ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
                await mediator.Publish(domainEvent);

            //If event handlers produced more domain events, keep processing them until there's no more
            if (ctx.ChangeTracker.Entries<Entity>().Any(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any()))
                continue;

            break;
        }
    }
}