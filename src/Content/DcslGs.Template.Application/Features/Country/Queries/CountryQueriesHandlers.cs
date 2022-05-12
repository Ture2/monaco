﻿using DcslGs.Template.Application.DTOs;
using DcslGs.Template.Application.DTOs.Extensions;
using DcslGs.Template.Common.Application.Queries.Extensions;
using DcslGs.Template.Application.Infrastructure.Context;
using MediatR;

namespace DcslGs.Template.Application.Features.Country.Queries;

public sealed class CountryQueriesHandlers : IRequestHandler<GetCountryListQuery, List<CountryDto>>,
											 IRequestHandler<GetCountryByIdQuery, CountryDto?>
{
	private readonly AppDbContext _dbContext;

	public CountryQueriesHandlers(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public Task<List<CountryDto>> Handle(GetCountryListQuery request, CancellationToken cancellationToken) =>
		request.ExecuteQueryAsync(_dbContext,
								  x => x.Map()!,
								  nameof(CountryDto.Name),
								  CountryExtensions.GetMappedFields(),
								  cancellationToken);

	public Task<CountryDto?> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken) =>
		request.ExecuteQueryAsync<Domain.Model.Country, CountryDto>(_dbContext,
																	x => x.Map(),
																	cancellationToken);
}