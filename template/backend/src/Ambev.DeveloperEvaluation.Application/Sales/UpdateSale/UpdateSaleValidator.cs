using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SaleNumber).NotEmpty();
        RuleFor(x => x.CustomerExternalId).NotEmpty();
        RuleFor(x => x.BranchExternalId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
    }
}
