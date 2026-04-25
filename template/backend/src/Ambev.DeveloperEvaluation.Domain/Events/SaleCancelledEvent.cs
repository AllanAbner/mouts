using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public record SaleCancelledEvent(Guid SaleId, string SaleNumber) : INotification;
