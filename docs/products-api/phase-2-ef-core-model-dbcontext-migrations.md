# Products API - Phase 2: EF Core Model, DbContext, and Migrations

This phase creates the Product entity, DbContext, request DTOs, FluentValidation validators, and database table.

Start from the project created in Phase 1:

```bash
cd ProductMinimalApi
```

## 1. Create ProductStatus enum and Product class

Create folder:

```bash
mkdir Models
```

Create `Models/ProductStatus.cs`:

```csharp
namespace ProductMinimalApi.Models;

public enum ProductStatus
{
    Draft = 1,
    Active = 2,
    Inactive = 3,
    Discontinued = 4
}
```

This enum shows valid product states.

EF Core can map enums to the database. By default, enums are stored as numbers. In this project, we will map the enum as text so the database stores values like `Active` and `Inactive`.

Create `Models/Product.cs`:

```csharp
namespace ProductMinimalApi.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stocks { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

This class becomes the `Products` table.

## 2. Create DateTime extension

Create folder:

```bash
mkdir Extensions
```

Create `Extensions/DateTimeExtensions.cs`:

```csharp
namespace ProductMinimalApi.Extensions;

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

This extension makes sure a `DateTime` value has UTC kind before it is saved or read.

## 3. Create DbContext class

Create folder:

```bash
mkdir Data
```

Create `Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using ProductMinimalApi.Extensions;
using ProductMinimalApi.Models;

namespace ProductMinimalApi.Data;

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

            entity.Property(product => product.CreatedAt)
                .HasConversion(
                    dateTime => dateTime.MakeItUTC(),
                    dateTime => dateTime.MakeItUTC())
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        });
    }
}
```

`AppDbContext` is the class EF Core uses to connect C# classes with database tables.

The `Status` conversion stores enum values as uppercase text in PostgreSQL:

```text
DRAFT
ACTIVE
INACTIVE
DISCONTINUED
```

The `CreatedAt` conversion makes sure saved and loaded `DateTime` values are UTC.

## 4. Create request DTO classes

DTO means Data Transfer Object. These classes represent data coming from the client.

Create folder:

```bash
mkdir DTOs
```

Create `DTOs/CreateProductRequest.cs`:

```csharp
using ProductMinimalApi.Models;

namespace ProductMinimalApi.DTOs;

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
using ProductMinimalApi.Models;

namespace ProductMinimalApi.DTOs;

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Stocks { get; set; }
    public ProductStatus? Status { get; set; }
}
```

`CreateProductRequest` uses required values. `Status` is nullable here so FluentValidation can detect when the client does not send it.

`UpdateProductRequest` uses nullable values because PATCH can update only some fields.

## 5. Create FluentValidation validators

Create folder:

```bash
mkdir Validators
```

Create `Validators/CreateProductRequestValidator.cs`:

```csharp
using FluentValidation;
using ProductMinimalApi.DTOs;
using ProductMinimalApi.Models;

namespace ProductMinimalApi.Validators;

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
using ProductMinimalApi.DTOs;
using ProductMinimalApi.Models;

namespace ProductMinimalApi.Validators;

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

Validators keep request validation rules outside `Program.cs`.

This makes the API code cleaner and makes validation easier to test later.

## 6. Register DbContext in Program.cs

At the top of `Program.cs`, add:

```csharp
using Microsoft.EntityFrameworkCore;
using ProductMinimalApi.Data;
```

After this line:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

Add:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

one HTTP request = one AppDbContext instance

The project can now inject `AppDbContext` into Minimal API route handlers.

## 7. Create database using migrations

Install EF Core CLI if needed:

```bash
dotnet tool install --global dotnet-ef
```

Create migration:

```bash
dotnet ef migrations add InitialCreate
```

Update database:

```bash
dotnet ef database update
```

This creates the `Products` table in PostgreSQL.

If you already created the database before adding `ProductStatus`, create a new migration instead:

```bash
dotnet ef migrations add AddProductStatusAndUtcCreatedAt
dotnet ef database update
```

## Phase 2 checkpoint

Run:

```bash
dotnet build
```

Then check pgAdmin. You should see:

```text
product_api_db
  Schemas
    public
      Tables
        Products
```

The `Products` table should include:

- `Id`
- `Name`
- `Description`
- `Price`
- `Stocks`
- `Status`
- `CreatedAt`

Continue to Phase 3 to add API routes.
