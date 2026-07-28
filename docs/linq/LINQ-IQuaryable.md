# LINQ in C# with Examples

LINQ means **Language Integrated Query**.

It allows you to query collections, databases, XML, JSON-like objects, and other data sources using C# syntax.

Common LINQ methods:

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

We will use this sample model in the examples:

```csharp
public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Sample data:

```csharp
List<Order> orders = new()
{
    new Order
    {
        Id = Guid.NewGuid(),
        OrderNumber = "ORD-001",
        CustomerName = "Kamal",
        TotalAmount = 5000m,
        IsPaid = true,
        CreatedAt = new DateTime(2026, 7, 1)
    },
    new Order
    {
        Id = Guid.NewGuid(),
        OrderNumber = "ORD-002",
        CustomerName = "Nimal",
        TotalAmount = 25000m,
        IsPaid = false,
        CreatedAt = new DateTime(2026, 7, 2)
    },
    new Order
    {
        Id = Guid.NewGuid(),
        OrderNumber = "ORD-003",
        CustomerName = "Sunil",
        TotalAmount = 15000m,
        IsPaid = true,
        CreatedAt = new DateTime(2026, 7, 3)
    }
};
```

---

# 1. IEnumerable LINQ

## What

`IEnumerable<T>` LINQ works with **in-memory collections**.

Examples:

```csharp
List<T>
Array
HashSet<T>
Queue<T>
Stack<T>
```

When you use LINQ on a `List<T>`, `Array`, or other in-memory collection, you are usually using `IEnumerable<T>` LINQ.

Example:

```csharp
IEnumerable<Order> paidOrders = orders
    .Where(order => order.IsPaid);
```

---

## Why

Use `IEnumerable<T>` LINQ when data is already loaded into application memory.

Good for:

```text
Filtering in-memory lists
Transforming objects
Calculating totals
Grouping local data
Simple collection operations
```

---

## Example: filter paid orders

```csharp
IEnumerable<Order> paidOrders = orders
    .Where(order => order.IsPaid);

foreach (Order order in paidOrders)
{
    Console.WriteLine(order.OrderNumber);
}
```

Output:

```text
ORD-001
ORD-003
```

---

## Example: select only customer names

```csharp
IEnumerable<string> customerNames = orders
    .Select(order => order.CustomerName);

foreach (string name in customerNames)
{
    Console.WriteLine(name);
}
```

Output:

```text
Kamal
Nimal
Sunil
```

---

## Example: order by amount

```csharp
IEnumerable<Order> orderedByAmount = orders
    .OrderByDescending(order => order.TotalAmount);

foreach (Order order in orderedByAmount)
{
    Console.WriteLine($"{order.OrderNumber} - {order.TotalAmount}");
}
```

Output:

```text
ORD-002 - 25000
ORD-003 - 15000
ORD-001 - 5000
```

---

## Example: calculate total paid amount

```csharp
decimal totalPaidAmount = orders
    .Where(order => order.IsPaid)
    .Sum(order => order.TotalAmount);

Console.WriteLine(totalPaidAmount);
```

Output:

```text
20000
```

---

## Query syntax

LINQ also supports query syntax.

```csharp
IEnumerable<Order> highValueOrders =
    from order in orders
    where order.TotalAmount > 10000m
    orderby order.TotalAmount descending
    select order;
```

This is equivalent to:

```csharp
IEnumerable<Order> highValueOrders = orders
    .Where(order => order.TotalAmount > 10000m)
    .OrderByDescending(order => order.TotalAmount);
```

Most modern .NET developers commonly use method syntax.

---

## Senior-level note

`IEnumerable<T>` works in memory.

This means if you already loaded 100,000 records from the database and then filter using `IEnumerable<T>`, filtering happens inside your application.

Example:

```csharp
List<Order> allOrders = dbContext.Orders.ToList();

List<Order> paidOrders = allOrders
    .Where(order => order.IsPaid)
    .ToList();
```

Problem:

```text
All orders are loaded first.
Filtering happens in memory.
```

Better with database query:

```csharp
List<Order> paidOrders = dbContext.Orders
    .Where(order => order.IsPaid)
    .ToList();
```

Here, filtering can happen in the database.

That leads us to `IQueryable<T>`.

---

# 2. IQueryable

## What

`IQueryable<T>` represents a query that can be translated and executed by a query provider.

Common real-world example:

```csharp
DbSet<Order>
```

in Entity Framework Core implements `IQueryable<Order>`.

Example:

```csharp
IQueryable<Order> query = dbContext.Orders
    .Where(order => order.IsPaid);
