using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, PaginatedResult<SaleListItemResult>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<SaleListItemResult>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var validator = new ListSalesValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var result = await _saleRepository.ListAsync(
            request.Page,
            request.Size,
            request.Order,
            request.MinDate,
            request.MaxDate,
            request.CustomerExternalId,
            request.BranchExternalId,
            request.IsCancelled,
            cancellationToken);

        return new PaginatedResult<SaleListItemResult>(
            result.Items.Select(_mapper.Map<SaleListItemResult>),
            result.CurrentPage,
            result.PageSize,
            result.TotalCount);
    }
}
