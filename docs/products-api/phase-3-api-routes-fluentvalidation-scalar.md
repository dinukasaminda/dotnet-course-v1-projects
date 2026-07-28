# Products API - Phase 3: API Routes, FluentValidation, Scalar, and API Concepts

This phase adds Product CRUD endpoints, FluentValidation validation, automatic OpenAPI mapping, Scalar API docs, and `IQueryable` / `IEnumerable` examples.

Start from the project completed in Phase 2:

```bash
cd ProductMinimalApi
```

## 1. Update Program.cs

Replace `Program.cs` with this:

```csharp
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using ProductMinimalApi.Data;
using ProductMinimalApi.DTOs;
using ProductMinimalApi.Extensions;
using ProductMinimalApi.Models;
using ProductMinimalApi.Validators;
using Scalar.AspNetCore;

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

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => Results.Ok(new
{
    App = "ProductMinimalApi",
    Message = "Product CRUD API using Minimal API, EF Core, FluentValidation, and Scalar.",
    OpenApi = "/openapi/v1.json",
    Scalar = "/scalar/v1"
}));

app.MapGet("/products", async (
    AppDbContext db,
    string? search,
    decimal? minPrice,
    decimal? maxPrice,
    ProductStatus? status,
    bool? inStockOnly) =>
{
    // IQueryable means the query is not executed yet.
    // EF Core keeps building SQL until ToListAsync(), FirstOrDefaultAsync(), CountAsync(), etc.
    IQueryable<Product> query = db.Products.AsNoTracking();

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

    // SQL runs here.
    var products = await query
        .OrderBy(product => product.Name)
        .ToListAsync();

    return Results.Ok(products);
});

app.MapGet("/products/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var product = await db.Products
        .AsNoTracking()
        .FirstOrDefaultAsync(product => product.Id == id);

    return product is null
        ? Results.NotFound()
        : Results.Ok(product);
});

app.MapPost("/products", async (
    CreateProductRequest request,
    IValidator<CreateProductRequest> validator,
    AppDbContext db) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(ToValidationErrors(validationResult));
    }

    var product = new Product
    {
        Name = request.Name.Trim(),
        Description = request.Description.Trim(),
        Price = request.Price,
        Stocks = request.Stocks,
        Status = request.Status!.Value,
        CreatedAt = DateTime.UtcNow.MakeItUTC()
    };

    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Created($"/products/{product.Id}", product);
});

app.MapPatch("/products/{id:guid}", async (
    Guid id,
    UpdateProductRequest request,
    IValidator<UpdateProductRequest> validator,
    AppDbContext db) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(ToValidationErrors(validationResult));
    }

    var product = await db.Products.FirstOrDefaultAsync(product => product.Id == id);

    if (product is null)
    {
        return Results.NotFound();
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

    await db.SaveChangesAsync();

    return Results.Ok(product);
});

app.MapDelete("/products/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var product = await db.Products.FirstOrDefaultAsync(product => product.Id == id);

    if (product is null)
    {
        return Results.NotFound();
    }

    db.Products.Remove(product);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapGet("/examples/ienumerable", async (AppDbContext db) =>
{
    // IEnumerable means data is already in memory.
    // This query loads all products from the database first.
    IEnumerable<Product> products = await db.Products
        .AsNoTracking()
        .ToListAsync();

    // This filter runs in C# memory, not in SQL.
    var lowStockProducts = products
        .Where(product => product.Stocks < 5)
        .ToList();

    return Results.Ok(lowStockProducts);
});

app.Run();

static Dictionary<string, string[]> ToValidationErrors(ValidationResult validationResult)
{
    return validationResult.Errors
        .GroupBy(error => error.PropertyName)
        .ToDictionary(
            group => group.Key,
            group => group.Select(error => error.ErrorMessage).ToArray());
}
```



## 2. How FluentValidation is used

This line registers all validators in the project:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
```

Then endpoints can request validators through dependency injection:

```csharp
IValidator<CreateProductRequest> validator
```

POST validation:

```csharp
var validationResult = await validator.ValidateAsync(request);

