namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SaleListItemResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerExternalId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchExternalId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}
