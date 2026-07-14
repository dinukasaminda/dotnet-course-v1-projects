# Collections in C# with Examples

---

# 1. Arrays

## What

An **array** stores a fixed-size collection of values of the same type.

```csharp
int[] numbers = new int[3];

numbers[0] = 10;
numbers[1] = 20;
numbers[2] = 30;
```

Shorter syntax:

```csharp
int[] numbers = { 10, 20, 30 };
```

Access by index:

```csharp
Console.WriteLine(numbers[0]); // 10
Console.WriteLine(numbers[1]); // 20
```

---

## Why

Arrays are useful when:

* size is fixed
* index-based access is needed
* performance matters
* you are working with low-level APIs
* you are handling binary data, buffers, or fixed input

---

## Example

```csharp
string[] roles = { "Admin", "Manager", "User" };

for (int i = 0; i < roles.Length; i++)
{
    Console.WriteLine(roles[i]);
}
```

---

## Example with objects

```csharp
public class Product
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
```

```csharp
Product[] products =
{
    new Product { Name = "Keyboard", Price = 12000m },
    new Product { Name = "Mouse", Price = 5000m }
};

foreach (Product product in products)
{
    Console.WriteLine($"{product.Name} - {product.Price}");
}
```

---

## Senior-level note

Arrays are fixed-size.

This is not allowed:

```csharp
numbers.Add(40); // Not possible
```

For dynamic data, prefer:

```csharp
List<int> numbers = new();
```

Use arrays when the size is naturally fixed.

Good example:

```csharp
byte[] fileBytes = File.ReadAllBytes("invoice.pdf");
```

Less ideal:

```csharp
Customer[] customers = new Customer[1000];
```

If customers are added/removed dynamically, use `List<Customer>`.

---

# 2. Collections

## What

A **collection** is a type that stores multiple values.

Common generic collections:

```csharp
List<T>
Dictionary<TKey, TValue>
HashSet<T>
Queue<T>
Stack<T>
```

These are available mainly from:

```csharp
System.Collections.Generic
```

---

## Why

Collections help you manage groups of data:

* list of customers
* list of orders
* product catalog
* user permissions
* processing jobs
* cache lookups
* unique values

---

## Common collection examples

```csharp
List<string> names = new();
Dictionary<Guid, string> users = new();
HashSet<string> permissions = new();
Queue<string> jobs = new();
Stack<string> undoHistory = new();
```

---

## Generic vs non-generic collections

Old non-generic collection:

```csharp
ArrayList items = new ArrayList();

items.Add(10);
items.Add("Hello");
items.Add(true);
```

Problem: it accepts any object.

Better:

```csharp
List<int> numbers = new();

numbers.Add(10);
numbers.Add(20);
```

Generic collections are type-safe.

---

## Senior-level note

Prefer generic collections.

Good:

```csharp
List<Order> orders = new();
Dictionary<Guid, Customer> customersById = new();
HashSet<string> uniqueEmails = new();
```

Avoid old non-generic collections in normal modern C#:

```csharp
ArrayList
Hashtable
Queue
Stack
```

Prefer:

```csharp
List<T>
Dictionary<TKey, TValue>
Queue<T>
Stack<T>
```

---

# 3. IEnumerable

## What

`IEnumerable<T>` represents a sequence that can be iterated.

It is read-only from the consumer perspective.

```csharp
IEnumerable<string> names = new List<string>
{
    "Kamal",
    "Nimal",
    "Sunil"
};

foreach (string name in names)
{
    Console.WriteLine(name);
}
```

---

## Why

Use `IEnumerable<T>` when you only need to loop through data.

It is good for method parameters when the method only reads items.

```csharp
public void PrintOrders(IEnumerable<Order> orders)
{
    foreach (Order order in orders)
    {
        Console.WriteLine(order.OrderNumber);
    }
}
```

This method can accept:

```csharp
List<Order>
Order[]
HashSet<Order>
IQueryable<Order>
```

Because all of them can be enumerated.

---

## Example

```csharp
public class Order
{
    public string OrderNumber { get; set; } = "";
    public decimal TotalAmount { get; set; }
}
```

