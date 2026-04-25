using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMediator mediator, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {request.Id} was not found");

        sale.UpdateHeader(
            request.SaleNumber,
            request.SaleDate,
            request.CustomerExternalId,
            request.CustomerName,
            request.BranchExternalId,
            request.BranchName);

        sale.ReplaceItems(request.Items.Select(item =>
            (item.Id, item.ProductExternalId, item.ProductName, item.Quantity, item.UnitPrice)));

        foreach (var domainEvent in sale.DomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        sale.ClearDomainEvents();

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(sale);
    }
}
