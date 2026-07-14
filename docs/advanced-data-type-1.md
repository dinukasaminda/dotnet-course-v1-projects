# Advanced Type System in C# with Examples

---

# 1. Value Types vs Reference Types

## What

C# types are mainly divided into:

### Value types

A value type stores the actual value.

Examples:

```csharp
int age = 30;
decimal salary = 150000m;
bool isActive = true;
DateTime createdAt = DateTime.UtcNow;
Guid id = Guid.NewGuid();
```

Common value types:

```csharp
int
long
decimal
double
bool
char
DateTime
Guid
enum
struct
```

### Reference types

A reference type variable stores a reference to an object.

Examples:

```csharp
Customer customer = new Customer();
string name = "Dinuka";
List<int> numbers = new List<int>();
```

Common reference types:

```csharp
class
string
object
array
List<T>
Dictionary<TKey, TValue>
interface
delegate
record class
```

---

## Example: value type copy

```csharp
int a = 10;
int b = a;

b = 20;

Console.WriteLine(a); // 10
Console.WriteLine(b); // 20
```

Changing `b` does not affect `a`.

Because `int` is a value type.

---

## Example: reference type copy

```csharp
public class Customer
{
    public string Name { get; set; } = "";
}
```

```csharp
Customer customer1 = new Customer();
customer1.Name = "Kamal";

Customer customer2 = customer1;
customer2.Name = "Nimal";

Console.WriteLine(customer1.Name); // Nimal
Console.WriteLine(customer2.Name); // Nimal
```

Both variables point to the same object.

---

## Why it matters

You need to understand this for:

* memory behavior
* object mutation
* method parameters
* equality comparison
* performance
* bugs caused by shared references

---

##  note

* Value type variable contains the actual value.
* Reference type variable contains a reference to an object.
* Where it is stored depends on context, compiler, runtime, fields, closures, async methods, etc.

---

# 2. Boxing and Unboxing

## What

**Boxing** means converting a value type into an object/reference type.

**Unboxing** means converting it back from object to the original value type.

---

## Boxing example

```csharp
int number = 100;

object boxedNumber = number;
```

Here, `int` is boxed into `object`.

---

## Unboxing example

```csharp
object boxedNumber = 100;

int number = (int)boxedNumber;
```

---

## Invalid unboxing

```csharp
object boxedNumber = 100;

long number = (long)boxedNumber; // Runtime error
```

Even though `int` can be converted to `long`, unboxing requires the exact original type.

Correct:

```csharp
object boxedNumber = 100;

int temp = (int)boxedNumber;
long number = temp;
```

---

## Why it matters

Boxing creates extra memory allocation.

In high-performance code, too much boxing can cause:

* more allocations
* more garbage collection
* slower execution

---

## Bad example

```csharp
ArrayList numbers = new ArrayList();

numbers.Add(10);
numbers.Add(20);
numbers.Add(30);
```

`ArrayList` stores items as `object`, so integers are boxed.

---

## Better example

```csharp
List<int> numbers = new List<int>();

numbers.Add(10);
numbers.Add(20);
numbers.Add(30);
```

`List<int>` avoids boxing.

---

##  note

Generics were introduced partly to avoid boxing and casting problems.

Prefer this:

```csharp
List<int> numbers = new();
```

Instead of this:

```csharp
ArrayList numbers = new();
```

---

# 3. Records

## What

A **record** is a type designed mainly for immutable data and value-based equality.

Records are useful for:

* DTOs
* response models
* value objects
* configuration models
* messages/events
* query results

---

## Record class example

```csharp
public record CustomerDto(
    Guid Id,
    string Name,
    string Email
);
```

Usage:

```csharp
CustomerDto customer = new CustomerDto(
    Guid.NewGuid(),
    "Dinuka",
    "dinuka@example.com"
);

Console.WriteLine(customer.Name);
```

---

## Value-based equality

```csharp
CustomerDto customer1 = new CustomerDto(
    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
    "Dinuka",
    "dinuka@example.com"
);

CustomerDto customer2 = new CustomerDto(
    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
    "Dinuka",
    "dinuka@example.com"
);

Console.WriteLine(customer1 == customer2); // True
```

With normal classes, this would usually be `false` unless equality is manually implemented.

---

## `with` expression

Records support non-destructive mutation.

```csharp
CustomerDto original = new CustomerDto(
    Guid.NewGuid(),
    "Dinuka",
    "old@example.com"
);

CustomerDto updated = original with
{
    Email = "new@example.com"
};
```

The original object is not changed.

---

## Why records matter

Records are good when identity is based on values.

Example:

```csharp
public record Money(decimal Amount, string Currency);
```

```csharp
Money price1 = new Money(1000m, "LKR");
Money price2 = new Money(1000m, "LKR");

Console.WriteLine(price1 == price2); // True
```

---

##  note

Use records for data models, not always for domain entities.

Good for DTO:

```csharp
public record OrganizationListItemDto(
    Guid Id,
    string Name,
    string Status
);
```

Be careful using records for mutable domain entities like:

```csharp
Order
Customer
Invoice
Hotel
Room
```

because those usually have identity and lifecycle, not only value equality.

---

# 4. Structs

## What

A `struct` is a value type.

It is useful for small, lightweight values.

Examples from .NET:

```csharp
DateTime
Guid
TimeSpan
decimal
int
bool
```

---

## Custom struct example

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

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.");
        }

        Amount = amount;
        Currency = currency;
    }
}
```

Usage:

```csharp
Money price = new Money(5000m, "LKR");

Console.WriteLine(price.Amount);
Console.WriteLine(price.Currency);
```

---

## Struct copy behavior

```csharp
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}
```

```csharp
Point point1 = new Point { X = 10, Y = 20 };
Point point2 = point1;

point2.X = 99;

Console.WriteLine(point1.X); // 10
Console.WriteLine(point2.X); // 99
```

Structs are copied by value.

---

## Why structs matter

Structs can be useful for:

* small data types
* immutable values
* performance-sensitive code
* avoiding heap allocations in some cases

---

##  note

Avoid large mutable structs.

Bad:

```csharp
public struct LargeOrderData
{
    public string CustomerName { get; set; }
    public string Address { get; set; }
    public List<string> Items { get; set; }
    public decimal TotalAmount { get; set; }
}
```

Better as class:

```csharp
public class Order
{
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public List<string> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
```

Good struct design:

```csharp
public readonly struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
        {
            throw new ArgumentException("Percentage must be between 0 and 100.");
        }

        Value = value;
    }
}
```

---


---

# Summary Table

| Concept                        | Main use                                |
| ------------------------------ | --------------------------------------- |
| Value Types vs Reference Types | Understand copy behavior and mutation   |
| Boxing and Unboxing            | Avoid unnecessary allocations and casts |
| Records                        | Immutable data and value-based equality |
| Structs                        | Small value-based types                 |

