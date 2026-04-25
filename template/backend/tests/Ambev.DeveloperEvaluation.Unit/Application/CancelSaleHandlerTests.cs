using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CancelsSale()
    {
        var repository = Substitute.For<ISaleRepository>();
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var handler = new CancelSaleHandler(repository, mediator, mapper);

        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [(Guid.NewGuid(), "Beer", 2, 10m)]);

        repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        mapper.Map<SaleResult>(sale).Returns(new SaleResult { Id = sale.Id, IsCancelled = true });

        var result = await handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        result.IsCancelled.Should().BeTrue();
        await repository.Received(1).UpdateAsync(sale, null, Arg.Any<CancellationToken>());
    }
}
