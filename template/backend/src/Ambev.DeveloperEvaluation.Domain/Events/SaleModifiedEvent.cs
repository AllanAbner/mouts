using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public record SaleModifiedEvent(Guid SaleId, string SaleNumber) : INotification;