```

This does not immediately load data.

It builds a query.

The query runs when you call:

```csharp
ToList()
FirstOrDefault()
SingleOrDefault()
Count()
Any()
Sum()
```

---

## Why

Use `IQueryable<T>` when querying external data sources like:

```text
Database
Entity Framework Core
Remote query provider
OData
Search provider
```

The main benefit is that LINQ can be translated into SQL or another query language.

---

## Example with EF Core style query

```csharp
IQueryable<Order> query = dbContext.Orders
    .Where(order => order.IsPaid)
    .OrderByDescending(order => order.CreatedAt);
```

At this point:

```text
No database call yet.
Only query is built.
```

Database call happens here:

```csharp
List<Order> paidOrders = query.ToList();
```

---

## Example: better database filtering

Good:

```csharp
List<Order> highValuePaidOrders = dbContext.Orders
    .Where(order => order.IsPaid)
    .Where(order => order.TotalAmount > 10000m)
    .OrderByDescending(order => order.TotalAmount)
    .ToList();
```

This can become SQL similar to:

```sql
SELECT *
FROM Orders
WHERE IsPaid = 1
AND TotalAmount > 10000
ORDER BY TotalAmount DESC
```

---

## Bad example

```csharp
List<Order> allOrders = dbContext.Orders.ToList();

List<Order> highValuePaidOrders = allOrders
    .Where(order => order.IsPaid)
    .Where(order => order.TotalAmount > 10000m)
    .ToList();
```

Problem:

```text
The database returns all orders first.
Then C# filters in memory.
```

For large tables, this is bad for performance.

---

## IQueryable vs IEnumerable

```csharp
IQueryable<Order> queryableOrders = dbContext.Orders;

IEnumerable<Order> enumerableOrders = dbContext.Orders.ToList();
```

Main difference:

| Type             | Where query runs                               |
| ---------------- | ---------------------------------------------- |
| `IEnumerable<T>` | In application memory                          |
| `IQueryable<T>`  | Usually in external provider, such as database |

---

## Example: IQueryable composition

```csharp
public IQueryable<Order> ApplyPaidFilter(IQueryable<Order> query)
{
    return query.Where(order => order.IsPaid);
}
```

```csharp
public IQueryable<Order> ApplyHighValueFilter(IQueryable<Order> query)
{
    return query.Where(order => order.TotalAmount > 10000m);
}
```

Usage:

```csharp
IQueryable<Order> query = dbContext.Orders;

query = ApplyPaidFilter(query);
query = ApplyHighValueFilter(query);

List<Order> result = query.ToList();
```

The query is built step by step and executed at the end.

---

## Senior-level note

Be careful with custom C# methods inside `IQueryable`.

Risky:

```csharp
bool IsHighValue(Order order)
{
    return order.TotalAmount > 10000m;
}
```

```csharp
List<Order> result = dbContext.Orders
    .Where(order => IsHighValue(order))
    .ToList();
```

This may fail because EF Core may not know how to translate `IsHighValue` into SQL.

Better:

```csharp
List<Order> result = dbContext.Orders
    .Where(order => order.TotalAmount > 10000m)
    .ToList();
```

For `IQueryable<T>`, use expressions that can be translated.

---

# 3. Extension Methods

## What

An **extension method** allows you to add a method to an existing type without modifying that type.

LINQ methods like `Where`, `Select`, and `OrderBy` are extension methods.

Example:

```csharp
orders.Where(order => order.IsPaid);
```

`Where()` looks like it belongs to `orders`, but it is actually an extension method.

---

## Why

Extension methods make code more readable and fluent.

Instead of:

```csharp
Enumerable.Where(orders, order => order.IsPaid);
```

You can write:

```csharp
orders.Where(order => order.IsPaid);
```

---

## Creating an extension method

Extension methods must be inside a static class.

```csharp
public static class OrderExtensions
{
    public static bool IsHighValue(this Order order)
    {
        return order.TotalAmount > 10000m;
    }
}
```

Important part:

```csharp
this Order order
```

This means the method extends the `Order` type.

---

## Usage

```csharp
Order order = new Order
{
    OrderNumber = "ORD-001",
    TotalAmount = 15000m
};

bool isHighValue = order.IsHighValue();

Console.WriteLine(isHighValue);
```

Output:

```text
True
```

---

## Extension method for collection

```csharp
public static class OrderEnumerableExtensions
{
    public static IEnumerable<Order> PaidOnly(this IEnumerable<Order> orders)
    {
        return orders.Where(order => order.IsPaid);
    }
}
```

Usage:

```csharp
IEnumerable<Order> paidOrders = orders.PaidOnly();
```

---

## Extension method for IQueryable

```csharp
public static class OrderQueryableExtensions
{
    public static IQueryable<Order> PaidOnly(this IQueryable<Order> query)
    {
        return query.Where(order => order.IsPaid);
    }

