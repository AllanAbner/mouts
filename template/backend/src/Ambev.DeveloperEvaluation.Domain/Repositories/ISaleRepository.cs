using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository
{
    Task<bool> ExistsBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Sale>> ListAsync(
        int page,
        int size,
        string? order,
        DateTime? minDate,
        DateTime? maxDate,
        Guid? customerId,
        Guid? branchId,
        bool? isCancelled,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Sale sale, uint? expectedVersion, CancellationToken cancellationToken = default);
}
