using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Endpoints;
using ProductApi.Services;
using ProductApi.Validators;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// db connection
builder.Services.AddDbContext<AppDbContext>(options =>
{
   options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")); 
});

// Product service registration
builder.Services.AddScoped<IProductService, ProductService>();

// validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductRequestValidator>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => Results.Ok(new
{
    App = "ProductApiv2",
    Message = "Product CRUD API using layered architecture.",
    Layers = "Minimal API endpoint layer -> Service -> EF Core DbContext -> Database",
    OpenApi = "/openapi/v1.json",
    Scalar = "/scalar/v1"
}));

// product endpoints registration 
app.MapProductEndpoints();

// user endpoints registration
//app.MapUserEndpoints();

app.Run();