    public static IQueryable<Order> HighValueOnly(this IQueryable<Order> query)
    {
        return query.Where(order => order.TotalAmount > 10000m);
    }
}
```

Usage:

```csharp
List<Order> result = dbContext.Orders
    .PaidOnly()
    .HighValueOnly()
    .ToList();
```

---

## Senior-level note

Create extension methods when they improve readability.

Good:

```csharp
query.PaidOnly()
query.HighValueOnly()
order.IsHighValue()
```

Avoid extension methods that hide complex business logic.

Risky:

```csharp
order.ProcessPaymentAndSendEmailAndUpdateInventory();
```

That should probably be a service.

Extension methods are best for small, reusable, readable operations.

---

# 4. Deferred Execution

## What

**Deferred execution** means the LINQ query is not executed immediately.

It executes later when you actually enumerate it.

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

---

## Example

```csharp
IEnumerable<Order> paidOrders = orders
    .Where(order =>
    {
        Console.WriteLine($"Checking {order.OrderNumber}");
        return order.IsPaid;
    });

Console.WriteLine("Query created.");

foreach (Order order in paidOrders)
{
    Console.WriteLine($"Paid order: {order.OrderNumber}");
}
```

Output:

```text
Query created.
Checking ORD-001
Paid order: ORD-001
Checking ORD-002
Checking ORD-003
Paid order: ORD-003
```

Notice:

```text
Where did not run when query was created.
It ran during foreach.
```

---

## Why

Deferred execution allows:

```text
Query composition
Better performance
Lazy evaluation
Database query building
Avoiding unnecessary work
```

Example:

```csharp
IQueryable<Order> query = dbContext.Orders;

if (onlyPaid)
{
    query = query.Where(order => order.IsPaid);
}

if (minimumAmount > 0)
{
    query = query.Where(order => order.TotalAmount >= minimumAmount);
}

List<Order> result = query.ToList();
```

The query is executed only once at the end.

---

## Deferred execution with changed data

```csharp
List<int> numbers = new() { 1, 2, 3 };

IEnumerable<int> query = numbers.Where(number => number > 1);

numbers.Add(4);

foreach (int number in query)
{
    Console.WriteLine(number);
}
```

Output:

```text
2
3
4
```

Why?

Because the query runs after `4` was added.

---

## Senior-level note

Deferred execution can cause bugs if you do not understand when the query runs.

Example:

```csharp
IEnumerable<Order> paidOrders = orders.Where(order => order.IsPaid);

orders.Clear();

foreach (Order order in paidOrders)
{
    Console.WriteLine(order.OrderNumber);
}
```

Output:

```text
No output
```

Because the query runs after the list was cleared.

If you want to capture the result now, use immediate execution:

```csharp
List<Order> paidOrders = orders
    .Where(order => order.IsPaid)
    .ToList();
```

---

# 5. Immediate Execution

## What

**Immediate execution** means the query runs immediately and returns a final result.

Common immediate execution methods:

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

---

## Example with `ToList`

```csharp
List<Order> paidOrders = orders
    .Where(order => order.IsPaid)
    .ToList();
```

Here, the query runs immediately.

---

## Example with `Count`

```csharp
int paidOrderCount = orders
    .Where(order => order.IsPaid)
    .Count();

Console.WriteLine(paidOrderCount);
```

Output:

```text
2
```

---

## Example with `Any`

```csharp
bool hasHighValueOrder = orders
    .Any(order => order.TotalAmount > 20000m);

Console.WriteLine(hasHighValueOrder);
```

Output:

```text
True
```

---

## Example with `FirstOrDefault`

```csharp
Order? firstPaidOrder = orders
    .FirstOrDefault(order => order.IsPaid);

if (firstPaidOrder is not null)
{
    Console.WriteLine(firstPaidOrder.OrderNumber);
}
```

Output:

```text
ORD-001
```

---

## Why

Use immediate execution when you need the result now.

Examples:

```text
Need a List
Need count
Need sum
Need first item
Need to send response from API
Need to avoid multiple enumeration
```

---

## First vs FirstOrDefault

```csharp
Order firstPaidOrder = orders.First(order => order.IsPaid);
```

If no item is found, `First()` throws exception.

Safer:

```csharp
Order? firstPaidOrder = orders.FirstOrDefault(order => order.IsPaid);
```

Then check:

```csharp
if (firstPaidOrder is null)
{
    Console.WriteLine("No paid order found.");
}
```

---

## Single vs SingleOrDefault

Use `Single` when exactly one item must exist.

```csharp
Order order = orders.Single(order => order.OrderNumber == "ORD-001");
```

If zero or more than one item exists, it throws exception.

Safer:

```csharp
Order? order = orders.SingleOrDefault(order => order.OrderNumber == "ORD-001");
```

Use `SingleOrDefault` when:

```text
Zero or one item is acceptable.
More than one is an error.
```

---

## Senior-level note

Do not call `ToList()` too early in database queries.

Less ideal:

```csharp
List<Order> allOrders = dbContext.Orders.ToList();

