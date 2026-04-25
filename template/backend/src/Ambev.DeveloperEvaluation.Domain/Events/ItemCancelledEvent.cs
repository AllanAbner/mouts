using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public record ItemCancelledEvent(Guid SaleId, Guid SaleItemId, string SaleNumber) : INotification;
