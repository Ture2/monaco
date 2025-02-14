﻿using Monaco.Template.Common.Application.Validators.Extensions;
using Monaco.Template.Common.Infrastructure.Context.Extensions;
using Monaco.Template.Application.Infrastructure.Context;
using FluentValidation;

namespace Monaco.Template.Application.Features.Company.Commands.Validators;

public sealed class CompanyEditCommandValidator : AbstractValidator<CompanyEditCommand>
{
	public CompanyEditCommandValidator(AppDbContext dbContext)
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		this.CheckIfExists<CompanyEditCommand, Domain.Model.Company>(dbContext);

		RuleFor(x => x.Name)
			.NotEmpty()
			.MaximumLength(100)
			.MustAsync(async (cmd, name, ct) => !await dbContext.ExistsAsync<Domain.Model.Company>(x => x.Id != cmd.Id &&
																										x.Name == name,
																								   ct))
			.WithMessage("Another company with the name {PropertyValue} already exists");

		RuleFor(x => x.Email)
			.NotEmpty()
			.EmailAddress();

		RuleFor(x => x.WebSiteUrl)
			.MaximumLength(300);

		RuleFor(x => x.Address)
			.MaximumLength(100);

		RuleFor(x => x.City)
			.MaximumLength(100);

		RuleFor(x => x.County)
			.MaximumLength(100);

		RuleFor(x => x.PostCode)
			.MaximumLength(10);

		RuleFor(x => x.CountryId)
			.NotEmpty()
			.MustExistAsync<CompanyEditCommand, Domain.Model.Country>(dbContext);
	}
}