```csharp
public decimal CalculateTotal(IEnumerable<Order> orders)
{
    decimal total = 0;

    foreach (Order order in orders)
    {
        total += order.TotalAmount;
    }

    return total;
}
```

Usage:

```csharp
List<Order> orders = new()
{
    new Order { OrderNumber = "ORD-001", TotalAmount = 5000m },
    new Order { OrderNumber = "ORD-002", TotalAmount = 8000m }
};

decimal total = CalculateTotal(orders);

Console.WriteLine(total);
```

---

## Deferred execution

Some `IEnumerable<T>` values are not executed immediately.

Example:

```csharp
IEnumerable<Order> highValueOrders = orders
    .Where(order => order.TotalAmount > 10000m);
```

The filtering happens when you enumerate:

```csharp
foreach (Order order in highValueOrders)
{
    Console.WriteLine(order.OrderNumber);
}
```

---

## Multiple enumeration issue

Be careful with this:

```csharp
public void PrintSummary(IEnumerable<Order> orders)
{
    int count = orders.Count();
    decimal total = orders.Sum(order => order.TotalAmount);

    Console.WriteLine($"Count: {count}, Total: {total}");
}
```

This may enumerate `orders` twice.

Better:

```csharp
public void PrintSummary(IEnumerable<Order> orders)
{
    List<Order> orderList = orders.ToList();

    int count = orderList.Count;
    decimal total = orderList.Sum(order => order.TotalAmount);

    Console.WriteLine($"Count: {count}, Total: {total}");
}
```

---

## Senior-level note

Use `IEnumerable<T>` when you want to expose read-only iteration.

Good:

```csharp
public IEnumerable<Order> GetPendingOrders()
{
    return _orders.Where(order => order.Status == OrderStatus.Pending);
}
```

But if the caller needs count and index access, use:

```csharp
IReadOnlyList<T>
```

Example:

```csharp
public IReadOnlyList<Order> GetOrders()
{
    return _orders;
}
```

---

# 4. ICollection

## What

`ICollection<T>` represents a collection that can be counted and modified.

It supports:

```csharp
Add()
Remove()
Clear()
Contains()
Count
```

Example:

```csharp
ICollection<string> names = new List<string>();

names.Add("Kamal");
names.Add("Nimal");

Console.WriteLine(names.Count);

names.Remove("Kamal");
```

---

## Why

Use `ICollection<T>` when you need collection behavior, but not index-based access.

It is more powerful than `IEnumerable<T>`.

```csharp
IEnumerable<T>  -> only iteration
ICollection<T> -> iteration + add/remove/count
```

---

## Example

```csharp
public class Team
{
    private readonly ICollection<string> _members = new List<string>();

    public void AddMember(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Member name is required.");
        }

        _members.Add(name);
    }

    public int GetMemberCount()
    {
        return _members.Count;
    }
}
```

Usage:

```csharp
Team team = new Team();

team.AddMember("Dinuka");
team.AddMember("Kamal");

Console.WriteLine(team.GetMemberCount());
```

---

## Example with EF Core style entity

```csharp
public class Order
{
    public Guid Id { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
```

```csharp
public class OrderItem
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
}
```

`ICollection<T>` is commonly used in EF Core navigation properties because EF needs to add/remove related entities.

---

## Senior-level note

Use `ICollection<T>` when mutation is required.

But for public read-only exposure, avoid this:

```csharp
public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
```

Because external code can modify it freely.

Better domain style:

```csharp
public class Order
{
    private readonly List<OrderItem> _items = new();

    public IReadOnlyCollection<OrderItem> Items => _items;

    public void AddItem(string productName, decimal price)
    {
        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.");
        }

        _items.Add(new OrderItem
        {
            ProductName = productName,
            Price = price
        });
    }
}
```

---

# 5. IList

## What

`IList<T>` represents a collection that supports index-based access and modification.

It supports:

```csharp
Add()
Remove()
Insert()
RemoveAt()
IndexOf()
this[index]
Count
```

Example:

