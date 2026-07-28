## Fluent API vs Data Annotations in EF Core

Both methods configure how C# entities map to database tables.

## 1. Data Annotations

Data annotations are attributes placed directly on entity classes and properties.

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
}
```

### Advantages

* Simple and easy to learn
* Configuration is visible beside the property
* Good for small applications
* Useful for basic validation attributes such as `[Required]` and `[MaxLength]`

### Disadvantages

* Makes entity classes depend on EF/database-related attributes
* Can make classes cluttered
* Supports fewer configuration options
* Complex relationships and indexes can become difficult to configure

---

## 2. Fluent API

Fluent API configuration is written in `OnModelCreating` or separate configuration classes.

```csharp
using Microsoft.EntityFrameworkCore;

public class InventoryDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");

            entity.HasKey(product => product.Id);

            entity.Property(product => product.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(product => product.Sku)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(product => product.Sku)
                .IsUnique();

            entity.Property(product => product.Price)
                .HasPrecision(18, 2);
        });
    }
}
```

### Advantages

* Supports all EF Core configuration options
* Keeps database configuration away from entity classes
* Better for relationships, indexes and constraints
* Easier to manage in medium and large projects
* Fluent API overrides conventions and data annotations when they conflict. ([Microsoft Learn][1])

### Disadvantages

* More code
* Configuration is not beside the entity property
* Can make `OnModelCreating` very large unless configuration classes are used

---

## Main Differences

| Area                    | Data Annotations    | Fluent API                         |
| ----------------------- | ------------------- | ---------------------------------- |
| Location                | Inside entity class | `DbContext` or configuration class |
| Complexity              | Simple              | More detailed                      |
| Configuration support   | Limited             | Full EF Core support               |
| Entity cleanliness      | Adds attributes     | Keeps entities cleaner             |
| Relationships           | Basic relationships | Complex relationships              |
| Indexes and constraints | Limited             | Better control                     |
| Priority                | Lower               | Highest                            |

EF Core uses this priority order:

```text
Fluent API
    ↓
Data Annotations
    ↓
EF Core Conventions
```

For example, this annotation sets a maximum length of 100:

```csharp
[MaxLength(100)]
public string Name { get; set; } = string.Empty;
```

But this Fluent API configuration sets it to 150:

```csharp
entity.Property(product => product.Name)
    .HasMaxLength(150);
```

EF Core will use `150` because Fluent API has higher priority. ([Microsoft Learn][1])

---

## Separate Fluent API Configuration Class

For larger projects, avoid putting everything inside `OnModelCreating`.

### ProductConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(product => product.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(product => product.Sku)
            .IsUnique();

        builder.Property(product => product.Price)
            .HasColumnName("price")
            .HasPrecision(18, 2);
    }
}
```

### InventoryDbContext.cs

```csharp
public sealed class InventoryDbContext(
    DbContextOptions<InventoryDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InventoryDbContext).Assembly);
    }
}
```

This is usually the cleanest approach for production projects.

---

## Practical Recommendation

For your beginner course:

* Start with **Data Annotations** to explain simple rules.
* Then show **Fluent API** for table names, precision, indexes and relationships.
* For the inventory project, use **Fluent API configuration classes**.

A reasonable mixed approach is:

```csharp
public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
}
```

Then use Fluent API for database-specific configuration:

```csharp
builder.ToTable("products");

builder.HasIndex(product => product.Sku)
    .IsUnique();

builder.Property(product => product.Price)
    .HasPrecision(18, 2);
```

However, for a clean domain model, use Fluent API for all database mappings and handle API request validation separately. EF Core supports conventions, annotations and Fluent API together, but Fluent API is the most powerful configuration method. ([Microsoft Learn][1])

[1]: https://learn.microsoft.com/en-us/ef/core/modeling/?utm_source=chatgpt.com "Creating and Configuring a Model - EF Core"