if (!validationResult.IsValid)
{
    return Results.ValidationProblem(ToValidationErrors(validationResult));
}
```

PATCH uses the same idea with `IValidator<UpdateProductRequest>`.

The important point: validation rules are not hardcoded in the endpoint. They live in the validator classes.

## 3. Status validation

`ProductStatus` is validated in the DTO validators.

Create request:

```csharp
RuleFor(request => request.Status)
    .NotNull()
    .WithMessage("Status is required.")
    .Must(status => status is not null &&
        Enum.IsDefined(typeof(ProductStatus), status.Value))
    .WithMessage("Status must be one of: Draft, Active, Inactive, Discontinued.");
```

Update request:

```csharp
RuleFor(request => request.Status)
    .Must(status => status is null ||
        Enum.IsDefined(typeof(ProductStatus), status.Value))
    .WithMessage("Status must be one of: Draft, Active, Inactive, Discontinued.");
```

For PATCH, `Status` can be missing. But if it is sent, it must be valid.

Allowed values:

```text
Draft
Active
Inactive
Discontinued
```



## 4. JSON enum behavior

This line makes API JSON use enum names:

```csharp
options.SerializerOptions.Converters.Add(
    new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
```

`allowIntegerValues: false` means clients should send `"Active"` instead of `2`.

If the client sends an unknown string like `"Deleted"`, JSON binding returns `400 Bad Request`.

If a value reaches FluentValidation, the validator checks it again before saving.

## 5. EF Core enum mapping

In `AppDbContext`, the enum is mapped as uppercase text:

```csharp
entity.Property(product => product.Status)
    .HasConversion(
        status => status.ToString().ToUpper(),
        value => Enum.Parse<ProductStatus>(value, ignoreCase: true))
    .HasMaxLength(30)
    .IsRequired();
```

PostgreSQL stores:

```text
DRAFT
ACTIVE
INACTIVE
DISCONTINUED
```

instead of:

```text
1
2
3
4
```



## 6. DateTime UTC kind before saving

Before adding a new product to the database, set `CreatedAt` with UTC kind:

```csharp
CreatedAt = DateTime.UtcNow.MakeItUTC()
```

In `AppDbContext`, `CreatedAt` is also mapped as:

```csharp
entity.Property(product => product.CreatedAt)
    .HasConversion(
        dateTime => dateTime.MakeItUTC(),
        dateTime => dateTime.MakeItUTC())
    .HasColumnType("timestamp with time zone")
    .IsRequired();
```

Simple rule:

Use UTC for dates saved to the database.

## 7. How automatic OpenAPI and Scalar work

These lines enable automatic OpenAPI generation:

```csharp
builder.Services.AddOpenApi();
app.MapOpenApi();
```

ASP.NET Core reads the Minimal API route mappings and automatically creates:

```text
/openapi/v1.json
```

This line enables Scalar UI:

```csharp
app.MapScalarApiReference();
```

Scalar reads the OpenAPI document and creates a browser API reference:

```text
/scalar/v1
```

No need to manually write a large OpenAPI JSON object.

## 8. IQueryable vs IEnumerable



### IQueryable

`IQueryable` is normally used with EF Core before data is loaded.

```csharp
IQueryable<Product> query = db.Products;

query = query.Where(product => product.Price >= 1000);
query = query.Where(product => product.Stocks > 0);

var products = await query.ToListAsync();
```

The database is called only when this line runs:

```csharp
await query.ToListAsync();
```

EF Core converts the full query into SQL.

Good for:

- database filtering
- database sorting
- paging
- searching



### IEnumerable

`IEnumerable` is normally used after data is already loaded into memory.

```csharp
IEnumerable<Product> products = await db.Products.ToListAsync();

var filteredProducts = products
    .Where(product => product.Price >= 1000)
    .ToList();
```

Here, `ToListAsync()` loads products first.

Then `Where()` runs in C# memory.

Good for:

- small in-memory lists
- already-loaded data
- simple C# filtering after database query is complete



### Main difference


| Type          | Where filtering happens | Best use            |
| ------------- | ----------------------- | ------------------- |
| `IQueryable`  | Database                | EF Core queries     |
| `IEnumerable` | C# memory               | Already-loaded data |


Simple rule:

Use `IQueryable` before loading data from the database.

Use `IEnumerable` after data is loaded.

## Phase 3 checkpoint

Run:

```bash
dotnet build
```

If the build works, continue to Phase 4 for testing and sample requests.