```csharp
IList<string> names = new List<string>();

names.Add("Kamal");
names.Add("Nimal");

Console.WriteLine(names[0]);

names.Insert(1, "Dinuka");

names.RemoveAt(0);
```

---

## Why

Use `IList<T>` when order and index matter.

Examples:

* ordered steps
* ranked results
* list items where position matters
* UI display order
* processing pipeline steps

---

## Example

```csharp
public class LearningPath
{
    private readonly IList<string> _modules = new List<string>();

    public void AddModule(string moduleName)
    {
        _modules.Add(moduleName);
    }

    public void InsertModule(int index, string moduleName)
    {
        _modules.Insert(index, moduleName);
    }

    public string GetModule(int index)
    {
        return _modules[index];
    }
}
```

Usage:

```csharp
LearningPath path = new LearningPath();

path.AddModule("C# Basics");
path.AddModule("ASP.NET Core");
path.InsertModule(1, "OOP");

Console.WriteLine(path.GetModule(0)); // C# Basics
Console.WriteLine(path.GetModule(1)); // OOP
```

---

## IList vs ICollection vs IEnumerable

```text
IEnumerable<T>
    only foreach

ICollection<T>
    foreach + Count + Add + Remove

IList<T>
    foreach + Count + Add + Remove + index access
```

---

## Senior-level note

Do not expose `IList<T>` unless external code really needs index modification.

Risky:

```csharp
public IList<OrderItem> Items { get; set; } = new List<OrderItem>();
```

This allows:

```csharp
order.Items[0] = anotherItem;
order.Items.RemoveAt(0);
order.Items.Clear();
```

For domain models, prefer:

```csharp
public IReadOnlyList<OrderItem> Items => _items;
```

Use `IList<T>` mainly inside internal logic when index modification is needed.

---

# 6. Dictionary

## What

A `Dictionary<TKey, TValue>` stores key-value pairs.

It allows fast lookup by key.

```csharp
Dictionary<string, string> countries = new();

countries["LK"] = "Sri Lanka";
countries["IN"] = "India";
countries["US"] = "United States";

Console.WriteLine(countries["LK"]);
```

---

## Why

Use `Dictionary` when you need fast lookup.

Examples:

* user by ID
* product by SKU
* settings by key
* permission by role
* cached data
* count by category

---

## Example: customer lookup by ID

```csharp
public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}
```

```csharp
Customer customer1 = new Customer
{
    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
    Name = "Kamal"
};

Customer customer2 = new Customer
{
    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
    Name = "Nimal"
};

Dictionary<Guid, Customer> customersById = new()
{
    [customer1.Id] = customer1,
    [customer2.Id] = customer2
};

Guid searchId = customer1.Id;

Customer customer = customersById[searchId];

Console.WriteLine(customer.Name);
```

---

## Safe lookup with `TryGetValue`

Bad:

```csharp
Customer customer = customersById[unknownId];
```

If key does not exist, it throws exception.

Better:

```csharp
if (customersById.TryGetValue(searchId, out Customer? customer))
{
    Console.WriteLine(customer.Name);
}
else
{
    Console.WriteLine("Customer not found.");
}
```

---

## Example: count orders by status

```csharp
public enum OrderStatus
{
    Pending,
    Paid,
    Cancelled
}
```

```csharp
List<OrderStatus> statuses = new()
{
    OrderStatus.Pending,
    OrderStatus.Paid,
    OrderStatus.Pending,
    OrderStatus.Cancelled,
    OrderStatus.Paid,
    OrderStatus.Paid
};

Dictionary<OrderStatus, int> countByStatus = new();

foreach (OrderStatus status in statuses)
{
    if (!countByStatus.ContainsKey(status))
    {
        countByStatus[status] = 0;
    }

    countByStatus[status]++;
}

foreach (var item in countByStatus)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}
```

---

## Senior-level note

Avoid using `ContainsKey` plus indexer when you can use `TryGetValue`.

Less efficient:

```csharp
if (customersById.ContainsKey(id))
{
    Customer customer = customersById[id];
}
```

Better:

```csharp
if (customersById.TryGetValue(id, out Customer? customer))
{
}
```

Also choose keys carefully.

Good keys:

```csharp
Guid userId
string normalizedEmail
string productSku
int statusCode
```

Be careful with case-sensitive string keys:

```csharp
Dictionary<string, Customer> customersByEmail = new();
```

Better for case-insensitive email lookup:

```csharp
Dictionary<string, Customer> customersByEmail =
    new(StringComparer.OrdinalIgnoreCase);
```

---

# 7. HashSet

## What

A `HashSet<T>` stores unique values.

It does not allow duplicates.

```csharp
HashSet<string> emails = new();

emails.Add("a@example.com");
emails.Add("b@example.com");
emails.Add("a@example.com");

Console.WriteLine(emails.Count); // 2
```

---

## Why

Use `HashSet<T>` when you need:

* uniqueness
* fast existence checks
* duplicate removal
* set operations

---

## Example: remove duplicate tags

```csharp
List<string> tags = new()
{
    "dotnet",
    "csharp",
    "backend",
    "dotnet",
    "api"
};

HashSet<string> uniqueTags = new(tags);

foreach (string tag in uniqueTags)
{
    Console.WriteLine(tag);
}
```

---

## Example: permission checking

```csharp
HashSet<string> userPermissions = new(StringComparer.OrdinalIgnoreCase)
{
    "Order.Read",
    "Order.Create",
    "Order.Approve"
};

bool canApprove = userPermissions.Contains("order.approve");

Console.WriteLine(canApprove); // True
```

---

## Set operations

```csharp
HashSet<string> userAFeatures = new()
{
    "Dashboard",
    "Reports",
    "Orders"
};

HashSet<string> userBFeatures = new()
{
    "Orders",
    "Invoices",
    "Reports"
};
```

Common features:

```csharp
HashSet<string> commonFeatures = new(userAFeatures);

commonFeatures.IntersectWith(userBFeatures);

Console.WriteLine(string.Join(", ", commonFeatures));
```

All features:

```csharp
HashSet<string> allFeatures = new(userAFeatures);

allFeatures.UnionWith(userBFeatures);

Console.WriteLine(string.Join(", ", allFeatures));
```

Features only in user A:

```csharp
HashSet<string> onlyUserA = new(userAFeatures);

onlyUserA.ExceptWith(userBFeatures);

Console.WriteLine(string.Join(", ", onlyUserA));
```

---

## Senior-level note

Use `HashSet<T>` instead of `List<T>` when your main operation is `Contains`.

Less ideal:

```csharp
List<Guid> allowedUserIds = GetAllowedUserIds();

if (allowedUserIds.Contains(userId))
{
}
```

Better for large collections:

```csharp
HashSet<Guid> allowedUserIds = GetAllowedUserIds().ToHashSet();

if (allowedUserIds.Contains(userId))
{
}
```

`HashSet<T>.Contains()` is usually much faster for large collections.

---

# 8. Queue

## What

A `Queue<T>` represents **first-in, first-out** behavior.

First item added is the first item removed.

```text
FIFO = First In, First Out
```

Example:

```csharp
Queue<string> jobs = new();

jobs.Enqueue("Send email");
jobs.Enqueue("Generate invoice");
jobs.Enqueue("Sync report");

string firstJob = jobs.Dequeue();

Console.WriteLine(firstJob); // Send email
```

---

## Why

Use `Queue<T>` when processing order matters.

Examples:

* background jobs
* print queue
* email sending queue
* order processing
* task scheduling
* breadth-first traversal

---

## Queue methods

```csharp
Queue<string> queue = new();

queue.Enqueue("Job 1");
queue.Enqueue("Job 2");

string next = queue.Peek();    // View next item without removing
string item = queue.Dequeue(); // Remove and return next item

int count = queue.Count;
```

---

## Safe dequeue with `TryDequeue`

```csharp
Queue<string> jobs = new();

if (jobs.TryDequeue(out string? job))
{
    Console.WriteLine($"Processing {job}");
}
else
{
    Console.WriteLine("No jobs available.");
}
```

---

