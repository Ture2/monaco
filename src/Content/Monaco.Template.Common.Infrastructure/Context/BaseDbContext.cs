﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Monaco.Template.Common.Domain.Model;
using Monaco.Template.Common.Domain.Model.Contracts;
using Monaco.Template.Common.Infrastructure.Context.Extensions;
using Monaco.Template.Common.Infrastructure.Context.AuditTrail;
using Monaco.Template.Common.Infrastructure.Context.Contracts;
using Monaco.Template.Common.Infrastructure.EntityConfigurations;

namespace Monaco.Template.Common.Infrastructure.Context;

public abstract class BaseDbContext : DbContext, IUnitOfWork
{
    protected readonly IMediator Mediator;
    protected readonly IHostEnvironment Env;

    protected BaseDbContext()
    {
    }

    protected BaseDbContext(DbContextOptions options, IMediator mediator, IHostEnvironment env) : base(options)
    {
        Mediator = mediator;
        Env = env;
    }

    protected abstract Assembly GetConfigurationsAssembly();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();

		//This will apply all configurations that inherit from IEntityTypeConfiguration<T> and have a parameterless constructor
        modelBuilder.ApplyConfigurationsFromAssembly(GetConfigurationsAssembly());
		
		//For the ones deriving from EntityTypeConfigurationBase<T>, we process scan and apply them as follows:
		var derivedConfigsToRegister = GetConfigurationsAssembly().GetTypes()
																  .Where(t => (t.BaseType?.IsGenericType ?? false) &&
																			  t.BaseType?.GetGenericTypeDefinition() == typeof(EntityTypeConfigurationBase<>))
																  .ToList();
        derivedConfigsToRegister.ForEach(t => modelBuilder.ApplyConfiguration((dynamic)Activator.CreateInstance(t, Env)!));
    }

    public virtual async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken)
    {
        // Dispatch Domain Events collection. 
        // Choices:
        // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
        // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
        // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
        // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
        await Mediator.DispatchDomainEventsAsync(this);

        ResetReferentialEntitiesState();
			
        var entries = GetEntriesForAudit();
			
        // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
        // performed through the DbContext will be committed
        var result = await base.SaveChangesAsync(cancellationToken);

        AuditLog.Audit(entries).Information("Audit Trail");
			
        return true;
    }

    protected virtual void ResetReferentialEntitiesState()
    {
        foreach (var entry in ChangeTracker.Entries<IReferential>())
            entry.State = EntityState.Unchanged;
    }

	protected virtual List<AuditEntry> GetEntriesForAudit() =>
		ChangeTracker.Entries()
					 .Where(x => new[]
								 {
									 EntityState.Added,
									 EntityState.Modified,
									 EntityState.Deleted
								 }.Contains(x.State) &&
								 x.Entity is not INonAuditable)
					 .Select(x => new AuditEntry(x))
					 .ToList();
}