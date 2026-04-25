using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CancelsItemAndPersistsSale()
    {
        var repository = Substitute.For<ISaleRepository>();
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var handler = new CancelSaleItemHandler(repository, mediator, mapper);

        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [
                (Guid.NewGuid(), "Beer", 4, 10m),
                (Guid.NewGuid(), "Snack", 2, 5m)
            ]);

        var itemId = sale.Items.First().Id;
        repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        mapper.Map<SaleResult>(sale).Returns(new SaleResult { Id = sale.Id, TotalAmount = 10m });

        var result = await handler.Handle(new CancelSaleItemCommand(sale.Id, itemId), CancellationToken.None);

        result.TotalAmount.Should().Be(10m);
        await repository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await mediator.Received().Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }
}
