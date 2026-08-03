# Dependency Injection Before Layered Architecture

Before learning layered architecture, understand Dependency Injection.

DI means:

```text
Instead of a class creating its own dependencies,
ASP.NET Core creates and gives them to the class.
```

## Why learn DI first?

In layered architecture, the endpoint layer uses a service:

```text
ProductEndpoints -> IProductService -> ProductService -> AppDbContext
```

The endpoint layer does not manually create `ProductService`.

ASP.NET Core injects it automatically.

## Benefits of Dependency Injection

Dependency Injection gives important benefits in real applications.

### 1. Loose coupling

Without DI, a class creates its own dependency:

```csharp
var service = new ProductService();
```

That class is now tightly connected to `ProductService`.

With DI, code depends on an interface:

```csharp
IProductService productService
```

This means the endpoint only knows what the service can do, not how the service is built.

### 2. Better maintainability

When object creation is centralized in `Program.cs`, the project is easier to maintain.

Example:

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

If the implementation changes later, we update the registration in one place.

### 3. Better testability

DI makes code easier to test because we can replace real services with fake services.

Example:

```csharp
builder.Services.AddScoped<IProductService, FakeProductService>();
```

In tests, `FakeProductService` can return hardcoded products without connecting to a real database.

This makes unit tests:

- faster
- simpler
- independent from PostgreSQL
- easier to control

### 4. Easier to change implementations

If code depends on interfaces, changing implementation becomes easier.

Example:

```csharp
builder.Services.AddScoped<IProductRepository, PostgresProductRepository>();
```

Later, if the database changes from PostgreSQL to MySQL:

```csharp
builder.Services.AddScoped<IProductRepository, MySqlProductRepository>();
```

The endpoint layer and service layer do not need major changes because they depend on the interface:

```csharp
IProductRepository
```

This is one of the main reasons DI is used in layered architecture.

### 5. Lifetime management

DI controls how long objects live.

For example:

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

This means:

```text
one HTTP request = one ProductService instance
```

For EF Core:

```csharp
builder.Services.AddDbContext<AppDbContext>();
```

`AppDbContext` is scoped by default.

This avoids accidentally sharing one `DbContext` across many users.

## 1. Create simple project

```bash
dotnet new web -n DiExampleApp
cd DiExampleApp
```

## 2. Create service interface

Create `IGreetingService.cs`:

```csharp
namespace DiExampleApp;

public interface IGreetingService
{
    string GetMessage(string name);
}
```

An interface says what a service can do.

## 3. Create service class

Create `GreetingService.cs`:

```csharp
namespace DiExampleApp;

public class GreetingService : IGreetingService
{
    public string GetMessage(string name)
    {
        return $"Hello {name}, this message came from GreetingService.";
    }
}
```

This is the concrete class.

It implements the interface.

## 4. Register service in Program.cs

Update `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IGreetingService, GreetingService>();

var app = builder.Build();

app.MapGet("/", () => "DI Example App");

app.MapGet("/hello/{name}", (string name, IGreetingService greetingService) =>
{
    var message = greetingService.GetMessage(name);

    return Results.Ok(new
    {
        Message = message
    });
});

app.Run();
```

Important line:

```csharp
builder.Services.AddScoped<IGreetingService, GreetingService>();
```

This tells ASP.NET Core:

```text
When someone asks for IGreetingService,
create and give GreetingService.
```

## 5. Run app

```bash
dotnet run --urls http://localhost:5000
```

Open:

```text
http://localhost:5000/hello/Amal
```

Response:

```json
{
  "message": "Hello Amal, this message came from GreetingService."
}
```

## How injection happens

This endpoint asks for `IGreetingService`:

```csharp
app.MapGet("/hello/{name}", (string name, IGreetingService greetingService) =>
{
    return greetingService.GetMessage(name);
});
```

ASP.NET Core sees:

```text
Endpoint needs IGreetingService
```

Then it checks the DI container:

```text
IGreetingService -> GreetingService
```

Then it creates `GreetingService` and gives it to the endpoint.

## DI lifetime types

| Lifetime | Meaning | Example use |
| --- | --- | --- |
| Transient | New instance every time | small stateless helper |
| Scoped | One instance per HTTP request | service, DbContext |
| Singleton | One instance for whole app | app-wide cache/config |

## Lifetime examples with counter services

Counters make DI lifetimes easy to understand.

## 1. Create counter services

Create `Counters.cs`:

