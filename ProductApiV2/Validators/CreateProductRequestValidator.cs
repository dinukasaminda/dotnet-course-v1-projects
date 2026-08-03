using FluentValidation;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(100)
            .WithMessage("Product name cannot be longer than 100 characters.");

        RuleFor(request => request.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(500)
            .WithMessage("Description cannot be longer than 500 characters.");

        RuleFor(request => request.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Price cannot be greater than 1,000,000.");

        RuleFor(request => request.Stocks)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stocks cannot be negative.")
            .LessThanOrEqualTo(100_000)
            .WithMessage("Stocks cannot be greater than 100,000.");

        RuleFor(request => request.Status)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Status is required.")
            .Must(status => status is not null &&
                Enum.IsDefined(typeof(ProductStatus), status.Value))
            .WithMessage("Status must be one of: Draft, Active, Inactive, Discontinued.");
    }
}