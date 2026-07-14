# Generics in C# with Examples

---

# 1. Generic Classes

## What

A **generic class** is a class that works with any type.

Instead of writing separate classes for `int`, `string`, `Customer`, `Order`, etc., you create one reusable class.

```csharp
public class Box<T>
{
    public T Value { get; set; }

    public Box(T value)
    {
        Value = value;
    }
}
```

Usage:

```csharp
Box<int> numberBox = new Box<int>(100);
Box<string> textBox = new Box<string>("Hello");
Box<Customer> customerBox = new Box<Customer>(new Customer());
```

Here, `T` is a type parameter.

---

## Why

Generic classes give you:

* type safety
* reusable code
* less duplication
* better performance than `object`
* compile-time checking

---

## Without generics

```csharp
public class ObjectBox
{
    public object Value { get; set; }

    public ObjectBox(object value)
    {
        Value = value;
    }
}
```

Usage:

```csharp
ObjectBox box = new ObjectBox(100);

int number = (int)box.Value;
```

Problem: casting is required.

This can fail at runtime:

```csharp
ObjectBox box = new ObjectBox("Hello");

int number = (int)box.Value; // Runtime error
```

---

## With generics

```csharp
public class GenericBox<T>
{
    public T Value { get; set; }

    public GenericBox(T value)
    {
        Value = value;
    }
}
```

Usage:

```csharp
GenericBox<int> box = new GenericBox<int>(100);

int number = box.Value;
```

No cast required.

The compiler knows `Value` is an `int`.

---

## Real-world example: Generic API response

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
```

Usage with customer:

```csharp
ApiResponse<CustomerDto> response = new ApiResponse<CustomerDto>
{
    Success = true,
    Message = "Customer loaded successfully.",
    Data = new CustomerDto
    {
        Id = Guid.NewGuid(),
        Name = "Dinuka"
    }
};
```

Usage with list:

```csharp
ApiResponse<List<CustomerDto>> response = new ApiResponse<List<CustomerDto>>
{
    Success = true,
    Data = new List<CustomerDto>()
};
```

---

## Real-world example: Generic repository interface

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> ListAsync();
    Task AddAsync(T entity);
    Task DeleteAsync(T entity);
}
```

Usage:

```csharp
public class CustomerRepository : IRepository<Customer>
{
    public Task<Customer?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Customer>> ListAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Customer entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Customer entity)
    {
        throw new NotImplementedException();
    }
}
```

---

## Senior-level note

Generics are good when the behavior is truly common.

Good generic use:

```csharp
ApiResponse<T>
PagedResult<T>
Repository<T>
Result<T>
CacheService<T>
```

Bad generic use:

```csharp
public class Manager<T>
{
}
```

If `T` does not give real value, it makes the design harder to understand.

---

# 2. Generic Methods

## What

A **generic method** is a method that works with different types.

The class itself does not need to be generic.

```csharp
public T Echo<T>(T value)
{
    return value;
}
```

Usage:

```csharp
int number = Echo<int>(100);
string text = Echo<string>("Hello");
```

C# can usually infer the type:

```csharp
int number = Echo(100);
string text = Echo("Hello");
```

---

## Why

Generic methods are useful when only one method needs to be reusable.

You do not need to make the whole class generic.

---

## Example: generic logging method

```csharp
public void LogValue<T>(T value)
{
    Console.WriteLine($"Value: {value}");
}
```

Usage:

```csharp
LogValue(100);
LogValue("Payment completed");
LogValue(DateTime.UtcNow);
```

---

## Example: generic validation result

```csharp
public Result<T> Success<T>(T data)
{
    return new Result<T>
    {
        IsSuccess = true,
        Data = data
    };
}
```

