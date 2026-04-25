using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact]
    public void Create_WithLessThanFourItems_DoesNotApplyDiscount()
    {
        var sale = CreateSale([(Guid.NewGuid(), "Beer", 3, 10m)]);

        sale.TotalAmount.Should().Be(30m);
        sale.Items.Single().Discount.Should().Be(0m);
    }

    [Fact]
    public void Create_WithFourItems_AppliesTenPercentDiscount()
    {
        var sale = CreateSale([(Guid.NewGuid(), "Beer", 4, 10m)]);

        sale.TotalAmount.Should().Be(36m);
        sale.Items.Single().Discount.Should().Be(4m);
    }

    [Fact]
    public void Create_WithTenItems_AppliesTwentyPercentDiscount()
    {
        var sale = CreateSale([(Guid.NewGuid(), "Beer", 10, 10m)]);

        sale.TotalAmount.Should().Be(80m);
        sale.Items.Single().Discount.Should().Be(20m);
    }

    [Fact]
    public void Create_WithTwentyItems_AppliesTwentyPercentDiscount()
    {
        var sale = CreateSale([(Guid.NewGuid(), "Beer", 20, 10m)]);

        sale.TotalAmount.Should().Be(160m);
        sale.Items.Single().Discount.Should().Be(40m);
    }

    [Fact]
    public void Create_WithoutItems_ThrowsDomainException()
    {
        var act = () => CreateSale([]);

        act.Should().Throw<DomainException>()
            .WithMessage("*at least one valid item*");
    }

    [Fact]
    public void Create_WithMoreThanTwentyItems_ThrowsDomainException()
    {
        var act = () => CreateSale([(Guid.NewGuid(), "Beer", 21, 10m)]);

        act.Should().Throw<DomainException>()
            .WithMessage("*more than 20*");
    }

    [Fact]
    public void CancelItem_RemovesItemValueFromTotal()
    {
        var sale = CreateSale(
        [
            (Guid.NewGuid(), "Beer", 4, 10m),
            (Guid.NewGuid(), "Snack", 2, 5m)
        ]);

        var itemId = sale.Items.First().Id;
        sale.CancelItem(itemId);

        sale.TotalAmount.Should().Be(10m);
        sale.Items.First().IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void CancelItem_WhenCancellingLastActiveItem_ThrowsDomainException()
    {
        var sale = CreateSale([(Guid.NewGuid(), "Beer", 2, 10m)]);

        var act = () => sale.CancelItem(sale.Items.Single().Id);

        act.Should().Throw<DomainException>()
            .WithMessage("*at least one valid item*");
    }

    [Fact]
    public void CancelSale_MarksSaleAsCancelled()
    {
        var sale = CreateSale([(Guid.NewGuid(), "Beer", 2, 10m)]);

        sale.CancelSale();

        sale.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void UpdateHeader_WhenSaleCancelled_ThrowsDomainException()
    {
        var sale = CreateSale([(Guid.NewGuid(), "Beer", 2, 10m)]);

        sale.CancelSale();

        var act = () => sale.UpdateHeader(
            "SALE-004",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Other Customer",
            Guid.NewGuid(),
            "Other Branch");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateItem_WhenItemCancelled_ThrowsDomainException()
    {
        var sale = CreateSale(
        [
            (Guid.NewGuid(), "Beer", 4, 10m),
            (Guid.NewGuid(), "Snack", 2, 5m)
        ]);

        var itemId = sale.Items.First().Id;
        sale.CancelItem(itemId);

        var act = () => sale.UpdateItem(itemId, "Beer", 5, 10m);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cancelled item cannot be changed*");
    }

    [Fact]
    public void ReplaceItems_WhenInputIsInvalid_DoesNotMutateAggregate()
    {
        var sale = CreateSale(
        [
            (Guid.NewGuid(), "Beer", 4, 10m),
            (Guid.NewGuid(), "Snack", 2, 5m)
        ]);

        var originalTotal = sale.TotalAmount;
        var originalActiveItems = sale.Items.Count(x => !x.IsCancelled);
        var existingItem = sale.Items.First();

        var act = () => sale.ReplaceItems(
        [
            (existingItem.Id, existingItem.ProductExternalId, existingItem.ProductName, 4, 10m),
            ((Guid?)null, Guid.NewGuid(), "Invalid", 21, 10m)
        ]);

        act.Should().Throw<DomainException>();
        sale.TotalAmount.Should().Be(originalTotal);
        sale.Items.Count(x => !x.IsCancelled).Should().Be(originalActiveItems);
    }

    [Fact]
    public void ReplaceItems_WithDuplicatedItemIds_ThrowsDomainException()
    {
        var sale = CreateSale(
        [
            (Guid.NewGuid(), "Beer", 4, 10m),
            (Guid.NewGuid(), "Snack", 2, 5m)
        ]);

        var existingItem = sale.Items.First();

        var act = () => sale.ReplaceItems(
        [
            (existingItem.Id, existingItem.ProductExternalId, existingItem.ProductName, 4, 10m),
            (existingItem.Id, existingItem.ProductExternalId, existingItem.ProductName, 5, 10m)
        ]);

        act.Should().Throw<DomainException>()
            .WithMessage("*Duplicated item ids*");
    }

    private static Sale CreateSale(IEnumerable<(Guid ProductExternalId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        return Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            items);
    }
}
