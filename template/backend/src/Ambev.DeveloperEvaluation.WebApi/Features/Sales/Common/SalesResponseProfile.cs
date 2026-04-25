using Ambev.DeveloperEvaluation.Application.Sales.Common;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

public class SalesResponseProfile : Profile
{
    public SalesResponseProfile()
    {
        CreateMap<SaleItemResult, SaleItemResponse>();
        CreateMap<SaleResult, SaleResponse>();
        CreateMap<SaleListItemResult, SaleListItemResponse>();
    }
}