Supporting class:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public T? Data { get; set; }
}
```

Usage:

```csharp
Result<CustomerDto> result = Success(new CustomerDto
{
    Id = Guid.NewGuid(),
    Name = "Dinuka"
});
```

---

## Example: generic method with two type parameters

```csharp
public Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(
    TKey key,
    TValue value)
    where TKey : notnull
{
    return new Dictionary<TKey, TValue>
    {
        { key, value }
    };
}
```

Usage:

```csharp
Dictionary<string, int> ages = CreateDictionary("Dinuka", 31);
Dictionary<Guid, string> users = CreateDictionary(Guid.NewGuid(), "Admin");
```

---

## Real-world example: generic mapper method

```csharp
public TDestination Map<TSource, TDestination>(TSource source)
{
    // Simplified example
    throw new NotImplementedException();
}
```

Usage:

```csharp
CustomerDto dto = Map<Customer, CustomerDto>(customer);
```

---

## Senior-level note

Prefer generic methods when type safety matters.

Less safe:

```csharp
public object GetValue()
{
    return "Hello";
}
```

Better:

```csharp
public T GetValue<T>()
{
    throw new NotImplementedException();
}
```

But do not overuse generic methods when a normal method is clearer.

---

# 3. Generic Constraints

## What

**Generic constraints** restrict what type can be passed to a generic class or method.

They are written using `where`.

```csharp
public class Repository<T>
    where T : class
{
}
```

This means `T` must be a reference type.

---

## Why

Constraints allow you to safely use certain members or behaviors inside generic code.

Without constraints, C# knows almost nothing about `T`.

Example:

```csharp
public void PrintId<T>(T entity)
{
    Console.WriteLine(entity.Id); // Not allowed
}
```

C# does not know whether `T` has an `Id`.

---

## Interface constraint

```csharp
public interface IEntity
{
    Guid Id { get; }
}
```

```csharp
public void PrintId<T>(T entity)
    where T : IEntity
{
    Console.WriteLine(entity.Id);
}
```

Usage:

```csharp
public class Customer : IEntity
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = "";
}
```

```csharp
Customer customer = new Customer();

PrintId(customer);
```

Now C# knows `T` has `Id`.

---

## `class` constraint

`T` must be a reference type.

```csharp
public class Repository<T>
    where T : class
{
    public T? FindById(Guid id)
    {
        return null;
    }
}
```

Usage:

```csharp
Repository<Customer> customerRepository = new Repository<Customer>();
```

Invalid:

```csharp
Repository<int> numberRepository = new Repository<int>(); // Not allowed
```

---

## `struct` constraint

`T` must be a value type.

```csharp
public class ValueContainer<T>
    where T : struct
{
    public T Value { get; set; }
}
```

Usage:

```csharp
ValueContainer<int> number = new ValueContainer<int>();
ValueContainer<DateTime> date = new ValueContainer<DateTime>();
```

Invalid:

```csharp
ValueContainer<string> text = new ValueContainer<string>(); // Not allowed
```

---

## `new()` constraint

`T` must have a public parameterless constructor.

```csharp
public T CreateInstance<T>()
    where T : new()
{
    return new T();
}
```

Usage:

```csharp
Customer customer = CreateInstance<Customer>();
```

Customer class:

```csharp
public class Customer
{
    public string Name { get; set; } = "";
}
```

---

## Interface + constructor constraint

```csharp
public T CreateEntity<T>()
    where T : IEntity, new()
{
    T entity = new T();

    Console.WriteLine(entity.Id);

    return entity;
}
```

---

## Base class constraint

```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }
}
```

```csharp
public class Repository<T>
    where T : Entity
{
    public void PrintId(T entity)
    {
        Console.WriteLine(entity.Id);
    }
}
```

Usage:

```csharp
public class Order : Entity
{
    public decimal TotalAmount { get; set; }
}
```

```csharp
Repository<Order> repository = new Repository<Order>();
```

---

## `notnull` constraint

`T` cannot be nullable.

```csharp
public class Cache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _items = new();

    public void Set(TKey key, TValue value)
    {
        _items[key] = value;
    }
}
```

Why useful?

Dictionary keys cannot be `null`.

Usage:

```csharp
Cache<string, Customer> cache = new Cache<string, Customer>();
```

---

## `unmanaged` constraint

`T` must be an unmanaged type.

Useful for low-level memory/performance code.

```csharp
public void PrintSize<T>()
    where T : unmanaged
{
    Console.WriteLine(sizeof(T));
}
```

Common unmanaged types:

```csharp
int
long
float
double
decimal
bool
char
Guid
```

You will not use this often in normal business applications.

---

## `Enum` constraint

```csharp
public bool IsValidEnumValue<TEnum>(TEnum value)
    where TEnum : struct, Enum
{
    return Enum.IsDefined(typeof(TEnum), value);
}
```

Usage:

```csharp
bool valid = IsValidEnumValue(OrderStatus.Paid);
```

---

## Common constraint examples

| Constraint            | Meaning                               |
| --------------------- | ------------------------------------- |
| `where T : class`     | T must be reference type              |
| `where T : struct`    | T must be value type                  |
| `where T : new()`     | T must have parameterless constructor |
| `where T : IEntity`   | T must implement interface            |
| `where T : Entity`    | T must inherit base class             |
| `where T : notnull`   | T cannot be nullable                  |
| `where T : unmanaged` | T must be unmanaged type              |
| `where T : Enum`      | T must be enum type                   |

---

## Senior-level note

Do not add constraints without reason.

Bad:

```csharp
public class Service<T>
    where T : class, new()
{
}
```

If you do not actually create `new T()` inside the class, `new()` is unnecessary.

Good:

```csharp
public T Create<T>()
    where T : new()
{
    return new T();
}
```

Constraints should express a real requirement.

---

# 4. Covariance

## What

**Covariance** allows you to use a more derived type where a base type is expected.

In C#, covariance uses the `out` keyword in generic interfaces and delegates.

Simple meaning:

```csharp
IEnumerable<Dog>
```

can be used as:

```csharp
IEnumerable<Animal>
```

because `Dog` is an `Animal`.

---

## Example classes

```csharp
public class Animal
{
    public string Name { get; set; } = "";
}
```

```csharp
public class Dog : Animal
{
    public void Bark()
    {
        Console.WriteLine("Bark");
    }
}
```

---

## Covariance with `IEnumerable<T>`

```csharp
IEnumerable<Dog> dogs = new List<Dog>
{
    new Dog { Name = "Rocky" },
    new Dog { Name = "Max" }
};

