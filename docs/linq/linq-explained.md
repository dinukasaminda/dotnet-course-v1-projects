# .NET LINQ Class Explained

## 1. LINQ Definition

LINQ means Language Integrated Query.

It lets C# query collections, databases, XML, JSON-like objects, and other data sources.

Common methods:

```csharp
Where()
Select()
OrderBy()
GroupBy()
FirstOrDefault()
SingleOrDefault()
Any()
All()
Count()
Sum()
ToList()
ToArray()
```

## 2. Create Project

```bash
mkdir LinqExamples
cd LinqExamples
dotnet new console
```

## 3. Install Packages

EF Core is needed for `IQueryable<T>` with a real database.

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

## 4. Project File

File: `LinqExamples.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.10">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
  </ItemGroup>
</Project>
```

## 5. PostgreSQL Docker

File: `docker-compose.yml`

```yaml
services:
  linq_example_postgres:
    image: postgres:16
    container_name: linq_example_product_postgres
    environment:
      POSTGRES_DB: product_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - linq_example_product_postgres_data:/var/lib/postgresql/data

  linq_example_pgadmin:
    image: dpage/pgadmin4
    container_name: linq_example_product_pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@dotnetclass.com
      PGADMIN_DEFAULT_PASSWORD: admin
    ports:
      - "5050:80"
    depends_on:
      - linq_example_postgres

volumes:
  linq_example_product_postgres_data:
```

Start database:

```bash
docker compose up -d
```

## 6. Create Model

File: `model.cs`

```csharp
namespace App.Models;

public enum OrderStatus
{
    Pending,
    Approved,
    Rejected,
    Canceled
}

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; }

    public static List<Order> SampleDataList()
    {
        return new()
        {
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-001",
                CustomerName = "Kamal",
                TotalAmount = 5000m,
                IsPaid = true,
                Status = OrderStatus.Approved,
                CreatedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-002",
                CustomerName = "Nimal",
                TotalAmount = 25000m,
                IsPaid = false,
                Status = OrderStatus.Rejected,
                CreatedAt = new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-003",
                CustomerName = "Sunil",
                TotalAmount = 15000m,
                IsPaid = true,
                Status = OrderStatus.Approved,
                CreatedAt = new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc)
            }
        };
    }
}
```

## 7. DateTime Extension

Extension method means adding a method to an existing type without changing that type.

It makes code readable and fluent.

File: `Program.cs`

```csharp
public static class DatetimeExtensionMakeUTC
{
    public static DateTime MakeItUTC(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            return dateTime;
        }

        return dateTime.ToUniversalTime();
    }
}
```

## 8. Create DbContext

File: `OrderContext/OrderDbContext.cs`

```csharp
using App.Application;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.OrderContext;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=product_db;Username=postgres;Password=postgres");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Id)
                .HasColumnType("uuid");

            entity.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(45);

            entity.Property(o => o.CustomerName)
                .HasMaxLength(100);

            entity.Property(o => o.IsPaid)
                .HasDefaultValue(false);

            entity.Property(o => o.Status)
                .HasConversion(
                    status => status.ToString().ToUpper(),
                    value => Enum.Parse<OrderStatus>(value));

            entity.Property(o => o.TotalAmount)
                .HasDefaultValue(0);

            entity.Property(o => o.CreatedAt)
                .HasConversion(
                    date => date.MakeItUTC(),
                    date => date.MakeItUTC())
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(o => o.OrderNumber)
                .IsUnique();
        });
    }

    public void CreateSampleData()
    {
        var dataList = Order.SampleDataList();

        foreach (Order order in dataList)
        {
            Orders.Add(order);
        }

        SaveChanges();
    }
}
```

## 9. Program Imports

File: `Program.cs`

```csharp
using System.Linq.Expressions;
using App.Models;
using App.OrderContext;

namespace App.Application;
```

## 10. IEnumerable LINQ

Use when data is already in memory.

Examples:

```csharp
List<T>
Array
HashSet<T>
Queue<T>
Stack<T>
```

Good for:

```text
filtering in-memory lists
transforming objects
calculating totals
grouping local data
simple collection operations
```

Important: if 100,000 database rows are already loaded into a `List<T>`, then `IEnumerable<T>` filters inside the application memory.

File: `Program.cs`

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        List<Order> orders = Order.SampleDataList();

        Console.WriteLine("Paid Customers:");
        IEnumerable<Order> paidOrders = orders.Where(order => order.IsPaid);

        foreach (Order order in paidOrders)
        {
            Console.WriteLine(order.OrderNumber);
        }

        Console.WriteLine("Customer Names:");
        IEnumerable<string> customerNames = orders.Select(order => order.CustomerName);

        foreach (string name in customerNames)
        {
            Console.WriteLine(name);
        }

        Console.WriteLine("Order by amount:");
        IEnumerable<Order> ordered = orders.OrderByDescending(order => order.TotalAmount);

        foreach (Order order in ordered)
        {
            Console.WriteLine($"{order.OrderNumber} - {order.TotalAmount}");
        }

        Console.WriteLine("Total paid amount:");
        decimal totalPaidAmount = orders
            .Where(order => order.IsPaid)
            .Sum(order => order.TotalAmount);

        Console.WriteLine(totalPaidAmount);
    }
}
```

## 11. Query Syntax

Same idea, SQL-like syntax.

File: `Program.cs`, inside `Main`

```csharp
IEnumerable<Order> highValueOrdersUsingSyntax =
    from order in orders
    where order.TotalAmount > 10000m
    orderby order.TotalAmount descending
    select order;

