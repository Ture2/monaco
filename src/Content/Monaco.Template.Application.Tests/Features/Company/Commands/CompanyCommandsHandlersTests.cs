﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Monaco.Template.Application.Features.Company.Commands;
using Monaco.Template.Application.Infrastructure.Context;
using Monaco.Template.Common.Tests.Factories;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Monaco.Template.Application.Tests.Features.Company.Commands;

[ExcludeFromCodeCoverage]
public class CompanyCommandsHandlersTests
{
    [Trait("Application Commands", "Company Commands")]
    [Theory(DisplayName = "Create new company succeeds")]
    [AnonymousData]
    public async Task CreateNewCompanySucceeds(Domain.Model.Country country)
    {
        var dbContextMock = new Mock<AppDbContext>();
        var companyDbSetMock = new List<Domain.Model.Company>().AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(x => x.Set<Domain.Model.Company>())
                     .Returns(companyDbSetMock.Object);
        var countryDbSetMock = new[] { country }.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(x => x.Set<Domain.Model.Country>())
                     .Returns(countryDbSetMock.Object);
        var commandMock = new Mock<CompanyCreateCommand>(It.IsAny<string>(),
														 It.IsAny<string>(),
														 It.IsAny<string>(),
														 It.IsAny<string>(),
														 It.IsAny<string>(),
														 It.IsAny<string>(),
														 It.IsAny<string>(),
														 It.IsAny<Guid>());
			
        var sut = new CompanyCommandsHandlers(dbContextMock.Object);
        var result = await sut.Handle(commandMock.Object, new CancellationToken());

        companyDbSetMock.Verify(x => x.Attach(It.IsAny<Domain.Model.Company>()), Times.Once);
        dbContextMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.ValidationResult.IsValid.Should().BeTrue();
        result.ItemNotFound.Should().BeFalse();
    }

    [Trait("Application Commands", "Company Commands")]
    [Theory(DisplayName = "Edit company succeeds")]
    [AnonymousData]
    public async Task EditCompanySucceeds(Domain.Model.Country country)
    {
        var companyMock = new Mock<Domain.Model.Company>();
        var dbContextMock = new Mock<AppDbContext>();
        var companyDbSetMock = new List<Domain.Model.Company> {companyMock.Object}.AsQueryable().BuildMockDbSet();
        companyDbSetMock.Setup(x => x.FindAsync(new object[] {It.IsAny<Guid>()}, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(companyMock.Object);
        dbContextMock.Setup(x => x.Set<Domain.Model.Company>())
                     .Returns(companyDbSetMock.Object);
        var countryDbSetMock = new[] { country }.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(x => x.Set<Domain.Model.Country>())
                     .Returns(countryDbSetMock.Object);
        var commandMock = new Mock<CompanyEditCommand>(It.IsAny<Guid>(),
													   It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<string>(),
													   It.IsAny<Guid>());

        var sut = new CompanyCommandsHandlers(dbContextMock.Object);
        var result = await sut.Handle(commandMock.Object, new CancellationToken());

        companyMock.Verify(x => x.Update(It.IsAny<string>(),
                                         It.IsAny<string>(),
                                         It.IsAny<string>(),
                                         It.IsAny<string>(),
                                         It.IsAny<string>(),
                                         It.IsAny<string>(),
                                         It.IsAny<string>(),
                                         It.IsAny<Domain.Model.Country>()),
                           Times.Once);
        dbContextMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.ValidationResult.IsValid.Should().BeTrue();
        result.ItemNotFound.Should().BeFalse();
    }
}