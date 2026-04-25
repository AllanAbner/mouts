using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateSaleHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_UpdatesSale()
    {
        var repository = Substitute.For<ISaleRepository>();
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var handler = new UpdateSaleHandler(repository, mediator, mapper);

        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [(Guid.NewGuid(), "Beer", 2, 10m)]);

        var item = sale.Items.Single();
        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleNumber = "SALE-002",
            SaleDate = DateTime.UtcNow,
            CustomerExternalId = Guid.NewGuid(),
            CustomerName = "Updated Customer",
            BranchExternalId = Guid.NewGuid(),
            BranchName = "Updated Branch",
            Items =
            [
                new SaleItemInput
                {
                    Id = item.Id,
                    ProductExternalId = item.ProductExternalId,
                    ProductName = item.ProductName,
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };

        repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        mapper.Map<SaleResult>(sale).Returns(new SaleResult { Id = sale.Id, SaleNumber = "SALE-002" });

        var result = await handler.Handle(command, CancellationToken.None);

        result.SaleNumber.Should().Be("SALE-002");
        await repository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await mediator.Received().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommandIsInvalid_ThrowsValidationException()
    {
        var repository = Substitute.For<ISaleRepository>();
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var handler = new UpdateSaleHandler(repository, mediator, mapper);

        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            Items = []
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }
}
