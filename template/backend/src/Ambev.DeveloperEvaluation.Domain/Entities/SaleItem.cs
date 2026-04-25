using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem
{
    public Guid Id { get; private set; }
    public Guid ProductExternalId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }

    private SaleItem()
    {
    }

    internal SaleItem(Guid productExternalId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ProductExternalId = productExternalId;
        ProductName = productName;
        UpdateDetails(productName, quantity, unitPrice);
    }

    internal void UpdateDetails(string productName, int quantity, decimal unitPrice)
    {
        if (IsCancelled)
            throw new DomainException("Cancelled item cannot be changed");

        Validate(ProductExternalId, productName, quantity, unitPrice);

        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = CalculateDiscount(unitPrice, quantity);
        TotalAmount = quantity * unitPrice - Discount;
    }

    internal void Cancel()
    {
        if (IsCancelled)
            return;

        IsCancelled = true;
        Discount = 0;
        TotalAmount = 0;
    }

    internal static void Validate(Guid productExternalId, string productName, int quantity, decimal unitPrice)
    {
        if (ProductExternalIdIsEmpty(productExternalId))
            throw new DomainException("Product external id is required");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        if (quantity > 20)
            throw new DomainException("Cannot sell more than 20 identical items");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero");
    }

    private static decimal CalculateDiscount(decimal unitPrice, int quantity)
    {
        var grossAmount = unitPrice * quantity;

        if (quantity >= 10)
            return grossAmount * 0.20m;

        if (quantity >= 4)
            return grossAmount * 0.10m;

        return 0;
    }

    private static bool ProductExternalIdIsEmpty(Guid productExternalId)
    {
        return productExternalId == Guid.Empty;
    }
}
