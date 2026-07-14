# Delegates and Events in C# with Examples

---

# 1. Delegates

## What

A **delegate** is a type that can hold a reference to a method.

Simple meaning:

```text
A delegate is like a variable that stores a method.
```

The method must match the delegate signature.

---

## Example

```csharp
public delegate void NotificationDelegate(string message);
```

This delegate can point to any method that:

```text
returns void
accepts string
```

Example method:

```csharp
public static void SendEmail(string message)
{
    Console.WriteLine($"Email sent: {message}");
}
```

Usage:

```csharp
NotificationDelegate notification = SendEmail;

notification("Order placed successfully.");
```

Output:

```text
Email sent: Order placed successfully.
```

---

## Why

Delegates are useful when you want to pass behavior as data.

Common use cases:

```text
Callbacks
Event handling
Validation rules
Sorting logic
Filtering logic
Retry callbacks
Notification pipelines
Plugin-style behavior
```

---

## Real-world example: payment callback

```csharp
public delegate void PaymentCompletedDelegate(decimal amount);
```

```csharp
public class PaymentService
{
    public void Pay(decimal amount, PaymentCompletedDelegate onPaymentCompleted)
    {
        Console.WriteLine($"Processing payment: {amount:N2}");

        // Payment completed
        onPaymentCompleted(amount);
    }
}
```

```csharp
public static void SendReceipt(decimal amount)
{
    Console.WriteLine($"Receipt sent for amount: {amount:N2}");
}
```

Usage:

```csharp
PaymentService paymentService = new PaymentService();

paymentService.Pay(5000m, SendReceipt);
```

Output:

```text
Processing payment: 5,000.00
Receipt sent for amount: 5,000.00
```

---

## Senior-level note

Delegates decouple the method caller from the actual behavior.

Without delegate:

```csharp
public class PaymentService
{
    public void Pay(decimal amount)
    {
        Console.WriteLine($"Processing payment: {amount:N2}");

        SendReceipt(amount);
    }

    private void SendReceipt(decimal amount)
    {
        Console.WriteLine($"Receipt sent for amount: {amount:N2}");
    }
}
```

Problem:

```text
PaymentService is directly coupled to receipt sending.
```

With delegate:

```csharp
public void Pay(decimal amount, PaymentCompletedDelegate onPaymentCompleted)
{
    Console.WriteLine($"Processing payment: {amount:N2}");

    onPaymentCompleted(amount);
}
```

Now the caller decides what happens after payment.

---

# 2. Multicast Delegates

## What

A **multicast delegate** can hold references to multiple methods.

When invoked, it calls all assigned methods in order.

---

## Example

```csharp
public delegate void OrderNotificationDelegate(string orderNumber);
```

Methods:

```csharp
public static void SendEmail(string orderNumber)
{
    Console.WriteLine($"Email sent for order {orderNumber}");
}

public static void SendSms(string orderNumber)
{
    Console.WriteLine($"SMS sent for order {orderNumber}");
}

public static void WriteAuditLog(string orderNumber)
{
    Console.WriteLine($"Audit log written for order {orderNumber}");
}
```

Usage:

```csharp
OrderNotificationDelegate notifications = SendEmail;

notifications += SendSms;
notifications += WriteAuditLog;

notifications("ORD-001");
```

Output:

```text
Email sent for order ORD-001
SMS sent for order ORD-001
Audit log written for order ORD-001
```

---

## Removing a method

```csharp
notifications -= SendSms;

notifications("ORD-002");
```

Output:

```text
Email sent for order ORD-002
Audit log written for order ORD-002
```

---

## Why

Multicast delegates are useful when one action should trigger multiple operations.

Examples:

```text
Order placed -> send email, send SMS, write audit log
User registered -> send welcome email, create profile, notify admin
Payment completed -> send receipt, update ledger, publish message
```

---

## Senior-level note

Be careful with exceptions.

If one method throws an exception, the remaining methods may not run.

Example:

```csharp
public static void SendEmail(string orderNumber)
{
    Console.WriteLine($"Email sent for order {orderNumber}");
}

public static void SendSms(string orderNumber)
{
    throw new Exception("SMS provider failed.");
}

public static void WriteAuditLog(string orderNumber)
{
    Console.WriteLine($"Audit log written for order {orderNumber}");
}
```

```csharp
OrderNotificationDelegate notifications = SendEmail;
notifications += SendSms;
notifications += WriteAuditLog;

notifications("ORD-001");
```

`WriteAuditLog` may not execute because `SendSms` failed.

Safer manual invocation:

```csharp
foreach (OrderNotificationDelegate handler in notifications.GetInvocationList())
{
    try
    {
        handler("ORD-001");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Handler failed: {ex.Message}");
    }
}
```

---

# 3. Action

## What

`Action` is a built-in delegate type that returns `void`.

You do not need to define a custom delegate.

---

## Example

```csharp
Action<string> logMessage = message =>
{
    Console.WriteLine($"LOG: {message}");
};

logMessage("Application started.");
```

Output:

```text
LOG: Application started.
```

---

## `Action` with multiple parameters

```csharp
Action<string, decimal> printOrder = (orderNumber, amount) =>
{
    Console.WriteLine($"Order {orderNumber}, Amount: {amount:N2}");
};

printOrder("ORD-001", 5000m);
```

---

## Why

Use `Action` when:

```text
You need to pass a method
The method returns void
You do not need a custom delegate name
```

---

## Real-world example: retry with logging callback

```csharp
public class RetryService
{
    public void Execute(Action operation, Action<string> log)
    {
        try
        {
            operation();
            log("Operation completed successfully.");
        }
        catch (Exception ex)
        {
            log($"Operation failed: {ex.Message}");
        }
    }
}
```

Usage:

```csharp
RetryService retryService = new RetryService();

retryService.Execute(
    operation: () =>
    {
        Console.WriteLine("Calling external API...");
    },
    log: message =>
    {
        Console.WriteLine($"LOG: {message}");
    });
```

Output:

```text
Calling external API...
LOG: Operation completed successfully.
```

---

## Senior-level note

`Action` is good for short, local behavior.

Good:

```csharp
Action<string> log = message => Console.WriteLine(message);
```

But for important domain concepts, a named delegate or interface can be clearer.

Less expressive:

```csharp
Action<decimal> callback
```

More expressive:

```csharp
PaymentCompletedDelegate onPaymentCompleted
```

or:

```csharp
IPaymentNotificationService
```

---

# 4. Func

## What

`Func` is a built-in delegate type that returns a value.

The last type parameter is the return type.

---

## Example

```csharp
Func<int, int, int> add = (a, b) => a + b;

int result = add(10, 20);

Console.WriteLine(result);
```

Output:

```text
30
```

Here:

```csharp
Func<int, int, int>
```

means:

```text
accepts int
accepts int
returns int
```

---

## Example with one input

```csharp
Func<decimal, decimal> calculateVat = amount => amount * 0.15m;

decimal vat = calculateVat(10000m);

Console.WriteLine(vat);
```

---

## Example with no input

```csharp
Func<DateTime> getCurrentTime = () => DateTime.UtcNow;

Console.WriteLine(getCurrentTime());
```

---

## Why

Use `Func` when:

```text
You need to pass a calculation
You need to pass a selector
You need to return a value
You want flexible reusable logic
```

---

## Real-world example: discount calculation

```csharp
public class DiscountService
{
    public decimal ApplyDiscount(decimal amount, Func<decimal, decimal> discountRule)
    {
        decimal discount = discountRule(amount);

        return amount - discount;
    }
}
```

Usage:

```csharp
DiscountService discountService = new DiscountService();

decimal finalAmount = discountService.ApplyDiscount(
    amount: 10000m,
    discountRule: amount => amount * 0.10m);

Console.WriteLine(finalAmount);
```

Output:

```text
9000
```

---

## Real-world example: dynamic pricing rule

```csharp
Func<decimal, bool, decimal> calculateFinalPrice = (amount, isVipCustomer) =>
{
    if (isVipCustomer)
    {
        return amount * 0.80m;
    }

    return amount;
};

decimal price = calculateFinalPrice(10000m, true);

Console.WriteLine(price);
```

Output:

```text
8000
```

---

## Senior-level note

`Func` is heavily used in LINQ.

Example:

```csharp
List<Order> orders = new()
{
    new Order { OrderNumber = "ORD-001", TotalAmount = 5000m },
    new Order { OrderNumber = "ORD-002", TotalAmount = 15000m }
};

Func<Order, bool> highValueFilter = order => order.TotalAmount > 10000m;

IEnumerable<Order> highValueOrders = orders.Where(highValueFilter);
```

`Where` expects:

```csharp
Func<T, bool>
```

That means:

```text
input: item
output: true or false
```

---

# 5. Predicate

## What

`Predicate<T>` is a built-in delegate that accepts one value and returns `bool`.

```csharp
Predicate<T>
```

is similar to:

```csharp
Func<T, bool>
```

---

## Example

```csharp
Predicate<int> isAdultAge = age => age >= 18;

Console.WriteLine(isAdultAge(20)); // True
Console.WriteLine(isAdultAge(15)); // False
```

