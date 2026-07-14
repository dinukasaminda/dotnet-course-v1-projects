# Advanced Type System in C# with Examples

# 9. Object Initializers

## What

Object initializers allow you to create an object and set properties in one expression.

---

## Example

```csharp
public class Customer
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}
```

Usage:

```csharp
Customer customer = new Customer
{
    Name = "Dinuka",
    Email = "dinuka@example.com"
};
```

---

## Without object initializer

```csharp
Customer customer = new Customer();

customer.Name = "Dinuka";
customer.Email = "dinuka@example.com";
```

Object initializer is cleaner.

---

## Object initializer with constructor

```csharp
public class Product
{
    public Product(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
```

Usage:

```csharp
Product product = new Product(Guid.NewGuid())
{
    Name = "Keyboard",
    Price = 12000m
};
```

---

## Why object initializers matter

They are useful for:

* DTOs
* test data
* configuration objects
* simple data models
* object creation readability

---

##  note

Object initializers are not always best for domain entities.

Risky domain model:

```csharp
public class Order
{
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
}
```

Usage:

```csharp
Order order = new Order
{
    TotalAmount = -100,
    IsPaid = true
};
```

This allows invalid state.

Better domain model:

```csharp
public class Order
{
    public decimal TotalAmount { get; private set; }
    public bool IsPaid { get; private set; }

    public Order(decimal totalAmount)
    {
        if (totalAmount <= 0)
        {
            throw new ArgumentException("Total amount must be greater than zero.");
        }

        TotalAmount = totalAmount;
    }

    public void MarkAsPaid()
    {
        IsPaid = true;
    }
}
```

Use object initializers mostly for data objects, not behavior-rich domain objects.

---

# 10. Collection Initializers

## What

Collection initializers allow you to create and fill a collection in one expression.

---

## List example

```csharp
List<string> names = new List<string>
{
    "Kamal",
    "Nimal",
    "Sunil"
};
```

Shorter version:

```csharp
List<string> names = new()
{
    "Kamal",
    "Nimal",
    "Sunil"
};
```

---

## Dictionary example

```csharp
Dictionary<string, string> countries = new()
{
    { "LK", "Sri Lanka" },
    { "IN", "India" },
    { "US", "United States" }
};
```

Alternative dictionary initializer:

```csharp
Dictionary<string, string> countries = new()
{
    ["LK"] = "Sri Lanka",
    ["IN"] = "India",
    ["US"] = "United States"
};
```

---

## Collection of objects

```csharp
List<Product> products = new()
{
    new Product
    {
        Name = "Keyboard",
        Price = 12000m
    },
    new Product
    {
        Name = "Mouse",
        Price = 5000m
    }
};
```

---

## Why collection initializers matter

They improve readability when creating:

* test data
* seed data
* default values
* in-memory collections
* DTO lists

---

##  note

Collection initializers are good for small fixed collections.

Good:

```csharp
private static readonly List<string> SupportedCurrencies = new()
{
    "LKR",
    "USD",
    "EUR"
};
```

But for large or dynamic data, load from configuration or database.

Also prefer `IReadOnlyList<T>` when exposing collections:

```csharp
public class Order
{
    private readonly List<string> _items = new();

    public IReadOnlyList<string> Items => _items;

    public void AddItem(string item)
    {
        _items.Add(item);
    }
}
```

This prevents external code from directly modifying the internal list.

---

# 11. Required Members

## What

`required` means the caller must set that property or field when creating the object.

It helps prevent incomplete objects.

---

## Example

```csharp
public class CreateCustomerRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
}
```

Usage:

```csharp
CreateCustomerRequest request = new CreateCustomerRequest
{
    Name = "Dinuka",
    Email = "dinuka@example.com"
};
```

This is valid.

---

## Missing required property

```csharp
CreateCustomerRequest request = new CreateCustomerRequest
{
    Name = "Dinuka"
};
```

This gives a compile-time error because `Email` is required.

---

## Required with `init`

```csharp
public class RegisterUserRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
```

Usage:

```csharp
RegisterUserRequest request = new RegisterUserRequest
{
    Email = "user@example.com",
    Password = "StrongPassword123"
};
```

After creation, values cannot be changed:

```csharp
request.Email = "new@example.com"; // Not allowed
```

---

## Why required members matter

They are useful for:

* request models
* DTOs
* configuration models
* immutable-style objects
* preventing missing important values

---

##  note

`required` does not validate business rules.

This compiles:

```csharp
CreateCustomerRequest request = new CreateCustomerRequest
{
    Name = "",
    Email = ""
};
```

The properties are set, but values are invalid.

So you still need validation:

```csharp
public void Validate(CreateCustomerRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        throw new ArgumentException("Name is required.");
    }

    if (string.IsNullOrWhiteSpace(request.Email))
    {
        throw new ArgumentException("Email is required.");
    }
}
```

Use `required` for object completeness.
Use validation for correctness.

---

# Combined Example

```csharp
public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled
}
```

```csharp
public readonly struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.");
        }

        Amount = amount;
        Currency = currency;
    }
}
```

```csharp
public record BookingDto(
    Guid Id,
    string GuestName,
    Money TotalAmount,
    BookingStatus Status
);
```

```csharp
public class CreateBookingRequest
{
    public required string GuestName { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
}
```

```csharp
CreateBookingRequest request = new CreateBookingRequest
{
    GuestName = "Dinuka",
    Amount = 25000m,
    Currency = "LKR"
};

BookingDto booking = new BookingDto(
    Guid.NewGuid(),
    request.GuestName,
    new Money(request.Amount, request.Currency),
    BookingStatus.Pending
);

BookingDto confirmedBooking = booking with
{
    Status = BookingStatus.Confirmed
};

Console.WriteLine(confirmedBooking);
```

---

# Summary Table

| Concept                        | Main use                                |
| ------------------------------ | --------------------------------------- |
| Object Initializers            | Cleaner object creation                 |
| Collection Initializers        | Cleaner collection creation             |
| Required Members               | Force important properties to be set    |

