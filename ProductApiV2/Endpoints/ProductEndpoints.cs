using FluentValidation;
using FluentValidation.Results;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Services;


namespace ProductApi.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetAllProducts);
        group.MapGet("/{id:guid}", GetProductById);
        group.MapPost("/", CreateProduct);
        group.MapPatch("/{id:guid}", UpdateProduct);
        group.MapDelete("/{id:guid}", DeleteProduct);
    }

    private static async Task<IResult> GetAllProducts(
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        ProductStatus? status,
        bool? inStockOnly,
        IProductService productService)
    {
        var products = await productService.GetProductsAsync(
            search,
            minPrice,
            maxPrice,
            status,
            inStockOnly);

        return Results.Ok(products);
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        IProductService productService)
    {
        var product = await productService.GetProductByIdAsync(id);

        return product is null
            ? Results.NotFound()
            : Results.Ok(product);
    }

    private static async Task<IResult> CreateProduct(
       CreateProductRequest request,
       IValidator<CreateProductRequest> validator,
       IProductService productService)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(ToValidationErrors(validationResult));
        }

        var product = await productService.CreateProductAsync(request);

        return Results.Created($"/api/products/{product.Id}", product);
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        IValidator<UpdateProductRequest> validator,
        IProductService productService)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(ToValidationErrors(validationResult));
        }

        var product = await productService.UpdateProductAsync(id, request);

        return product is null
            ? Results.NotFound()
            : Results.Ok(product);
    }

    private static async Task<IResult> DeleteProduct(
        Guid id,
        IProductService productService)
    {
        var deleted = await productService.DeleteProductAsync(id);

        return deleted
            ? Results.NoContent()
            : Results.NotFound();
    }
    private static Dictionary<string, string[]> ToValidationErrors(
        ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
    }

}