```csharp
namespace DiExampleApp;

public interface ITransientCounter
{
    int Increment();
}

public interface IScopedCounter
{
    int Increment();
}

public interface ISingletonCounter
{
    int Increment();
}

public class TransientCounter : ITransientCounter
{
    private int _count;

    public int Increment()
    {
        _count++;
        return _count;
    }
}

public class ScopedCounter : IScopedCounter
{
    private int _count;

    public int Increment()
    {
        _count++;
        return _count;
    }
}

public class SingletonCounter : ISingletonCounter
{
    private int _count;

    public int Increment()
    {
        _count++;
        return _count;
    }
}
```

Each class has its own private `_count`.

The lifetime decides how long that object is reused.

## 2. Register counters

Update `Program.cs`:

```csharp
builder.Services.AddTransient<ITransientCounter, TransientCounter>();
builder.Services.AddScoped<IScopedCounter, ScopedCounter>();
builder.Services.AddSingleton<ISingletonCounter, SingletonCounter>();
```

Meaning:

```text
Transient -> new object every time it is requested
Scoped    -> one object per HTTP request
Singleton -> one object for the full application lifetime
```

## 3. Add test endpoints

Add these endpoints to `Program.cs`:

```csharp
app.MapGet("/counter/transient", (
    ITransientCounter first,
    ITransientCounter second) =>
{
    return Results.Ok(new
    {
        First = first.Increment(),
        Second = second.Increment()
    });
});

app.MapGet("/counter/scoped", (
    IScopedCounter first,
    IScopedCounter second) =>
{
    return Results.Ok(new
    {
        First = first.Increment(),
        Second = second.Increment()
    });
});

app.MapGet("/counter/singleton", (
    ISingletonCounter first,
    ISingletonCounter second) =>
{
    return Results.Ok(new
    {
        First = first.Increment(),
        Second = second.Increment()
    });
});
```

## 4. Expected behavior

Open:

```text
http://localhost:5000/counter/transient
```

Result is usually:

```json
{
  "first": 1,
  "second": 1
}
```

Why? `first` and `second` are two different transient objects.

Open:

```text
http://localhost:5000/counter/scoped
```

Result for one request:

```json
{
  "first": 1,
  "second": 2
}
```

Why? `first` and `second` share the same scoped object inside one HTTP request.

Refresh the page and it starts again:

```json
{
  "first": 1,
  "second": 2
}
```

Open:

```text
http://localhost:5000/counter/singleton
```

First request:

```json
{
  "first": 1,
  "second": 2
}
```

Refresh again:

```json
{
  "first": 3,
  "second": 4
}
```

Why? Singleton keeps the same object for the whole app lifetime.

## 5. Lifetime comparison

| Lifetime | Same request | Next request | Main idea |
| --- | --- | --- | --- |
| Transient | New every injection | New again | Does not remember state |
| Scoped | Same object | New object | Remembers state only during one request |
| Singleton | Same object | Same object | Remembers state while app is running |

## Scoped example

```csharp
builder.Services.AddScoped<IGreetingService, GreetingService>();
```

Scoped means:

```text
one HTTP request = one service instance
```

This is also how EF Core `DbContext` usually works:

```csharp
builder.Services.AddDbContext<AppDbContext>();
```

`AddDbContext` registers `AppDbContext` as scoped by default.

## How this connects to Product API layered architecture

In Product API V2, we register:

```csharp
builder.Services.AddScoped<IProductService, ProductService>();
```

Then the Minimal API endpoint can ask for `IProductService`:

```csharp
app.MapGet("/products", async (IProductService productService) =>
{
    var products = await productService.GetProductsAsync();
    return Results.Ok(products);
});
```

ASP.NET Core creates `ProductService` and gives it to the endpoint.

`ProductService` can also ask for `AppDbContext`:

```csharp
public ProductService(AppDbContext db)
{
    _db = db;
}
```

ASP.NET Core creates the full chain:

```text
ProductEndpoints
  needs IProductService

ProductService
  needs AppDbContext

AppDbContext
  connects to PostgreSQL
```

## Simple rule

Register dependencies in `Program.cs`.

Ask for dependencies in constructors or endpoint parameters.

Do not manually create services with `new` inside endpoints.

Avoid:

```csharp
var service = new ProductService();
```

Prefer:

```csharp
app.MapGet("/products", async (IProductService productService) =>
{
    var products = await productService.GetProductsAsync();
    return Results.Ok(products);
});
```

That is Dependency Injection.
