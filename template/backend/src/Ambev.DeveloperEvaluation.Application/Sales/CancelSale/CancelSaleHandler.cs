using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CancelSaleHandler(ISaleRepository saleRepository, IMediator mediator, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {request.Id} was not found");

        sale.CancelSale();

        foreach (var domainEvent in sale.DomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        sale.ClearDomainEvents();

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(sale);
    }
}
