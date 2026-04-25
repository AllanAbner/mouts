using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var command = _mapper.Map<CreateSaleCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, new ApiResponseWithData<SaleResponse>
        {
            Success = true,
            Message = "Sale created successfully",
            Data = _mapper.Map<SaleResponse>(response)
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new GetSaleRequest { Id = id };
        var validator = new GetSaleRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var response = await _mediator.Send(new GetSaleQuery(id), cancellationToken);

        return new OkObjectResult(new ApiResponseWithData<SaleResponse>
        {
            Success = true,
            Message = "Sale retrieved successfully",
            Data = _mapper.Map<SaleResponse>(response)
        });
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<SaleListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ListSalesRequest request, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<ListSalesQuery>(request);
        var result = await _mediator.Send(query, cancellationToken);

        return new OkObjectResult(new PaginatedResponse<SaleListItemResponse>
        {
            Success = true,
            Message = "Sales retrieved successfully",
            Data = result.Items.Select(_mapper.Map<SaleListItemResponse>),
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount
        });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;

        var response = await _mediator.Send(command, cancellationToken);

        return new OkObjectResult(new ApiResponseWithData<SaleResponse>
        {
            Success = true,
            Message = "Sale updated successfully",
            Data = _mapper.Map<SaleResponse>(response)
        });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new CancelSaleCommand(id), cancellationToken);

        return new OkObjectResult(new ApiResponseWithData<SaleResponse>
        {
            Success = true,
            Message = "Sale cancelled successfully",
            Data = _mapper.Map<SaleResponse>(response)
        });
    }

    [HttpPatch("{saleId}/items/{itemId}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<SaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelItem([FromRoute] Guid saleId, [FromRoute] Guid itemId, CancellationToken cancellationToken)
    {
        var request = new CancelSaleItemRequest { SaleId = saleId, ItemId = itemId };
        var validator = new CancelSaleItemRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var command = _mapper.Map<CancelSaleItemCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return new OkObjectResult(new ApiResponseWithData<SaleResponse>
        {
            Success = true,
            Message = "Sale item cancelled successfully",
            Data = _mapper.Map<SaleResponse>(response)
        });
    }
}
