# Products API V2 - Layered Architecture / N-Tier Architecture

This guide converts `ProductApiv1` into `ProductApiv2` using layered architecture.

V1 style:

```text
Program.cs -> EF Core DbContext -> Database
```

V2 layered style:

```text
Minimal API endpoint layer -> Service -> EF Core DbContext -> Database
```

The goal is to keep the same product CRUD requirements and endpoints, but move logic out of `Program.cs`.

## Why convert to layered architecture?

In `ProductApiv1`, route handlers, validation calls, business logic, and EF Core queries live close together in `Program.cs`.

That is okay for small demos, but the file grows quickly.

Layered architecture helps because:

- Minimal API endpoints handle HTTP only
- services handle business logic
- `DbContext` handles database access
- validation stays in validator classes
- code is easier to test
- code is easier to change later
- large projects stay organized



## V2 project structure

```text
ProductApiv2/
  appsettings.json
  Program.cs
  Data/
    AppDbContext.cs
  DTOs/
    CreateProductRequest.cs
    UpdateProductRequest.cs
  Endpoints/
    ProductEndpoints.cs
  Extensions/
    DateTimeExtensions.cs
  Models/
    Product.cs
    ProductStatus.cs
  Services/
    IProductService.cs
    ProductService.cs
  Validators/
    CreateProductRequestValidator.cs
    UpdateProductRequestValidator.cs
```



## 1. Create ProductApiv2 project

```bash
dotnet new web -n ProductApiv2
cd ProductApiv2
```

Add packages:

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Scalar.AspNetCore
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```



## 2. Copy reusable classes from ProductApiv1

From `ProductApiv1`, reuse these folders:

```text
Models/
DTOs/
Extensions/
Validators/
Data/
```

In V2, keep them as separate folders.

The namespace should match the new project:

```csharp
namespace ProductApiv2.Models;
namespace ProductApiv2.DTOs;
namespace ProductApiv2.Data;
namespace ProductApiv2.Endpoints;
namespace ProductApiv2.Services;
namespace ProductApiv2.Validators;
namespace ProductApiv2.Extensions;
```



## 3. Product model

Create `Models/Product.cs`:

```csharp
namespace ProductApiv2.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stocks { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
```

Create `Models/ProductStatus.cs`:

```csharp
namespace ProductApiv2.Models;

public enum ProductStatus
{
    Draft = 1,
    Active = 2,
    Inactive = 3,
    Discontinued = 4
}
```



## 4. DTO classes

Create `DTOs/CreateProductRequest.cs`:

```csharp
using ProductApiv2.Models;

namespace ProductApiv2.DTOs;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stocks { get; set; }
    public ProductStatus? Status { get; set; }
}
```

Create `DTOs/UpdateProductRequest.cs`:

```csharp
using ProductApiv2.Models;

namespace ProductApiv2.DTOs;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Stocks { get; set; }
    public ProductStatus? Status { get; set; }
}
```

DTOs are used for request data. They are not database tables.

## 5. DateTime extension

Create `Extensions/DateTimeExtensions.cs`:

```csharp
namespace ProductApiv2.Extensions;

public static class DateTimeExtensions
{
    public static DateTime MakeItUTC(this DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }
}
```

Use this before saving dates to PostgreSQL.

## 6. DbContext layer

Create `Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using ProductApiv2.Extensions;
using ProductApiv2.Models;

namespace ProductApiv2.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);

            entity.Property(product => product.Id)
                .HasColumnType("uuid");

            entity.Property(product => product.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(product => product.Description)
                .HasMaxLength(500);

            entity.Property(product => product.Price)
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            entity.Property(product => product.Stocks)
                .IsRequired();

            entity.Property(product => product.Status)
                .HasConversion(
                    status => status.ToString().ToUpper(),
                    value => Enum.Parse<ProductStatus>(value, ignoreCase: true))
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(product => product.CreatedDate)
                .HasConversion(
                    dateTime => dateTime.MakeItUTC(),
                    dateTime => dateTime.MakeItUTC())
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        });
    }
}
```

This is the EF Core database layer.

It knows about PostgreSQL mappings, enum conversion, and date conversion.

## 7. FluentValidation validators

Create `Validators/CreateProductRequestValidator.cs`:

```csharp
using FluentValidation;
using ProductApiv2.DTOs;
using ProductApiv2.Models;

