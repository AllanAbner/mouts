using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SaleCreatedEventLoggingHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedEventLoggingHandler> _logger;

    public SaleCreatedEventLoggingHandler(ILogger<SaleCreatedEventLoggingHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SaleCreated {SaleId} {SaleNumber}", notification.SaleId, notification.SaleNumber);
        return Task.CompletedTask;
    }
}

public class SaleModifiedEventLoggingHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedEventLoggingHandler> _logger;

    public SaleModifiedEventLoggingHandler(ILogger<SaleModifiedEventLoggingHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SaleModified {SaleId} {SaleNumber}", notification.SaleId, notification.SaleNumber);
        return Task.CompletedTask;
    }
}

public class SaleCancelledEventLoggingHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledEventLoggingHandler> _logger;

    public SaleCancelledEventLoggingHandler(ILogger<SaleCancelledEventLoggingHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SaleCancelled {SaleId} {SaleNumber}", notification.SaleId, notification.SaleNumber);
        return Task.CompletedTask;
    }
}

public class ItemCancelledEventLoggingHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledEventLoggingHandler> _logger;

    public ItemCancelledEventLoggingHandler(ILogger<ItemCancelledEventLoggingHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ItemCancelled {SaleId} {SaleItemId} {SaleNumber}",
            notification.SaleId,
            notification.SaleItemId,
            notification.SaleNumber);
        return Task.CompletedTask;
    }
}
