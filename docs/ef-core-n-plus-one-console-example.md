# EF Core N+1 Problem - Console App Example

This example uses a console application with EF Core and PostgreSQL. It explains the N+1 problem, why it happens, and how to avoid it.

## What is the N+1 problem?

The N+1 problem happens when an application runs:

- 1 query to get a list of parent records
- then N extra queries to get child records for each parent

Example:

- 1 query gets all categories
- then 1 query per category gets products

If there are 10 categories, the app runs 11 queries.

If there are 100 categories, the app runs 101 queries.

That becomes slow because the app keeps going back to the database again and again.

## 1. Create project

```bash
dotnet new console -n EfCoreNPlusOneDemo
cd EfCoreNPlusOneDemo
```

## 2. Add packages

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

## 3. Add PostgreSQL with Docker Compose

Create `docker-compose.yml`:

```yaml
services:
  dotnet_class_postgres:
    image: postgres:16
    container_name: dotnet_class_nplusone_postgres
    environment:
      POSTGRES_DB: nplusone_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - dotnet_class_nplusone_postgres_data:/var/lib/postgresql/data

volumes:
  dotnet_class_nplusone_postgres_data:
```

Start database:

```bash
docker compose up -d
```

## 4. Create models

Create `Category.cs`:

```csharp
namespace EfCoreNPlusOneDemo;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = [];
}
```

Create `Product.cs`:

```csharp
namespace EfCoreNPlusOneDemo;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
}
```

## 5. Create DbContext

Create `AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EfCoreNPlusOneDemo;

public class AppDbContext : DbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql("Host=localhost;Port=5432;Database=nplusone_db;Username=postgres;Password=postgres")
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).IsRequired().HasMaxLength(100);
            entity.Property(product => product.Price).HasColumnType("numeric(18,2)");

            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId);
        });
    }
}
```

`LogTo(Console.WriteLine, LogLevel.Information)` helps us see SQL queries in the console.

## 6. Bad example: N+1 problem

Update `Program.cs`:

```csharp
namespace EfCoreNPlusOneDemo;

internal class Program
{
    private static void Main(string[] args)
    {
        using var db = new AppDbContext();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        SeedData(db);

        Console.WriteLine("Bad example: N+1 problem");

        var categories = db.Categories.ToList(); // Query 1

        foreach (var category in categories)
        {
            // Extra query per category
            var products = db.Products
                .Where(product => product.CategoryId == category.Id)
                .ToList();

            Console.WriteLine($"{category.Name}: {products.Count} products");
        }
    }

    private static void SeedData(AppDbContext db)
    {
        var electronics = new Category { Name = "Electronics" };
        var books = new Category { Name = "Books" };
        var foods = new Category { Name = "Foods" };

        db.Categories.AddRange(electronics, books, foods);

        db.Products.AddRange(
            new Product { Name = "Keyboard", Price = 75000m, Category = electronics },
            new Product { Name = "Mouse", Price = 25000m, Category = electronics },
            new Product { Name = "C# Book", Price = 15000m, Category = books },
            new Product { Name = "SQL Book", Price = 18000m, Category = books },
            new Product { Name = "Rice", Price = 5000m, Category = foods },
            new Product { Name = "Tea", Price = 3000m, Category = foods }
        );

        db.SaveChanges();
    }
}
```

This code runs:

- 1 query for categories
- 3 extra queries for products because there are 3 categories

Total: 4 queries.

With 100 categories, it becomes 101 queries.

## Why does it happen?

It happens because product data is loaded inside the loop.

```csharp
foreach (var category in categories)
{
    var products = db.Products
        .Where(product => product.CategoryId == category.Id)
        .ToList();
}
```

Every loop iteration sends another SQL query to the database.

This is easy to miss when the database has only a few records. It becomes a serious performance problem when the database grows.

## 7. Fix 1: Use Include

Use `Include` when you need parent records with their related child records.

```csharp
using Microsoft.EntityFrameworkCore;

var categories = db.Categories
    .Include(category => category.Products)
    .ToList();

foreach (var category in categories)
{
    Console.WriteLine($"{category.Name}: {category.Products.Count} products");
}
```

This loads categories and products together.

Use this when you need full `Category` and `Product` entity objects.

## 8. Fix 2: Use projection

Projection means selecting only the data you need.

```csharp
var categorySummaries = db.Categories
    .Select(category => new
    {
        CategoryName = category.Name,
        ProductCount = category.Products.Count
    })
    .ToList();

foreach (var item in categorySummaries)
{
    Console.WriteLine($"{item.CategoryName}: {item.ProductCount} products");
}
```

This is usually better for reports, lists, dashboards, and API responses.

Use projection when you do not need to update the entities.

## 9. Fix 3: Load all needed data first

Another option is to load related data in fewer queries and group in memory.

```csharp
var categories = db.Categories.ToList();
var products = db.Products.ToList();

foreach (var category in categories)
{
    var categoryProducts = products
        .Where(product => product.CategoryId == category.Id)
        .ToList();

    Console.WriteLine($"{category.Name}: {categoryProducts.Count} products");
}
```

This uses 2 queries:

- 1 query for categories
- 1 query for products

Use this carefully. If the products table is very large, loading all products can be expensive.

## 10. Best complete Program.cs

This version avoids N+1 using projection.

```csharp
namespace EfCoreNPlusOneDemo;

internal class Program
{
    private static void Main(string[] args)
    {
        using var db = new AppDbContext();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        SeedData(db);

        var categorySummaries = db.Categories
            .Select(category => new
            {
                CategoryName = category.Name,
                ProductCount = category.Products.Count,
                TotalValue = category.Products.Sum(product => product.Price)
            })
            .ToList();

        foreach (var item in categorySummaries)
        {
            Console.WriteLine($"{item.CategoryName}: {item.ProductCount} products, total value {item.TotalValue}");
        }
    }

    private static void SeedData(AppDbContext db)
    {
        var electronics = new Category { Name = "Electronics" };
        var books = new Category { Name = "Books" };
        var foods = new Category { Name = "Foods" };

        db.Categories.AddRange(electronics, books, foods);

        db.Products.AddRange(
            new Product { Name = "Keyboard", Price = 75000m, Category = electronics },
            new Product { Name = "Mouse", Price = 25000m, Category = electronics },
            new Product { Name = "C# Book", Price = 15000m, Category = books },
            new Product { Name = "SQL Book", Price = 18000m, Category = books },
            new Product { Name = "Rice", Price = 5000m, Category = foods },
            new Product { Name = "Tea", Price = 3000m, Category = foods }
        );

        db.SaveChanges();
    }
}
```

## How to avoid N+1

- Do not query related data inside loops.
- Use `Include` when you need full related entities.
- Use `Select` projection when you only need specific fields.
- Use `AsNoTracking()` for read-only queries.
- Check SQL logs during development.
- Think about how many database queries your code will create.

## Quick rule

Bad:

```csharp
foreach (var category in categories)
{
    var products = db.Products
        .Where(product => product.CategoryId == category.Id)
        .ToList();
}
```

Better:

```csharp
var categories = db.Categories
    .Include(category => category.Products)
    .ToList();
```

Usually best for read screens:

```csharp
var result = db.Categories
    .Select(category => new
    {
        category.Name,
        ProductCount = category.Products.Count
    })
    .ToList();
```