## Example: in-memory job processor

```csharp
Queue<string> emailJobs = new();

emailJobs.Enqueue("Send welcome email to kamal@example.com");
emailJobs.Enqueue("Send invoice email to nimal@example.com");
emailJobs.Enqueue("Send reminder email to sunil@example.com");

while (emailJobs.TryDequeue(out string? job))
{
    Console.WriteLine($"Processing: {job}");
}
```

Output:

```text
Processing: Send welcome email to kamal@example.com
Processing: Send invoice email to nimal@example.com
Processing: Send reminder email to sunil@example.com
```

---

## Senior-level note

`Queue<T>` is in-memory only.

For real distributed systems, do not use `Queue<T>` for durable background jobs.

Use proper messaging systems:

```text
Azure Service Bus
RabbitMQ
Kafka
Amazon SQS
Hangfire
Quartz
```

Use `Queue<T>` for local in-memory ordering logic, not cross-service communication.

---

# 9. Stack

## What

A `Stack<T>` represents **last-in, first-out** behavior.

Last item added is the first item removed.

```text
LIFO = Last In, First Out
```

Example:

```csharp
Stack<string> history = new();

history.Push("Open dashboard");
history.Push("Open order details");
history.Push("Edit order");

string lastAction = history.Pop();

Console.WriteLine(lastAction); // Edit order
```

---

## Why

Use `Stack<T>` when the most recent item should be processed first.

Examples:

* undo history
* browser back navigation
* expression parsing
* depth-first traversal
* nested operations
* call stack-like behavior

---

## Stack methods

```csharp
Stack<string> stack = new();

stack.Push("Step 1");
stack.Push("Step 2");

string latest = stack.Peek(); // View latest item without removing
string item = stack.Pop();    // Remove and return latest item

int count = stack.Count;
```

---

## Safe pop with `TryPop`

```csharp
Stack<string> actions = new();

if (actions.TryPop(out string? action))
{
    Console.WriteLine($"Undo: {action}");
}
else
{
    Console.WriteLine("Nothing to undo.");
}
```

---

## Example: undo feature

```csharp
Stack<string> undoHistory = new();

undoHistory.Push("Created order");
undoHistory.Push("Added order item");
undoHistory.Push("Applied discount");

while (undoHistory.TryPop(out string? action))
{
    Console.WriteLine($"Undo action: {action}");
}
```

Output:

```text
Undo action: Applied discount
Undo action: Added order item
Undo action: Created order
```

---

## Senior-level note

`Stack<T>` is useful for temporary in-memory workflows.

But do not use it as permanent history storage.

For real audit/history features, store records in:

```text
Database table
Event store
Audit log
Message log
```

---

# Important Interface Hierarchy

A simple way to remember:

```text
IEnumerable<T>
    Can foreach

ICollection<T>
    Can foreach
    Has Count
    Can Add / Remove

IList<T>
    Can foreach
    Has Count
    Can Add / Remove
    Has index access
```

Example:

```csharp
IEnumerable<string> enumerable;
ICollection<string> collection;
IList<string> list;
```

---

# Choosing the Right Collection

## Use `Array`

When size is fixed.

```csharp
byte[] fileBytes;
int[] fixedScores;
```

---

## Use `List<T>`

When you need a normal dynamic list.

```csharp
List<Order> orders = new();
```

---

## Use `IEnumerable<T>`

When a method only needs to read/loop.

```csharp
public decimal CalculateTotal(IEnumerable<Order> orders)
{
    return orders.Sum(order => order.TotalAmount);
}
```

---

## Use `ICollection<T>`

When you need add/remove/count but not index.

```csharp
public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
```

---

## Use `IList<T>`

When index matters.

```csharp
IList<string> orderedSteps = new List<string>();
```

---

## Use `Dictionary<TKey, TValue>`

When you need fast lookup by key.

```csharp
Dictionary<Guid, Customer> customersById = new();
```

---

## Use `HashSet<T>`

When you need uniqueness or fast `Contains`.

```csharp
HashSet<string> permissions = new();
```

---

## Use `Queue<T>`

When first item must be processed first.