---

## Why

Use `Predicate<T>` when the method represents a yes/no condition.

Examples:

```text
Is order paid?
Is user active?
Is product available?
Is customer VIP?
Is amount valid?
```

---

## Real-world example: order validation

```csharp
public class Order
{
    public string OrderNumber { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
}
```

```csharp
Predicate<Order> isPaidOrder = order => order.IsPaid;

Order order = new Order
{
    OrderNumber = "ORD-001",
    TotalAmount = 5000m,
    IsPaid = true
};

Console.WriteLine(isPaidOrder(order));
```

Output:

```text
True
```

---

## Example with `List<T>.Find`

```csharp
List<Order> orders = new()
{
    new Order { OrderNumber = "ORD-001", TotalAmount = 5000m, IsPaid = false },
    new Order { OrderNumber = "ORD-002", TotalAmount = 15000m, IsPaid = true }
};

Predicate<Order> isHighValueOrder = order => order.TotalAmount > 10000m;

Order? highValueOrder = orders.Find(isHighValueOrder);

Console.WriteLine(highValueOrder?.OrderNumber);
```

Output:

```text
ORD-002
```

---

## Predicate vs Func

```csharp
Predicate<Order> predicate = order => order.IsPaid;

Func<Order, bool> func = order => order.IsPaid;
```

Both work similarly.

But `Predicate<T>` communicates intent more clearly:

```text
This is a condition.
```

---

## Senior-level note

In LINQ, you mostly use:

```csharp
Func<T, bool>
```

Example:

```csharp
orders.Where(order => order.IsPaid);
```

In older APIs or list methods, you may see:

```csharp
Predicate<T>
```

Example:

```csharp
orders.Find(order => order.IsPaid);
```

---

# 6. Events

## What

An **event** is a safe wrapper around a delegate.

Events allow a class to notify other classes when something happens.

Simple meaning:

```text
An event says: "Something happened."
Subscribers can react to it.
```

---

## Example

```csharp
public class OrderService
{
    public event Action<string>? OrderPlaced;

    public void PlaceOrder(string orderNumber)
    {
        Console.WriteLine($"Order placed: {orderNumber}");

        OrderPlaced?.Invoke(orderNumber);
    }
}
```

Subscriber:

```csharp
public class EmailService
{
    public void SendOrderEmail(string orderNumber)
    {
        Console.WriteLine($"Email sent for order {orderNumber}");
    }
}
```

Usage:

```csharp
OrderService orderService = new OrderService();
EmailService emailService = new EmailService();

orderService.OrderPlaced += emailService.SendOrderEmail;

orderService.PlaceOrder("ORD-001");
```

Output:

```text
Order placed: ORD-001
Email sent for order ORD-001
```

---

## Why

Events are useful for notification-style design.

Examples:

```text
Order placed
Payment completed
User registered
File uploaded
Report generated
Stock level changed
Button clicked
Message received
```

---

## Why events are safer than public delegates

Public delegate field:

```csharp
public Action<string>? OrderPlaced;
```

Problem:

```csharp
orderService.OrderPlaced = null;
```

External code can remove all subscribers.

External code can also invoke it:

```csharp
orderService.OrderPlaced?.Invoke("ORD-FAKE");
```

That is dangerous.

Event:

```csharp
public event Action<string>? OrderPlaced;
```

External code can only subscribe/unsubscribe:

```csharp
orderService.OrderPlaced += handler;
orderService.OrderPlaced -= handler;
```

External code cannot directly invoke the event.

Only the owning class can invoke it.

---

## Senior-level note

Events are best for in-process notifications.

Good:

```text
Inside the same application
UI events
Domain model notifications
Local application hooks
```

For distributed systems, do not use C# events for communication between services.

Use:

```text
Azure Service Bus
RabbitMQ
Kafka
Amazon SQS
MassTransit
NServiceBus
```

C# events are memory-local, not distributed.

---

# 7. Event Handlers

## What

An **event handler** is a method that responds to an event.

C# has a common event pattern:

```csharp
object? sender
EventArgs e
```

or:

```csharp
object? sender
SomeCustomEventArgs e
```

---

## Built-in `EventHandler`

```csharp
public event EventHandler? OrderPlaced;
```

Raise event:

```csharp
OrderPlaced?.Invoke(this, EventArgs.Empty);
```

---

## Example with `EventHandler`

```csharp
public class OrderService
{
    public event EventHandler? OrderPlaced;

    public void PlaceOrder()
    {
        Console.WriteLine("Order placed.");

        OrderPlaced?.Invoke(this, EventArgs.Empty);
    }
}
```

Subscriber:

