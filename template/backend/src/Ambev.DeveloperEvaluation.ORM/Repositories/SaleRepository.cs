using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales.AnyAsync(x => x.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PaginatedResult<Sale>> ListAsync(
        int page,
        int size,
        string? order,
        DateTime? minDate,
        DateTime? maxDate,
        Guid? customerId,
        Guid? branchId,
        bool? isCancelled,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Sale> query = _context.Sales.AsNoTracking();

        if (minDate.HasValue)
            query = query.Where(x => x.SaleDate >= minDate.Value);

        if (maxDate.HasValue)
            query = query.Where(x => x.SaleDate <= maxDate.Value);

        if (customerId.HasValue)
            query = query.Where(x => x.CustomerExternalId == customerId.Value);

        if (branchId.HasValue)
            query = query.Where(x => x.BranchExternalId == branchId.Value);

        if (isCancelled.HasValue)
            query = query.Where(x => x.IsCancelled == isCancelled.Value);

        query = ApplyOrdering(query, order);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Sale>(items, page, size, totalCount);
    }

    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(sale);
        if (entry.State == EntityState.Detached)
        {
            _context.Attach(sale);
            entry = _context.Entry(sale);
        }

        var persistedItemIds = (await _context.Set<SaleItem>()
            .Where(x => EF.Property<Guid>(x, "SaleId") == sale.Id)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        foreach (var item in sale.Items)
        {
            var itemEntry = _context.Entry(item);

            if (!persistedItemIds.Contains(item.Id))
            {
                itemEntry.State = EntityState.Added;
                continue;
            }

            if (itemEntry.State == EntityState.Detached)
            {
                _context.Attach(item);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderByDescending(x => x.SaleDate);

        IOrderedQueryable<Sale>? ordered = null;
        foreach (var rawPart in order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = rawPart.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var field = tokens[0].ToLowerInvariant();
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            ordered = ApplySingleOrdering(ordered ?? query.OrderBy(x => 0), field, desc, ordered == null);
        }

        return ordered ?? query.OrderByDescending(x => x.SaleDate);
    }

    private static IOrderedQueryable<Sale> ApplySingleOrdering(IOrderedQueryable<Sale> query, string field, bool desc, bool first)
    {
        return (field, desc, first) switch
        {
            ("salenumber", true, true) => query.OrderByDescending(x => x.SaleNumber),
            ("salenumber", false, true) => query.OrderBy(x => x.SaleNumber),
            ("saledate", true, true) => query.OrderByDescending(x => x.SaleDate),
            ("saledate", false, true) => query.OrderBy(x => x.SaleDate),
            ("customername", true, true) => query.OrderByDescending(x => x.CustomerName),
            ("customername", false, true) => query.OrderBy(x => x.CustomerName),
            ("branchname", true, true) => query.OrderByDescending(x => x.BranchName),
            ("branchname", false, true) => query.OrderBy(x => x.BranchName),
            ("totalamount", true, true) => query.OrderByDescending(x => x.TotalAmount),
            ("totalamount", false, true) => query.OrderBy(x => x.TotalAmount),
            ("iscancelled", true, true) => query.OrderByDescending(x => x.IsCancelled),
            ("iscancelled", false, true) => query.OrderBy(x => x.IsCancelled),
            ("salenumber", true, false) => query.ThenByDescending(x => x.SaleNumber),
            ("salenumber", false, false) => query.ThenBy(x => x.SaleNumber),
            ("saledate", true, false) => query.ThenByDescending(x => x.SaleDate),
            ("saledate", false, false) => query.ThenBy(x => x.SaleDate),
            ("customername", true, false) => query.ThenByDescending(x => x.CustomerName),
            ("customername", false, false) => query.ThenBy(x => x.CustomerName),
            ("branchname", true, false) => query.ThenByDescending(x => x.BranchName),
            ("branchname", false, false) => query.ThenBy(x => x.BranchName),
            ("totalamount", true, false) => query.ThenByDescending(x => x.TotalAmount),
            ("totalamount", false, false) => query.ThenBy(x => x.TotalAmount),
            ("iscancelled", true, false) => query.ThenByDescending(x => x.IsCancelled),
            ("iscancelled", false, false) => query.ThenBy(x => x.IsCancelled),
            _ => query
        };
    }
}
