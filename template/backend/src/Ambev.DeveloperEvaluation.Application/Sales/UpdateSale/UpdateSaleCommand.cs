using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleCommand : IRequest<SaleResult>
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerExternalId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchExternalId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public uint Version { get; set; }
    public List<SaleItemInput> Items { get; set; } = [];
}
