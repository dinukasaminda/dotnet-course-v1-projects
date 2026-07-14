# Advanced Type System in C# with Examples

---
# 5. Enums

## What

An `enum` defines a named set of constant values.

It makes code more readable and safer than raw numbers or strings.

---

## Example

```csharp
public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Cancelled
}
```

Usage:

```csharp
OrderStatus status = OrderStatus.Pending;

if (status == OrderStatus.Pending)
{
    Console.WriteLine("Order is pending.");
}
```

---

## Switch with enum

```csharp
public string GetStatusMessage(OrderStatus status)
{
    return status switch
    {
        OrderStatus.Pending => "Order is waiting for payment.",
        OrderStatus.Paid => "Order has been paid.",
        OrderStatus.Shipped => "Order has been shipped.",
        OrderStatus.Cancelled => "Order has been cancelled.",
        _ => "Unknown order status."
    };
}
```

---

## Why enums matter

Without enum:

```csharp
string status = "Paid";
```

Problem:

```csharp
status = "paid";
status = "PAID";
status = "Paidd";
```

Better:

```csharp
OrderStatus status = OrderStatus.Paid;
```

Now the compiler helps you.

---

## Enum with explicit values

```csharp
public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}
```

This is useful when storing values in a database or communicating with external systems.

---

## Flags enum

Use `[Flags]` when values can be combined.

```csharp
[Flags]
public enum Permission
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    Approve = 8
}
```

Usage:

```csharp
Permission userPermissions = Permission.Read | Permission.Write;

bool canWrite = userPermissions.HasFlag(Permission.Write);

Console.WriteLine(canWrite); // True
```

---

##  note

Enums are good for stable lists.

Good:

```csharp
OrderStatus
PaymentStatus
UserRole
BookingStatus
```

Be careful when the values are managed by users or database configuration.

Example:

```csharp
RoomType
Department
Country
Currency
```

These may be better as database records instead of enums.

---

# 6. Tuples

## What

A tuple groups multiple values together without creating a separate class.

---

## Simple tuple

```csharp
(string name, int age) person = ("Dinuka", 31);

Console.WriteLine(person.name);
Console.WriteLine(person.age);
```

---

## Returning multiple values

```csharp
public (bool Success, string Message) ValidateOrder(decimal amount)
{
    if (amount <= 0)
    {
        return (false, "Amount must be greater than zero.");
    }

    return (true, "Valid order.");
}
```

Usage:

```csharp
var result = ValidateOrder(5000m);

Console.WriteLine(result.Success);
Console.WriteLine(result.Message);
```

---

## Tuple deconstruction

```csharp
(bool success, string message) = ValidateOrder(5000m);

Console.WriteLine(success);
Console.WriteLine(message);
```

---

## Why tuples matter

Tuples are useful for small temporary results.

Good use:

```csharp
public (int TotalCount, int PageCount) CalculatePagination(int totalItems, int pageSize)
{
    int pageCount = (int)Math.Ceiling((double)totalItems / pageSize);

    return (totalItems, pageCount);
}
```

---

##  note

Do not overuse tuples for important domain concepts.

Less clear:

```csharp
public (Guid, string, decimal, bool) GetOrder()
{
    return (Guid.NewGuid(), "ORD-001", 5000m, true);
}
```

Better:

```csharp
public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    decimal TotalAmount,
    bool IsPaid
);
```

Use tuples for small internal operations. Use records/classes for public APIs and domain models.

---

# 7. Anonymous Types

## What

An anonymous type is an object without a named class.

The compiler creates the type for you.

---

## Example

```csharp
var customer = new
{
    Name = "Dinuka",
    Email = "dinuka@example.com"
};

Console.WriteLine(customer.Name);
Console.WriteLine(customer.Email);
```

You did not create a `Customer` class, but C# creates an anonymous type internally.

---

## LINQ projection example

```csharp
var customers = new List<Customer>
{
    new Customer { Id = Guid.NewGuid(), Name = "Kamal", Email = "kamal@example.com" },
    new Customer { Id = Guid.NewGuid(), Name = "Nimal", Email = "nimal@example.com" }
};

var result = customers.Select(customer => new
{
    customer.Id,
    customer.Name
});
```

Here, the result contains only `Id` and `Name`.

---

## Why anonymous types matter

Useful for:

* LINQ projections
* temporary shapes
* internal transformations
* selecting only required fields

---

##  note

Anonymous types are good inside a method.

Good:

```csharp
var query = orders.Select(order => new
{
    order.Id,
    order.TotalAmount,
    order.CreatedAt
});
```

Avoid returning anonymous types from public methods.

Bad:

```csharp
public object GetCustomer()
{
    return new
    {
        Name = "Dinuka",
        Email = "dinuka@example.com"
    };
}
```

Better:

```csharp
public record CustomerDto(string Name, string Email);
```

```csharp
public CustomerDto GetCustomer()
{
    return new CustomerDto("Dinuka", "dinuka@example.com");
}
```

---

# 8. Dynamic Types

## What

`dynamic` tells C# to skip compile-time type checking and resolve members at runtime.

---

## Example

```csharp
dynamic value = "Hello";

Console.WriteLine(value.Length);
```

This works because the runtime value is a string.

---

## Runtime error example

```csharp
dynamic value = 100;

Console.WriteLine(value.Length);
```

This compiles, but fails at runtime because `int` does not have `Length`.

---

## Why dynamic exists

`dynamic` is useful when working with:

* COM interop
* reflection-heavy code
* dynamic JSON structures
* scripting scenarios
* APIs where shape is unknown

---

## Example with dynamic object

```csharp
dynamic customer = new ExpandoObject();

customer.Name = "Dinuka";
customer.Email = "dinuka@example.com";

Console.WriteLine(customer.Name);
```

Need namespace:

```csharp
using System.Dynamic;
```

---

##  note

Avoid `dynamic` in normal application code.

Bad:

```csharp
public dynamic GetOrder()
{
    return new
    {
        Id = Guid.NewGuid(),
        Total = 5000m
    };
}
```

Better:

```csharp
public record OrderDto(Guid Id, decimal Total);
```

```csharp
public OrderDto GetOrder()
{
    return new OrderDto(Guid.NewGuid(), 5000m);
}
```

Use strong types when possible because they give:

* compile-time safety
* refactoring support
* better IDE support
* fewer runtime errors

---

# Summary Table

| Concept                        | Main use                                |
| ------------------------------ | --------------------------------------- |
| Enums                          | Named fixed values                      |
| Tuples                         | Temporary multiple return values        |
| Anonymous Types                | Temporary projections                   |
| Dynamic Types                  | Runtime typing for special cases        |

