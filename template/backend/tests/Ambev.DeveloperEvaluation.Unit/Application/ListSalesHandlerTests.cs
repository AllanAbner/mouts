using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class ListSalesHandlerTests
{
    [Fact]
    public async Task Handle_WithPagingAndFilters_DelegatesToRepositoryAndMapsResult()
    {
        var repository = Substitute.For<ISaleRepository>();
        var mapper = Substitute.For<IMapper>();
        var handler = new ListSalesHandler(repository, mapper);

        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [(Guid.NewGuid(), "Beer", 4, 10m)]);

        var query = new ListSalesQuery
        {
            Page = 2,
            Size = 5,
            Order = "saleDate desc",
            MinDate = DateTime.UtcNow.AddDays(-1),
            MaxDate = DateTime.UtcNow.AddDays(1),
            CustomerExternalId = sale.CustomerExternalId,
            BranchExternalId = sale.BranchExternalId,
            IsCancelled = false
        };

        repository.ListAsync(
                query.Page,
                query.Size,
                query.Order,
                query.MinDate,
                query.MaxDate,
                query.CustomerExternalId,
                query.BranchExternalId,
                query.IsCancelled,
                Arg.Any<CancellationToken>())
            .Returns(new PaginatedResult<Sale>([sale], 2, 5, 8));

        mapper.Map<SaleListItemResult>(sale).Returns(new SaleListItemResult { Id = sale.Id, SaleNumber = sale.SaleNumber });

        var result = await handler.Handle(query, CancellationToken.None);

        result.CurrentPage.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(8);
        result.Items.Should().ContainSingle(x => x.SaleNumber == "SALE-001");
    }
}