```csharp
public class AuditService
{
    public void OnOrderPlaced(object? sender, EventArgs e)
    {
        Console.WriteLine("Audit log written for order placed event.");
    }
}
```

Usage:

```csharp
OrderService orderService = new OrderService();
AuditService auditService = new AuditService();

orderService.OrderPlaced += auditService.OnOrderPlaced;

orderService.PlaceOrder();
```

Output:

```text
Order placed.
Audit log written for order placed event.
```

---

## Custom EventArgs

Use custom event args when you need to pass event data.

```csharp
public sealed class OrderPlacedEventArgs : EventArgs
{
    public string OrderNumber { get; }
    public decimal TotalAmount { get; }

    public OrderPlacedEventArgs(string orderNumber, decimal totalAmount)
    {
        OrderNumber = orderNumber;
        TotalAmount = totalAmount;
    }
}
```

---

## Event with custom EventArgs

```csharp
public class OrderService
{
    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    public void PlaceOrder(string orderNumber, decimal totalAmount)
    {
        Console.WriteLine($"Order placed: {orderNumber}");

        OrderPlaced?.Invoke(
            this,
            new OrderPlacedEventArgs(orderNumber, totalAmount));
    }
}
```

Handlers:

```csharp
public class EmailService
{
    public void OnOrderPlaced(object? sender, OrderPlacedEventArgs e)
    {
        Console.WriteLine(
            $"Email sent for order {e.OrderNumber}, amount {e.TotalAmount:N2}");
    }
}
```

```csharp
public class AuditService
{
    public void OnOrderPlaced(object? sender, OrderPlacedEventArgs e)
    {
        Console.WriteLine(
            $"Audit log: Order {e.OrderNumber} placed with amount {e.TotalAmount:N2}");
    }
}
```

Usage:

```csharp
OrderService orderService = new OrderService();

EmailService emailService = new EmailService();
AuditService auditService = new AuditService();

orderService.OrderPlaced += emailService.OnOrderPlaced;
orderService.OrderPlaced += auditService.OnOrderPlaced;

orderService.PlaceOrder("ORD-001", 15000m);
```

Output:

```text
Order placed: ORD-001
Email sent for order ORD-001, amount 15,000.00
Audit log: Order ORD-001 placed with amount 15,000.00
```

---

## Unsubscribe event handler

```csharp
orderService.OrderPlaced -= emailService.OnOrderPlaced;
```

After unsubscribe, that handler will not run.

---

# Full Simple Console Example

```csharp
public sealed class OrderPlacedEventArgs : EventArgs
{
    public string OrderNumber { get; }
    public decimal TotalAmount { get; }

    public OrderPlacedEventArgs(string orderNumber, decimal totalAmount)
    {
        OrderNumber = orderNumber;
        TotalAmount = totalAmount;
    }
}
```

```csharp
public class OrderService
{
    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    public void PlaceOrder(string orderNumber, decimal totalAmount)
    {
        Console.WriteLine($"Order placed: {orderNumber}");

        OnOrderPlaced(new OrderPlacedEventArgs(orderNumber, totalAmount));
    }

    protected virtual void OnOrderPlaced(OrderPlacedEventArgs e)
    {
        OrderPlaced?.Invoke(this, e);
    }
}
```

```csharp
public class EmailService
{
    public void OnOrderPlaced(object? sender, OrderPlacedEventArgs e)
    {
        Console.WriteLine($"Email sent for order {e.OrderNumber}");
    }
}
```

```csharp
public class SmsService
{
    public void OnOrderPlaced(object? sender, OrderPlacedEventArgs e)
    {
        Console.WriteLine($"SMS sent for order {e.OrderNumber}");
    }
}
```

```csharp
public class AuditService
{
    public void OnOrderPlaced(object? sender, OrderPlacedEventArgs e)
    {
        Console.WriteLine(
            $"Audit: Order {e.OrderNumber}, Amount: {e.TotalAmount:N2}");
    }
}
```

```csharp
OrderService orderService = new OrderService();

EmailService emailService = new EmailService();
SmsService smsService = new SmsService();
AuditService auditService = new AuditService();

orderService.OrderPlaced += emailService.OnOrderPlaced;
orderService.OrderPlaced += smsService.OnOrderPlaced;
orderService.OrderPlaced += auditService.OnOrderPlaced;

orderService.PlaceOrder("ORD-001", 25000m);
```

Output:

```text
Order placed: ORD-001
Email sent for order ORD-001
SMS sent for order ORD-001
Audit: Order ORD-001, Amount: 25,000.00
```

---

# Delegates vs Events

## Delegate

A delegate stores method references.