```csharp
Queue<string> jobs = new();
```

---

## Use `Stack<T>`

When last item must be processed first.

```csharp
Stack<string> undoHistory = new();
```

---

# Combined Example

```csharp
public enum OrderStatus
{
    Pending,
    Paid,
    Cancelled
}
```

```csharp
public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
}
```

```csharp
List<Order> orders = new()
{
    new Order
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        OrderNumber = "ORD-001",
        Status = OrderStatus.Pending,
        TotalAmount = 5000m
    },
    new Order
    {
        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        OrderNumber = "ORD-002",
        Status = OrderStatus.Paid,
        TotalAmount = 12000m
    },
    new Order
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        OrderNumber = "ORD-003",
        Status = OrderStatus.Paid,
        TotalAmount = 8000m
    }
};
```

## IEnumerable example

```csharp
IEnumerable<Order> paidOrders = orders
    .Where(order => order.Status == OrderStatus.Paid);

foreach (Order order in paidOrders)
{
    Console.WriteLine(order.OrderNumber);
}
```

## Dictionary example

```csharp
Dictionary<Guid, Order> ordersById = orders.ToDictionary(order => order.Id);

Guid orderId = Guid.Parse("22222222-2222-2222-2222-222222222222");

if (ordersById.TryGetValue(orderId, out Order? order))
{
    Console.WriteLine($"Found order: {order.OrderNumber}");
}
```

## HashSet example

```csharp
HashSet<OrderStatus> finalStatuses = new()
{
    OrderStatus.Paid,
    OrderStatus.Cancelled
};

bool isFinalStatus = finalStatuses.Contains(OrderStatus.Paid);

Console.WriteLine(isFinalStatus);
```

## Queue example

```csharp
Queue<Order> processingQueue = new();

foreach (Order pendingOrder in orders.Where(order => order.Status == OrderStatus.Pending))
{
    processingQueue.Enqueue(pendingOrder);
}

while (processingQueue.TryDequeue(out Order? pendingOrder))
{
    Console.WriteLine($"Processing order: {pendingOrder.OrderNumber}");
}
```

## Stack example

```csharp
Stack<string> undoHistory = new();

undoHistory.Push("Created order ORD-001");
undoHistory.Push("Added item to ORD-001");
undoHistory.Push("Applied discount to ORD-001");

while (undoHistory.TryPop(out string? action))
{
    Console.WriteLine($"Undo: {action}");
}
```

---

# Senior Interview Summary

| Type                    | Main purpose      |  Allows duplicates? |                     Ordered? |        Key access? |
| ----------------------- | ----------------- | ------------------: | ---------------------------: | -----------------: |
| Array                   | Fixed-size values |                 Yes |                          Yes |              Index |
| List<T>                 | Dynamic list      |                 Yes |                          Yes |              Index |
| IEnumerable<T>          | Read/iterate only |             Depends |                      Depends |                 No |
| ICollection<T>          | Add/remove/count  |             Depends |                      Depends | No index guarantee |
| IList<T>                | Add/remove/index  |                 Yes |                          Yes |              Index |
| Dictionary<TKey,TValue> | Key-value lookup  | Keys no, values yes |    Not for business ordering |                Key |
| HashSet<T>              | Unique values     |                  No | No guaranteed business order |       Value lookup |
| Queue<T>                | FIFO processing   |                 Yes |             Processing order |                 No |
| Stack<T>                | LIFO processing   |                 Yes |     Reverse processing order |                 No |

---

# Very Short Rule

```text
Need fixed size?
    Array

Need normal dynamic list?
    List<T>

Only need foreach?
    IEnumerable<T>

Need add/remove/count?
    ICollection<T>

Need index access?
    IList<T> or List<T>

Need lookup by key?
    Dictionary<TKey, TValue>

Need unique values?
    HashSet<T>

Need first-in-first-out?
    Queue<T>

Need last-in-first-out?
    Stack<T>
```

For senior .NET development, the most important part is not memorizing collection names. It is choosing the collection that correctly expresses **intent**, **performance needs**, and **mutation rules**.
