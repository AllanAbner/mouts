using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IMediator mediator, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {request.SaleId} was not found");

        sale.CancelItem(request.ItemId);

        foreach (var domainEvent in sale.DomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        sale.ClearDomainEvents();

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(sale);
    }
}
