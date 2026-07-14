# Database Project 01 - Console App with EF Core and PostgreSQL

Create a console application. No APIs. The `Main` method will run hardcoded CRUD operations for a `Product` table.

## 1. Create project

```bash
dotnet new console -n ProductConsoleDb
cd ProductConsoleDb
```

## 2. Add EF Core packages

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

## 3. Add PostgreSQL and pgAdmin with Docker Compose

Create `docker-compose.yml`:

```yaml
services:
  dotnet_class_postgres:
    image: postgres:16
    container_name: dotnet_class_product_postgres
    environment:
      POSTGRES_DB: product_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - dotnet_class_product_postgres_data:/var/lib/postgresql/data

  dotnet_class_pgadmin:
    image: dpage/pgadmin4
    container_name: dotnet_class_product_pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@dotnetclass.com
      PGADMIN_DEFAULT_PASSWORD: admin
    ports:
      - "5050:80"
    depends_on:
      - dotnet_class_postgres

volumes:
  dotnet_class_product_postgres_data:
```

Start PostgreSQL and pgAdmin:

```bash
docker compose up -d
```

Open pgAdmin:

- URL: `http://localhost:5050`
- Email: `admin@dotnetclass.com`
- Password: `admin`

Register PostgreSQL server in pgAdmin:

- Host name/address: `dotnet_class_postgres`
- Port: `5432`
- Maintenance database: `product_db`
- Username: `postgres`
- Password: `postgres`

Stop PostgreSQL and pgAdmin:

```bash
docker compose down
```

## 4. Create Product model

Create `Product.cs`:

```csharp
namespace ProductConsoleDb;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stocks { get; set; }
}
```

## 5. Create DbContext

Create `AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;

namespace ProductConsoleDb;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=product_db;Username=postgres;Password=postgres");
    }

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
                .HasColumnType("numeric(18,2)");

            entity.Property(product => product.Stocks)
                .IsRequired();
        });
    }
}
```

## 6. Add CRUD operations in Main method

Update `Program.cs`:

```csharp
namespace ProductConsoleDb;

internal class Program
{
    private static void Main(string[] args)
    {
        using var db = new AppDbContext();

        db.Database.EnsureCreated();

        Console.WriteLine("Product CRUD demo started.");

        // CREATE
        var keyboard = new Product
        {
            Name = "Keyboard",
            Description = "Mechanical keyboard",
            Price = 75_000m,
            Stocks = 10
        };

        var mouse = new Product
        {
            Name = "Mouse",
            Description = "Wireless mouse",
            Price = 25_000m,
            Stocks = 20
        };

        db.Products.Add(keyboard);
        db.Products.Add(mouse);
        db.SaveChanges();

        Console.WriteLine("Products created.");

        // READ ALL
        var products = db.Products.ToList();

        Console.WriteLine("All products:");
        foreach (var product in products)
        {
            Console.WriteLine($"{product.Id} | {product.Name} | {product.Price} | {product.Stocks}");
        }

        // READ ONE
        var firstProduct = db.Products.FirstOrDefault();

        if (firstProduct is not null)
        {
            Console.WriteLine($"First product: {firstProduct.Name}");
        }

        // UPDATE
        if (firstProduct is not null)
        {
            firstProduct.Price = 80_000m;
            firstProduct.Stocks = 8;
            db.SaveChanges();

            Console.WriteLine("Product updated.");
        }

        // DELETE
        var productToDelete = db.Products.FirstOrDefault(product => product.Name == "Mouse");

        if (productToDelete is not null)
        {
            db.Products.Remove(productToDelete);
            db.SaveChanges();

            Console.WriteLine("Product deleted.");
        }

        // FINAL LIST
        var finalProducts = db.Products.ToList();

        Console.WriteLine("Final products:");
        foreach (var product in finalProducts)
        {
            Console.WriteLine($"{product.Id} | {product.Name} | {product.Price} | {product.Stocks}");
        }

        Console.WriteLine("Product CRUD demo finished.");
    }
}
```

## 7. Run project

```bash
dotnet run
```

## Project files

```text
ProductConsoleDb/
  docker-compose.yml
  ProductConsoleDb.csproj
  Program.cs
  Product.cs
  AppDbContext.cs
```

## Notes

- `Guid` in C# becomes `uuid` in PostgreSQL.
- `EnsureCreated()` creates the database tables for this beginner project.
- Later projects can use migrations instead of `EnsureCreated()`.