IEnumerable<Animal> animals = dogs;
```

This works because `IEnumerable<out T>` is covariant.

`IEnumerable<T>` only produces values. You read from it.

```csharp
foreach (Animal animal in animals)
{
    Console.WriteLine(animal.Name);
}
```

---

## Why covariance is safe

This is safe because you are only reading animals from the collection.

Every `Dog` is an `Animal`.

So this is safe:

```csharp
Animal animal = dogs.First();
```

---

## Covariant custom interface

```csharp
public interface IReadOnlyRepository<out T>
{
    T GetById(Guid id);
}
```

`out T` means this interface only returns `T`.

Implementation:

```csharp
public class DogRepository : IReadOnlyRepository<Dog>
{
    public Dog GetById(Guid id)
    {
        return new Dog { Name = "Rocky" };
    }
}
```

Usage:

```csharp
IReadOnlyRepository<Dog> dogRepository = new DogRepository();

IReadOnlyRepository<Animal> animalRepository = dogRepository;
```

This works because of covariance.

---

## Important rule

With `out T`, you can return `T`, but you cannot accept `T` as method input.

Allowed:

```csharp
public interface IProducer<out T>
{
    T Produce();
}
```

Not allowed:

```csharp
public interface IProducer<out T>
{
    void Save(T item); // Not allowed
}
```

Because `T` is used as input.

---

## Senior-level note

Covariance is common in read-only abstractions.

Examples:

```csharp
IEnumerable<out T>
IReadOnlyList<out T>
IQueryable<out T>
```

Good design:

```csharp
public interface IReadOnlyQuery<out TResult>
{
    TResult Execute();
}
```

Use covariance when your interface only produces/returns values.

---

# 5. Contravariance

## What

**Contravariance** allows you to use a more general type where a more specific type is expected.

In C#, contravariance uses the `in` keyword in generic interfaces and delegates.

Simple meaning:

A handler that can handle `Animal` can also handle `Dog`.

Because `Dog` is an `Animal`.

---

## Example classes

```csharp
public class Animal
{
    public string Name { get; set; } = "";
}
```

```csharp
public class Dog : Animal
{
}
```

---

## Contravariance with custom interface

```csharp
public interface IHandler<in T>
{
    void Handle(T item);
}
```

`in T` means this interface accepts `T` as input.

Implementation:

```csharp
public class AnimalHandler : IHandler<Animal>
{
    public void Handle(Animal animal)
    {
        Console.WriteLine($"Handling animal: {animal.Name}");
    }
}
```

Usage:

```csharp
IHandler<Animal> animalHandler = new AnimalHandler();