```csharp
public delegate void Notify(string message);
```

```csharp
Notify notify = SendEmail;
notify("Hello");
```

External code can invoke a delegate variable if it has access.

---

## Event

An event is built on top of a delegate, but it restricts access.

```csharp
public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;
```

External code can subscribe:

```csharp
orderService.OrderPlaced += emailService.OnOrderPlaced;
```

External code can unsubscribe:

```csharp
orderService.OrderPlaced -= emailService.OnOrderPlaced;
```

But external code cannot invoke:

```csharp
orderService.OrderPlaced?.Invoke(...); // Not allowed outside the class
```

---

# Action vs Func vs Predicate

## Action

Returns `void`.

```csharp
Action<string> print = message => Console.WriteLine(message);

print("Hello");
```

Use when:

```text
Do something, return nothing.
```

---

## Func

Returns a value.

```csharp
Func<int, int, int> add = (a, b) => a + b;

int result = add(10, 20);
```

Use when:

```text
Calculate something and return result.
```

---

## Predicate

Returns `bool`.

```csharp
Predicate<int> isAdult = age => age >= 18;

bool result = isAdult(20);
```

Use when:

```text
Check condition.
```

---

# Combined Real-World Example

```csharp
public class Payment
{
    public string PaymentNumber { get; set; } = "";
    public decimal Amount { get; set; }
    public bool IsSuccessful { get; set; }
}
```

## Func: calculate fee

```csharp
Func<decimal, decimal> calculateGatewayFee = amount => amount * 0.025m;

decimal fee = calculateGatewayFee(10000m);

Console.WriteLine($"Gateway fee: {fee:N2}");
```

## Predicate: check successful payment

```csharp
Predicate<Payment> isSuccessfulPayment = payment => payment.IsSuccessful;

Payment payment = new Payment
{
    PaymentNumber = "PAY-001",
    Amount = 10000m,
    IsSuccessful = true
};

Console.WriteLine(isSuccessfulPayment(payment));
```

## Action: print receipt

```csharp
Action<Payment> printReceipt = payment =>
{
    Console.WriteLine(
        $"Receipt: {payment.PaymentNumber}, Amount: {payment.Amount:N2}");
};

printReceipt(payment);
```

## Event: notify payment completed

```csharp
public sealed class PaymentCompletedEventArgs : EventArgs
{
    public string PaymentNumber { get; }
    public decimal Amount { get; }

    public PaymentCompletedEventArgs(string paymentNumber, decimal amount)
    {
        PaymentNumber = paymentNumber;
        Amount = amount;
    }
}
```

```csharp
public class PaymentService
{
    public event EventHandler<PaymentCompletedEventArgs>? PaymentCompleted;

    public void CompletePayment(Payment payment)
    {
        Console.WriteLine($"Payment completed: {payment.PaymentNumber}");

        PaymentCompleted?.Invoke(
            this,
            new PaymentCompletedEventArgs(payment.PaymentNumber, payment.Amount));
    }
}
```

```csharp
public class LedgerService
{
    public void OnPaymentCompleted(object? sender, PaymentCompletedEventArgs e)
    {
        Console.WriteLine($"Ledger updated for payment {e.PaymentNumber}");
    }
}
```

Usage:

```csharp
PaymentService paymentService = new PaymentService();
LedgerService ledgerService = new LedgerService();

paymentService.PaymentCompleted += ledgerService.OnPaymentCompleted;

paymentService.CompletePayment(payment);
```

Output:

```text
Payment completed: PAY-001
Ledger updated for payment PAY-001
```

---

# Senior Interview Summary

| Concept            | Meaning                           | Common use                        |
| ------------------ | --------------------------------- | --------------------------------- |
| Delegate           | Type-safe method reference        | Callback, custom behavior         |
| Multicast Delegate | Delegate with multiple methods    | Notify multiple actions           |
| Action             | Built-in delegate returning void  | Logging, callbacks                |
| Func               | Built-in delegate returning value | Calculation, selection, filtering |
| Predicate          | Built-in delegate returning bool  | Condition checking                |
| Event              | Safe notification mechanism       | Something happened                |
| Event Handler      | Method that responds to event     | React to event                    |

---

# Very Short Rule

```text
Delegate:
    Stores a method reference.

Multicast delegate:
    Stores multiple method references.

Action:
    Method returns void.

Func:
    Method returns a value.

Predicate:
    Method returns bool.

Event:
    Notification built on delegate.

Event handler:
    Method that runs when event is raised.
```

For senior .NET development, the important part is understanding that delegates and events support **decoupling**. The class that triggers something does not need to know exactly who will respond.