List<Order> result = allOrders
    .Where(order => order.IsPaid)
    .Where(order => order.TotalAmount > 10000m)
    .ToList();
```

Better:

```csharp
List<Order> result = dbContext.Orders
    .Where(order => order.IsPaid)
    .Where(order => order.TotalAmount > 10000m)
    .ToList();
```

Keep the query as `IQueryable<T>` until the final point where you actually need the data.

---

# 6. Expression Trees

## What

An **expression tree** represents code as data.

Normal delegate:

```csharp
Func<Order, bool> filter = order => order.IsPaid;
```

Expression tree:

```csharp
Expression<Func<Order, bool>> filter = order => order.IsPaid;
```

Need namespace:

```csharp
using System.Linq.Expressions;
```

---

## Simple difference

### Func

```csharp
Func<Order, bool> filter = order => order.IsPaid;
```

This is executable code.

It can run in memory.

### Expression

```csharp
Expression<Func<Order, bool>> filter = order => order.IsPaid;
```

This is a data structure describing the code.

It can be inspected, translated, or converted.

---

## Why

Expression trees are used by query providers like Entity Framework Core.

For example:

```csharp
dbContext.Orders.Where(order => order.IsPaid);
```

When used with `IQueryable<T>`, the provider receives an expression tree.

It can translate this:

```csharp
order => order.IsPaid
```

into SQL:

```sql
WHERE IsPaid = 1
```

---

## Example: expression tree

```csharp
using System.Linq.Expressions;

Expression<Func<Order, bool>> paidFilter = order => order.IsPaid;

Console.WriteLine(paidFilter);
```

Output:

```text
order => order.IsPaid
```

---

## Compile expression to executable delegate

```csharp
Expression<Func<Order, bool>> paidFilterExpression =
    order => order.IsPaid;

Func<Order, bool> paidFilterFunc =
    paidFilterExpression.Compile();

Order order = new Order
{
    OrderNumber = "ORD-001",
    IsPaid = true
};

bool result = paidFilterFunc(order);

Console.WriteLine(result);
```

Output:

```text
True
```

---

## Real-world use case: reusable EF Core filter

```csharp
using System.Linq.Expressions;

public static class OrderFilters
{
    public static Expression<Func<Order, bool>> IsPaid()
    {
        return order => order.IsPaid;
    }

    public static Expression<Func<Order, bool>> IsHighValue(decimal minimumAmount)
    {
        return order => order.TotalAmount >= minimumAmount;
    }
}
```

Usage with `IQueryable`:

```csharp
List<Order> result = dbContext.Orders
    .Where(OrderFilters.IsPaid())
    .Where(OrderFilters.IsHighValue(10000m))
    .ToList();
```

This can still be translated to SQL.

---

## Why not just use Func?

This works for in-memory collections:

```csharp
Func<Order, bool> paidFilter = order => order.IsPaid;

List<Order> result = orders
    .Where(paidFilter)
    .ToList();
```

But for EF Core database queries, `Func<Order, bool>` usually causes the logic to be executed in memory or may not translate properly.

For database queries, prefer:

```csharp
Expression<Func<Order, bool>>
```

because the query provider can inspect and translate it.

---

## Example: Func vs Expression

```csharp
Func<Order, bool> funcFilter = order => order.IsPaid;

Expression<Func<Order, bool>> expressionFilter = order => order.IsPaid;
```

Use `Func` for:

```text
In-memory execution
Callbacks
Calculations
Business rules that run in C#
```

Use `Expression<Func<T, bool>>` for:

```text
Database query filters
Dynamic query building
Specification pattern
ORM translation
```

---

## Senior-level note

Expression trees are not only for EF Core, but EF Core is the most common use case for backend developers.

Expression trees allow frameworks to understand your code structure.

Example:

```csharp
order => order.TotalAmount > 10000m
```

An expression tree can represent:

```text
Parameter: order
Property: TotalAmount
Operator: >
Constant: 10000
```

That is why it can be translated into SQL.

A normal `Func` cannot be translated because it is already compiled executable code.

---

# Full Simple Console Example

This example runs fully in memory.

```csharp
using System.Linq.Expressions;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}

