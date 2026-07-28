using FluentValidation;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(request => request)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field is required.");

        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Product name cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Product name cannot be longer than 100 characters.")
            .When(request => request.Name is not null);

        RuleFor(request => request.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot be longer than 500 characters.")
            .When(request => request.Description is not null);

        RuleFor(request => request.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Price cannot be greater than 1,000,000.")
            .When(request => request.Price is not null);

        RuleFor(request => request.Stocks)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stocks cannot be negative.")
            .LessThanOrEqualTo(100_000)
            .WithMessage("Stocks cannot be greater than 100,000.")
            .When(request => request.Stocks is not null);

        RuleFor(request => request.Status)
            .Must(status => status is null ||
                Enum.IsDefined(typeof(ProductStatus), status.Value))
            .WithMessage("Status must be one of: Draft, Active, Inactive, Discontinued.");
    }

    private static bool HaveAtLeastOneField(UpdateProductRequest request)
    {
        return request.Name is not null ||
               request.Description is not null ||
               request.Price is not null ||
               request.Stocks is not null ||
               request.Status is not null;
    }
}

