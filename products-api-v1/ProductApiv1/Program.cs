using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Validators;
using System.Text.Json.Serialization;
using FluentValidation.Results;
using Scalar.AspNetCore;
using ProductApi.Models;
using ProductApi.DTOs;
using ProductApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>  
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductRequestValidator>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
});
// One HTTP Reuqest -> One Db Context 

builder.Services.AddOpenApi();


var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

static Dictionary<string, string[]> ToValidationErrors(ValidationResult validationResult)
{
    return validationResult.Errors
        .GroupBy(error => error.PropertyName)
        .ToDictionary(
            group => group.Key,
            group => group.Select(error => error.ErrorMessage).ToArray());
}


app.MapGet("/", () => Results.Ok(new
{
    App = "ProductMinimalApi",
    Message = "Product CRUD API using Minimal API, EF Core, FluentValidation, and Scalar.",
    OpenApi = "/openapi/v1.json",
    Scalar = "/scalar/v1"
}));

// GET Get all products 
app.MapGet("/products", async (
    AppDbContext db, 
    string? search , 
    decimal? minPrice, 
    decimal? maxPrice,
    ProductStatus? status,
    bool? inStockOnly
) =>
{
    

});


app.MapGet("/products/{id:guid}", async(Guid id, AppDbContext db ) =>
{
    var product = await db.Products.AsNoTracking().FirstOrDefaultAsync( product => product.Id == id);


    // product.Stocks = 0;

    // productViews 
    // productViews.View +=1 
    // db.SaveChanges();

    return product is null
    ? Results.NotFound()
    : Results.Ok(product);
    
});

app.MapPost("/products" , async (CreateProductRequest request, 
IValidator<CreateProductRequest> validator,
AppDbContext db
) =>
{
    var validationResult = await validator.ValidateAsync(request);

    
    if(!validationResult.IsValid)
    {
        // validation failed 
        return Results.ValidationProblem(ToValidationErrors(validationResult));
    }

    // payload is valid
    var product = new Product
    {
        Name = request.Name,
        Description = request.Description,
        Price = request.Price,
        Stocks = request.Stocks,
        Status = request.Status!.Value,
        CreatedDate = DateTime.UtcNow.MakeItUTC()
    };

    db.Products.Add(product);
    db.SaveChanges();

    return Results.Created("product:"+product.Id,product);

});


// POST Create Product => CreateProductRequest
// PATCH Update product => UpdateProductRequest not null fields db update , Example Price -> only update price
// DELETE Delete 


app.Run();