public static class OrderExtensions
{
    public static IEnumerable<Order> PaidOnly(this IEnumerable<Order> orders)
    {
        return orders.Where(order => order.IsPaid);
    }

    public static IEnumerable<Order> HighValueOnly(
        this IEnumerable<Order> orders,
        decimal minimumAmount)
    {
        return orders.Where(order => order.TotalAmount >= minimumAmount);
    }
}

public static class Program
{
    public static void Main()
    {
        List<Order> orders = new()
        {
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-001",
                CustomerName = "Kamal",
                TotalAmount = 5000m,
                IsPaid = true,
                CreatedAt = new DateTime(2026, 7, 1)
            },
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-002",
                CustomerName = "Nimal",
                TotalAmount = 25000m,
                IsPaid = false,
                CreatedAt = new DateTime(2026, 7, 2)
            },
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-003",
                CustomerName = "Sunil",
                TotalAmount = 15000m,
                IsPaid = true,
                CreatedAt = new DateTime(2026, 7, 3)
            }
        };

        Console.WriteLine("=== IEnumerable LINQ ===");

        IEnumerable<Order> paidOrders = orders.PaidOnly();

        foreach (Order order in paidOrders)
        {
            Console.WriteLine($"Paid order: {order.OrderNumber}");
        }

        Console.WriteLine();

        Console.WriteLine("=== Immediate Execution ===");

        List<Order> highValuePaidOrders = orders
            .PaidOnly()
            .HighValueOnly(10000m)
            .ToList();

        foreach (Order order in highValuePaidOrders)
        {
            Console.WriteLine($"High value paid order: {order.OrderNumber}");
        }

        Console.WriteLine();

        Console.WriteLine("=== Deferred Execution ===");

        IEnumerable<Order> deferredQuery = orders
            .Where(order =>
            {
                Console.WriteLine($"Checking order: {order.OrderNumber}");
                return order.TotalAmount > 10000m;
            });

        Console.WriteLine("Query created but not executed yet.");

        foreach (Order order in deferredQuery)
        {
            Console.WriteLine($"Result: {order.OrderNumber}");
        }

        Console.WriteLine();

        Console.WriteLine("=== Expression Tree ===");

        Expression<Func<Order, bool>> expressionFilter =
            order => order.IsPaid && order.TotalAmount >= 10000m;

        Console.WriteLine(expressionFilter);

        Func<Order, bool> compiledFilter = expressionFilter.Compile();

        List<Order> expressionResult = orders
            .Where(compiledFilter)
            .ToList();

        foreach (Order order in expressionResult)
        {
            Console.WriteLine($"Expression result: {order.OrderNumber}");
        }
    }
}
```

Expected output:

```text
=== IEnumerable LINQ ===
Paid order: ORD-001
Paid order: ORD-003

=== Immediate Execution ===
High value paid order: ORD-003

=== Deferred Execution ===
Query created but not executed yet.
Checking order: ORD-001
Checking order: ORD-002
Result: ORD-002
Checking order: ORD-003
Result: ORD-003

=== Expression Tree ===
order => (order.IsPaid AndAlso (order.TotalAmount >= 10000))
Expression result: ORD-003
```

---

# LINQ Summary Table

| Topic               | Meaning                                  | Main use                             |
| ------------------- | ---------------------------------------- | ------------------------------------ |
| `IEnumerable` LINQ  | LINQ over in-memory data                 | Lists, arrays, local collections     |
| `IQueryable`        | Query that can be translated by provider | EF Core/database queries             |
| Extension Methods   | Add methods to existing types            | LINQ syntax, reusable filters        |
| Deferred Execution  | Query runs later                         | Query composition, lazy execution    |
| Immediate Execution | Query runs now                           | `ToList`, `Count`, `Any`, `Sum`      |
| Expression Trees    | Code represented as data                 | EF Core translation, dynamic filters |

---

# Very Short Rule

```text
IEnumerable<T>
    In-memory LINQ

IQueryable<T>
    Database/provider LINQ

Extension method
    Adds method-like syntax to existing types

Deferred execution
    Query runs later

Immediate execution
    Query runs now

Expression tree
    Code stored as data, useful for translation
```

For senior .NET development, the most important LINQ skill is knowing **where the query executes**.

```text
IEnumerable<T> = usually application memory
IQueryable<T> = usually database/provider
```

That difference affects performance, SQL generation, memory usage, and application scalability.