namespace ProductApiv2.Validators;

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
```

Create `Validators/UpdateProductRequestValidator.cs`:

```csharp
using FluentValidation;
using ProductApiv2.DTOs;
using ProductApiv2.Models;

namespace ProductApiv2.Validators;

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
```

Validators keep validation logic away from the endpoint layer and service.

## 8. Service contract

Create `Services/IProductService.cs`:

```csharp
using ProductApiv2.DTOs;
using ProductApiv2.Models;

namespace ProductApiv2.Services;

public interface IProductService
{
    Task<List<Product>> GetProductsAsync(
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        ProductStatus? status,
        bool? inStockOnly);

    Task<Product?> GetProductByIdAsync(Guid id);
    Task<Product> CreateProductAsync(CreateProductRequest request);
    Task<Product?> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(Guid id);
    Task<List<Product>> GetLowStockProductsUsingIEnumerableAsync();
}
```

The endpoint layer depends on this interface, not directly on `AppDbContext`.

That is the main layered architecture idea.

## 9. Service implementation

Create `Services/ProductService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using ProductApiv2.Data;
using ProductApiv2.DTOs;
using ProductApiv2.Extensions;
using ProductApiv2.Models;

namespace ProductApiv2.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetProductsAsync(
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        ProductStatus? status,
        bool? inStockOnly)
    {
        IQueryable<Product> query = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
                EF.Functions.ILike(product.Name, $"%{search}%") ||
                EF.Functions.ILike(product.Description, $"%{search}%"));
        }

        if (minPrice is not null)
        {
            query = query.Where(product => product.Price >= minPrice);
        }

        if (maxPrice is not null)
        {
            query = query.Where(product => product.Price <= maxPrice);
        }

        if (status is not null)
        {
            query = query.Where(product => product.Status == status);
        }

        if (inStockOnly == true)
        {
            query = query.Where(product => product.Stocks > 0);
        }

        return await query
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Stocks = request.Stocks,
            Status = request.Status!.Value,
            CreatedDate = DateTime.UtcNow.MakeItUTC()
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return product;
    }

    public async Task<Product?> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product is null)
        {
            return null;
        }

        if (request.Name is not null)
        {
            product.Name = request.Name.Trim();
        }

        if (request.Description is not null)
        {
            product.Description = request.Description.Trim();
        }

        if (request.Price is not null)
        {
            product.Price = request.Price.Value;
        }

        if (request.Stocks is not null)
        {
            product.Stocks = request.Stocks.Value;
        }

        if (request.Status is not null)
        {
            product.Status = request.Status.Value;
        }

        await _db.SaveChangesAsync();

        return product;
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product is null)
        {
            return false;
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<List<Product>> GetLowStockProductsUsingIEnumerableAsync()
    {
        IEnumerable<Product> products = await _db.Products
            .AsNoTracking()
            .ToListAsync();

        return products
            .Where(product => product.Stocks < 5)
            .ToList();
    }
}
```

The service contains business logic and EF Core queries.

The endpoint layer will call service methods instead of querying the database directly.

## 10. Minimal API endpoint layer

Create `Endpoints/ProductEndpoints.cs`:

```csharp
using FluentValidation;
using FluentValidation.Results;
using ProductApiv2.DTOs;
using ProductApiv2.Models;
using ProductApiv2.Services;

