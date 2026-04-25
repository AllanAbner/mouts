using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<SaleItem, SaleItemResult>();

        CreateMap<Sale, SaleResult>();

        CreateMap<Sale, SaleListItemResult>();
    }
}
