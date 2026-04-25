using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CreateSaleHandler(ISaleRepository saleRepository, IMediator mediator, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _saleRepository.ExistsBySaleNumberAsync(request.SaleNumber, cancellationToken))
            throw new InvalidOperationException($"Sale number {request.SaleNumber} already exists");

        var sale = Sale.Create(
            request.SaleNumber,
            request.SaleDate,
            request.CustomerExternalId,
            request.CustomerName,
            request.BranchExternalId,
            request.BranchName,
            request.Items.Select(item => (item.ProductExternalId, item.ProductName, item.Quantity, item.UnitPrice)));

        foreach (var domainEvent in sale.DomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        sale.ClearDomainEvents();

        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(createdSale);
    }
}