namespace ProductApiv2.Endpoints;

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
        group.MapGet("/examples/ienumerable", GetLowStockProductsUsingIEnumerable);
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

    private static async Task<IResult> GetLowStockProductsUsingIEnumerable(
        IProductService productService)
    {
        var products = await productService.GetLowStockProductsUsingIEnumerableAsync();

        return Results.Ok(products);
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
```

The endpoint layer is now thin.

It only handles:

- route parameters
- query parameters
- request body
- validation response
- HTTP response status

The endpoint layer does not know EF Core query details.

## 11. Program.cs

Update `Program.cs`:

```csharp
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductApiv2.Data;
using ProductApiv2.Endpoints;
using ProductApiv2.Services;
using ProductApiv2.Validators;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductService, ProductService>();

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

app.MapProductEndpoints();

app.Run();
```

Important service registration:

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

`ProductService` is scoped because it uses `AppDbContext`.

`AppDbContext` is also scoped by default when using:

```csharp
builder.Services.AddDbContext<AppDbContext>(...);
```

That means:

```text
one HTTP request -> one AppDbContext
```



## 12. appsettings.json

Use the same connection string from V1:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=product_api_db;Username=postgres;Password=postgres"
  }
}
```



## 13. Migrations

Create migration:

```bash
dotnet ef migrations add InitialCreate
```

Update database:

```bash
dotnet ef database update
```

If V2 uses the same database as V1, use a different database name if you want to avoid migration conflicts:

```text
Database=product_api_v2_db
```



## 14. Same operations as V1

V2 keeps the same product operations as V1, but uses the `/api/products` route group:


| Method   | Endpoint                                      | Purpose                             |
| -------- | --------------------------------------------- | ----------------------------------- |
| `GET`    | `/`                                           | App info                            |
| `GET`    | `/api/products`                               | List products                       |
| `GET`    | `/api/products?search=key`                    | Search products                     |
| `GET`    | `/api/products?minPrice=20000&maxPrice=80000` | Price filter                        |
| `GET`    | `/api/products?status=Active`                 | Status filter                       |
| `GET`    | `/api/products?inStockOnly=true`              | Stock filter                        |
| `GET`    | `/api/products/{id}`                          | Get product by id                   |
| `POST`   | `/api/products`                               | Create product                      |
| `PATCH`  | `/api/products/{id}`                          | Update product                      |
| `DELETE` | `/api/products/{id}`                          | Delete product                      |
| `GET`    | `/api/products/examples/ienumerable`          | Learning endpoint for `IEnumerable` |
| `GET`    | `/openapi/v1.json`                            | OpenAPI document                    |
| `GET`    | `/scalar/v1`                                  | Scalar API docs                     |




## 15. Test sample requests

Run:

```bash
dotnet run --urls http://localhost:5000
```

Create product:

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Keyboard","description":"Mechanical keyboard","price":75000,"stocks":10,"status":"Active"}'
```

List products:

```bash
curl http://localhost:5000/api/products
```

Filter by status:

```bash
curl "http://localhost:5000/api/products?status=Active"
```

Update product:

```bash
curl -X PATCH http://localhost:5000/api/products/PRODUCT_ID_HERE \
  -H "Content-Type: application/json" \
  -d '{"price":80000,"stocks":8,"status":"Inactive"}'
```

Delete product:

```bash
curl -X DELETE http://localhost:5000/api/products/PRODUCT_ID_HERE
```

Open Scalar:

```text
http://localhost:5000/scalar/v1
```



## 16. V1 vs V2 comparison


| Area            | ProductApiv1                       | ProductApiv2                             |
| --------------- | ---------------------------------- | ---------------------------------------- |
| Routing         | Minimal API routes in `Program.cs` | Minimal API routes in `ProductEndpoints` |
| Business logic  | Inside route handlers              | Inside `ProductService`                  |
| Database access | Route handler uses `AppDbContext`  | Service uses `AppDbContext`              |
| Validation      | Route handler calls validators     | Endpoint layer calls validators          |
| Program.cs size | Grows as endpoints grow            | Stays small                              |
| Testability     | Harder to isolate logic            | Service can be tested separately         |




## 17. Main benefit

Layered architecture separates responsibilities.

```text
Minimal API endpoint layer
  receives HTTP request
  returns HTTP response

Service
  contains business logic
  talks to DbContext

DbContext
  maps C# entities to database tables
  sends SQL to PostgreSQL

Database
  stores product data
```

When the project grows, this structure is easier to maintain than putting all code in `Program.cs`.