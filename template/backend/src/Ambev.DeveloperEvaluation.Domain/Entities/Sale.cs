using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = [];
    private readonly List<INotification> _domainEvents = [];

    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }
    public Guid CustomerExternalId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public Guid BranchExternalId { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private Sale()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public static Sale Create(
        string saleNumber,
        DateTime saleDate,
        Guid customerExternalId,
        string customerName,
        Guid branchExternalId,
        string branchName,
        IEnumerable<(Guid ProductExternalId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        var sale = new Sale();
        sale.UpdateHeader(saleNumber, saleDate, customerExternalId, customerName, branchExternalId, branchName, false);

        foreach (var item in items)
        {
            sale.AddItem(item.ProductExternalId, item.ProductName, item.Quantity, item.UnitPrice);
        }

        sale.EnsureHasActiveItems();
        sale.AddDomainEvent(new SaleCreatedEvent(sale.Id, sale.SaleNumber));
        return sale;
    }

    public void UpdateHeader(
        string saleNumber,
        DateTime saleDate,
        Guid customerExternalId,
        string customerName,
        Guid branchExternalId,
        string branchName)
    {
        UpdateHeader(saleNumber, saleDate, customerExternalId, customerName, branchExternalId, branchName, true);
    }

    public void AddItem(Guid productExternalId, string productName, int quantity, decimal unitPrice)
    {
        EnsureNotCancelled();

        var item = new SaleItem(productExternalId, productName, quantity, unitPrice);
        _items.Add(item);
        Touch();
        RecalculateTotals();
    }

    public void UpdateItem(Guid itemId, string productName, int quantity, decimal unitPrice)
    {
        EnsureNotCancelled();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Sale item with id {itemId} was not found");

        item.UpdateDetails(productName, quantity, unitPrice);
        Touch();
        RecalculateTotals();
    }

    public void ReplaceItems(IEnumerable<(Guid? ItemId, Guid ProductExternalId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        EnsureNotCancelled();

        var incomingItems = items.ToList();
        if (incomingItems.Count == 0)
            throw new DomainException("Sale must contain at least one item");

        var activeExistingItems = _items.Where(i => !i.IsCancelled).ToDictionary(i => i.Id);
        var requestedIds = incomingItems.Where(i => i.ItemId.HasValue).Select(i => i.ItemId!.Value).ToHashSet();

        ValidateReplacementItems(incomingItems, activeExistingItems);

        foreach (var existingItem in _items.Where(i => !i.IsCancelled && !requestedIds.Contains(i.Id)).ToList())
        {
            existingItem.Cancel();
        }

        foreach (var item in incomingItems)
        {
            if (item.ItemId.HasValue)
            {
                if (!activeExistingItems.TryGetValue(item.ItemId.Value, out var existingItem))
                    throw new DomainException("Cannot update cancelled or missing items");

                existingItem.UpdateDetails(item.ProductName, item.Quantity, item.UnitPrice);
                continue;
            }

            AddItem(item.ProductExternalId, item.ProductName, item.Quantity, item.UnitPrice);
        }

        EnsureHasActiveItems();
        Touch();
        RecalculateTotals();
        AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber));
    }

    public void CancelItem(Guid itemId)
    {
        EnsureNotCancelled();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Sale item with id {itemId} was not found");

        item.Cancel();
        EnsureHasActiveItems();
        Touch();
        RecalculateTotals();
        AddDomainEvent(new ItemCancelledEvent(Id, itemId, SaleNumber));
    }

    public void CancelSale()
    {
        if (IsCancelled)
            return;

        IsCancelled = true;
        Touch();
        AddDomainEvent(new SaleCancelledEvent(Id, SaleNumber));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void UpdateHeader(
        string saleNumber,
        DateTime saleDate,
        Guid customerExternalId,
        string customerName,
        Guid branchExternalId,
        string branchName,
        bool addModifiedEvent)
    {
        EnsureNotCancelled();

        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number is required");

        if (customerExternalId == Guid.Empty)
            throw new DomainException("Customer external id is required");

        if (branchExternalId == Guid.Empty)
            throw new DomainException("Branch external id is required");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("Customer name is required");

        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("Branch name is required");

        SaleNumber = saleNumber.Trim();
        SaleDate = saleDate;
        CustomerExternalId = customerExternalId;
        CustomerName = customerName.Trim();
        BranchExternalId = branchExternalId;
        BranchName = branchName.Trim();

        if (Id == Guid.Empty)
            Id = Guid.NewGuid();

        if (addModifiedEvent)
        {
            Touch();
            AddDomainEvent(new SaleModifiedEvent(Id, SaleNumber));
        }
    }

    private void EnsureNotCancelled()
    {
        if (IsCancelled)
            throw new DomainException("Cancelled sale cannot be changed");
    }

    private void EnsureHasActiveItems()
    {
        if (_items.All(i => i.IsCancelled))
            throw new DomainException("Sale must contain at least one valid item");
    }

    private static void ValidateReplacementItems(
        List<(Guid? ItemId, Guid ProductExternalId, string ProductName, int Quantity, decimal UnitPrice)> incomingItems,
        Dictionary<Guid, SaleItem> activeExistingItems)
    {
        var duplicateIds = incomingItems
            .Where(i => i.ItemId.HasValue)
            .GroupBy(i => i.ItemId!.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
            throw new DomainException("Duplicated item ids are not allowed in sale update");

        foreach (var item in incomingItems)
        {
            if (item.ItemId.HasValue && !activeExistingItems.ContainsKey(item.ItemId.Value))
                throw new DomainException("Cannot update cancelled or missing items");

            SaleItem.Validate(item.ProductExternalId, item.ProductName, item.Quantity, item.UnitPrice);
        }
    }

    private void RecalculateTotals()
    {
        TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    private void AddDomainEvent(INotification notification)
    {
        _domainEvents.Add(notification);
    }
}
