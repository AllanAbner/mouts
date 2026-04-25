using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class SaleRepositoryTests
{
    [Fact]
    public async Task CreateAndGetById_PersistsSale()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new DefaultContext(options);
        var repository = new SaleRepository(context);

        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [(Guid.NewGuid(), "Beer", 4, 10m)]);

        await repository.CreateAsync(sale);
        var stored = await repository.GetByIdAsync(sale.Id);

        stored.Should().NotBeNull();
        stored!.SaleNumber.Should().Be("SALE-001");
        stored.TotalAmount.Should().Be(36m);
    }

    [Fact]
    public async Task ExistsBySaleNumberAsync_ReturnsTrueForPersistedSale()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new DefaultContext(options);
        var repository = new SaleRepository(context);

        var sale = Sale.Create(
            "SALE-XYZ",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer",
            Guid.NewGuid(),
            "Branch",
            [(Guid.NewGuid(), "Beer", 4, 10m)]);

        await repository.CreateAsync(sale);

        var exists = await repository.ExistsBySaleNumberAsync("SALE-XYZ");

        exists.Should().BeTrue();
    }
}
