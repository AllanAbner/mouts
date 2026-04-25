using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
{
    public UpdateSaleRequestValidator()
    {
        RuleFor(x => x.SaleNumber).NotEmpty();
        RuleFor(x => x.CustomerExternalId).NotEmpty();
        RuleFor(x => x.BranchExternalId).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0u);
        RuleFor(x => x.Items).NotEmpty();
    }
}
