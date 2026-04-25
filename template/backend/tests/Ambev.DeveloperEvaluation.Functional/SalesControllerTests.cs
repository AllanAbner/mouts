using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class SalesControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedResponse()
    {
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var controller = new SalesController(mediator, mapper);

        var request = new CreateSaleRequest
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerExternalId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchExternalId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new SaleItemRequest
                {
                    ProductExternalId = Guid.NewGuid(),
                    ProductName = "Beer",
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };

        var command = new CreateSaleCommand();
        var result = new SaleResult { Id = Guid.NewGuid(), SaleNumber = "SALE-001" };
        var response = new SaleResponse { Id = result.Id, SaleNumber = result.SaleNumber };

        mapper.Map<CreateSaleCommand>(request).Returns(command);
        mediator.Send(command, Arg.Any<CancellationToken>()).Returns(result);
        mapper.Map<SaleResponse>(result).Returns(response);

        var actionResult = await controller.Create(request, CancellationToken.None);

        actionResult.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)actionResult).Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Cancel_ReturnsOkResponse()
    {
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var controller = new SalesController(mediator, mapper);

        var saleId = Guid.NewGuid();
        var result = new SaleResult { Id = saleId, IsCancelled = true };
        var response = new SaleResponse { Id = saleId, IsCancelled = true };

        mediator.Send(Arg.Any<CancelSaleCommand>(), Arg.Any<CancellationToken>()).Returns(result);
        mapper.Map<SaleResponse>(result).Returns(response);

        var actionResult = await controller.Cancel(saleId, CancellationToken.None);

        actionResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CancelItem_ReturnsOkResponse()
    {
        var mediator = Substitute.For<IMediator>();
        var mapper = Substitute.For<IMapper>();
        var controller = new SalesController(mediator, mapper);

        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var result = new SaleResult { Id = saleId };
        var response = new SaleResponse { Id = saleId };

        mediator.Send(Arg.Any<CancelSaleItemCommand>(), Arg.Any<CancellationToken>()).Returns(result);
        mapper.Map<SaleResponse>(result).Returns(response);

        var actionResult = await controller.CancelItem(saleId, itemId, CancellationToken.None);

        actionResult.Should().BeOfType<OkObjectResult>();
    }
}
