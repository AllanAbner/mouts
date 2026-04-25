using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public class ListSalesRequest
{
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    [FromQuery(Name = "_order")]
    public string? Order { get; set; }

    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public Guid? CustomerExternalId { get; set; }
    public Guid? BranchExternalId { get; set; }
    public bool? IsCancelled { get; set; }
}