foreach (Order order in highValueOrdersUsingSyntax)
{
    Console.WriteLine($"{order.OrderNumber} - {order.TotalAmount}");
}
```

## 12. Method Syntax

Most common modern style.

File: `Program.cs`, inside `Main`

```csharp
IEnumerable<Order> highValueOrdersUsingMethodSyntax = orders
    .Where(order => order.IsPaid)
    .Where(order => order.TotalAmount > 10000m)
    .OrderByDescending(order => order.TotalAmount);

foreach (Order order in highValueOrdersUsingMethodSyntax)
{
    Console.WriteLine($"{order.OrderNumber} - {order.TotalAmount}");
}
```

## 13. IQueryable LINQ

Use when query should run in database.

`IQueryable<T>` represents a query that can be translated and executed by a query provider.

Example: `DbSet<Order>` in EF Core implements `IQueryable<Order>`.

It does not load data immediately. It builds a query.

Query runs when calling:

```csharp
ToList()
FirstOrDefault()
SingleOrDefault()
Count()
Any()
Sum()
```

Use for:

```text
Database
Entity Framework Core
Remote query provider
OData
Search provider
```

Main benefit: LINQ can become SQL or another query language.

File: `Program.cs`, inside `Main`

```csharp
var dbCtx = new OrderDbContext();

dbCtx.Database.EnsureDeleted();
dbCtx.Database.EnsureCreated();
dbCtx.CreateSampleData();

IQueryable<Order> query = dbCtx.Orders
    .Where(order => order.IsPaid)
    .OrderByDescending(order => order.TotalAmount);

List<Order> result = query.ToList();

foreach (Order order in result)
{
    Console.WriteLine($"{order.Id} {order.OrderNumber} {order.TotalAmount} {order.Status}");
}
```

## 14. Extension Method Usage

File: `Program.cs`, inside `Main`

```csharp
DateTime dateTime = DateTime.Now;

Console.WriteLine(dateTime);
Console.WriteLine(dateTime.Kind);
Console.WriteLine(dateTime.MakeItUTC());
Console.WriteLine(dateTime.MakeItUTC().Kind);
```

## 15. Deferred Execution

Query is created first. It runs later.

Enumeration happens with:

```csharp
foreach
ToList()
ToArray()
Count()
FirstOrDefault()
Any()
Sum()
```

Useful for:

```text
query composition
better performance
lazy evaluation
database query building
avoiding unnecessary work
```

File: `Program.cs`, inside `Main`

```csharp
IEnumerable<Order> myOrders = orders.Where(order => order.IsPaid);

foreach (Order order in myOrders)
{
    Console.WriteLine(order.OrderNumber);
}
```

Build query step by step:

```csharp
IEnumerable<Order> myQuery = orders.Where(order => order.IsPaid);
myQuery = myQuery.Where(order => order.TotalAmount > 20000m);
myQuery = myQuery.Take(20);

List<Order> items = myQuery.ToList();
```

Changed data example:

```csharp
List<int> values = new() { 1, 2, 3, 4 };

IEnumerable<int> valueQuery = values.Where(value => value > 10);

values.Add(12);
values.Add(50);

foreach (int value in valueQuery)
{
    Console.WriteLine("Result Item: " + value);
}
```

## 16. Immediate Execution

Methods like `ToList`, `Count`, `Any`, `Sum` run now.

Use when you need final result now:

```text
need a list
need count
need sum
need first item
need API response data
need to avoid multiple enumeration
```

Common immediate methods:

```csharp
ToList()
ToArray()
Count()
Any()
All()
First()
FirstOrDefault()
Single()
SingleOrDefault()
Sum()
Average()
Max()
Min()
```

File: `Program.cs`, inside `Main`

```csharp
List<Order> paidList = orders
    .Where(order => order.IsPaid)
    .ToList();

int orderCount = orders
    .Where(order => order.TotalAmount > 1000)
    .Count();

bool hasPaidOrders = orders.Any(order => order.IsPaid);

decimal totalAmount = orders.Sum(order => order.TotalAmount);

Console.WriteLine(orderCount);
Console.WriteLine(hasPaidOrders);
Console.WriteLine(totalAmount);
```

## 17. Expression Tree Definition

Expression tree means code as data.

Normal delegate:

```csharp
Func<Order, bool> filter = order => order.IsPaid;
```

Expression tree:

```csharp
Expression<Func<Order, bool>> filter = order => order.IsPaid;
```

Namespace:

```csharp
using System.Linq.Expressions;
```

Use `Func` for:

```text
in-memory execution
callbacks
calculators
business rules that run in C#
```

Use expression trees for:

```text
database query filters
dynamic query building
specification pattern
ORM translation
```

## 18. Expression Tree Method

Expression tree is code stored as data.

File: `Program.cs`, inside `Program` class, outside `Main`

```csharp
public static Expression<Func<Order, bool>> IsPaidFilter()
{
    return order => order.IsPaid;
}
```

## 19. Expression Tree With EF Core

EF Core can translate this expression to SQL.

File: `Program.cs`, inside `Main`

```csharp
List<Order> myPaidOrders = dbCtx.Orders
    .Where(IsPaidFilter())
    .ToList();

foreach (Order order in myPaidOrders)
{
    Console.WriteLine("Result Item: " + order.Id);
}
```

## 20. Compile Expression Tree

Compile expression when running against in-memory list.

File: `Program.cs`, inside `Main`

```csharp
Func<Order, bool> myFilter = IsPaidFilter().Compile();

IEnumerable<Order> myPaidOrdersInList = orders.Where(myFilter);

foreach (Order order in myPaidOrdersInList)
{
    Console.WriteLine("Result Item: " + order.Id);
}
```

## 21. Run Project

```bash
dotnet run
```
