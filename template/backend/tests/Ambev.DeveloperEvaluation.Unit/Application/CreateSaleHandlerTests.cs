using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesSale()
    {
        var repository = Substitute.For<ISaleRepository>();
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var handler = new CreateSaleHandler(repository, mediator, mapper);

        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerExternalId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchExternalId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new SaleItemInput
                {
                    ProductExternalId = Guid.NewGuid(),
                    ProductName = "Beer",
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };

        var createdSale = Sale.Create(
            command.SaleNumber,
            command.SaleDate,
            command.CustomerExternalId,
            command.CustomerName,
            command.BranchExternalId,
            command.BranchName,
            command.Items.Select(x => (x.ProductExternalId, x.ProductName, x.Quantity, x.UnitPrice)));

        repository.ExistsBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns(false);
        repository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(createdSale);
        mapper.Map<SaleResult>(createdSale).Returns(new SaleResult { Id = createdSale.Id, SaleNumber = createdSale.SaleNumber });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Id.Should().Be(createdSale.Id);
        await repository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await mediator.Received().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaleNumberAlreadyExists_ThrowsInvalidOperationException()
    {
        var repository = Substitute.For<ISaleRepository>();
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var handler = new CreateSaleHandler(repository, mediator, mapper);

        var command = BuildValidCommand();
        repository.ExistsBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await repository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await mediator.DidNotReceive().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommandIsInvalid_ThrowsValidationException()
    {
        var repository = Substitute.For<ISaleRepository>();
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var handler = new CreateSaleHandler(repository, mediator, mapper);

        var command = BuildValidCommand();
        command.Items = [];

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    private static CreateSaleCommand BuildValidCommand()
    {
        return new CreateSaleCommand
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerExternalId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchExternalId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new SaleItemInput
                {
                    ProductExternalId = Guid.NewGuid(),
                    ProductName = "Beer",
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };
    }
}