IHandler<Dog> dogHandler = animalHandler;

dogHandler.Handle(new Dog { Name = "Rocky" });
```

This works because an `AnimalHandler` can handle any animal, including dogs.

---

## Why contravariance is safe

If a method accepts `Animal`, then passing a `Dog` is safe.

```csharp
public void Handle(Animal animal)
{
}
```

Calling it with this is safe:

```csharp
Handle(new Dog());
```

Because `Dog` is an `Animal`.

---

## Contravariance with `Action<T>`

`Action<T>` is contravariant.

```csharp
Action<Animal> handleAnimal = animal =>
{
    Console.WriteLine(animal.Name);
};

Action<Dog> handleDog = handleAnimal;

handleDog(new Dog { Name = "Max" });
```

This works because a method that accepts `Animal` can accept `Dog`.

---

## Important rule

With `in T`, you can accept `T` as input, but you cannot return `T`.

Allowed:

```csharp
public interface IConsumer<in T>
{
    void Consume(T item);
}
```

Not allowed:

```csharp
public interface IConsumer<in T>
{
    T Get(); // Not allowed
}
```

Because `T` is used as output.

---

## Senior-level note

Contravariance is common in command handlers, validators, and comparers.

Examples:

```csharp
IComparer<in T>
IEqualityComparer<in T>
IProgress<in T>
Action<in T>
```

Good design:

```csharp
public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command);
}
```

Example:

```csharp
public class CreateOrderCommand
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
}
```

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    public Task HandleAsync(CreateOrderCommand command)
    {
        Console.WriteLine($"Creating order for {command.CustomerId}");
        return Task.CompletedTask;
    }
}
```

Use contravariance when your interface only consumes/accepts values.

---

# Covariance vs Contravariance Simple Table

| Concept        | Keyword | Direction | Meaning                 | Example                                    |
| -------------- | ------- | --------- | ----------------------- | ------------------------------------------ |
| Covariance     | `out`   | Output    | Produces/returns values | `IEnumerable<Dog>` → `IEnumerable<Animal>` |
| Contravariance | `in`    | Input     | Consumes/accepts values | `IHandler<Animal>` → `IHandler<Dog>`       |

---

# Combined Example

```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
```

```csharp
public class Customer : Entity
{
    public string Name { get; set; } = "";
}
```

```csharp
public class Order : Entity
{
    public decimal TotalAmount { get; set; }
}
```

Generic repository with constraint:

```csharp
public interface IRepository<T>
    where T : Entity
{
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
}
```

Generic response:

```csharp
public class Result<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public string? Error { get; private set; }

    private Result()
    {
    }

    public static Result<T> Ok(T data)
    {
        return new Result<T>
        {
            Success = true,
            Data = data
        };
    }

    public static Result<T> Fail(string error)
    {
        return new Result<T>
        {
            Success = false,
            Error = error
        };
    }
}
```

Generic method:

```csharp
public Result<T> CreateSuccessResult<T>(T data)
{
    return Result<T>.Ok(data);
}
```

Usage:

```csharp
Customer customer = new Customer
{
    Name = "Dinuka"
};

Result<Customer> result = CreateSuccessResult(customer);
```

Covariant query interface:

```csharp
public interface IQuery<out TResult>
{
    TResult Execute();
}
```

Contravariant command handler:

```csharp
public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command);
}
```

---

# Senior Interview Summary

| Topic               | Interview explanation                                  |
| ------------------- | ------------------------------------------------------ |
| Generic Classes     | Reusable type-safe classes                             |
| Generic Methods     | Reusable type-safe methods                             |
| Generic Constraints | Restrict generic types to guarantee behavior           |
| Covariance          | Allows derived output to be treated as base output     |
| Contravariance      | Allows base input handler to be used for derived input |

A strong senior-level answer:

> Generics allow us to write reusable and type-safe code without using `object` and manual casting. They improve compile-time safety, reduce duplication, and avoid boxing for value types. Generic constraints help us express what capabilities a type must have. Covariance and contravariance help us safely substitute generic types in inheritance scenarios, especially for producers and consumers.
