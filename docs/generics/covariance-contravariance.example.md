Below is a **simpler real-world example** than the event project.

We will use an **HR system**.

Main idea:

```text
Manager is an Employee
```

So:

```csharp
public sealed class Manager : Employee
```

---

# Simple Real-World Problem

We have:

1. A data source that returns only managers.
2. A dashboard that can display employees.
3. A validator that validates any employee.
4. A manager salary service that validates managers before calculating bonus.

This gives us:

| Concept        | Simple use case                                              |
| -------------- | ------------------------------------------------------------ |
| Covariance     | A manager data source can be used as an employee data source |
| Contravariance | An employee validator can be used as a manager validator     |

---

# Simple Rule First

```text
Covariance = out = returns data
Contravariance = in = accepts data
```

```text
Manager is an Employee
```

So:

```text
IReadOnlyDataSource<Manager>
can become
IReadOnlyDataSource<Employee>
```

Because it only returns data.

And:

```text
IValidator<Employee>
can become
IValidator<Manager>
```

Because it only accepts data.

---

# Project Structure

```text
VarianceSimpleHrDemo/
│
├── VarianceSimpleHrDemo.csproj
├── Program.cs
│
├── Domain/
│   ├── Employee.cs
│   └── Manager.cs
│
├── Application/
│   ├── IReadOnlyDataSource.cs
│   ├── IValidator.cs
│   ├── ValidationResult.cs
│   ├── EmployeeDashboard.cs
│   └── ManagerBonusService.cs
│
├── Infrastructure/
│   ├── InMemoryManagerDataSource.cs
│   └── Validators/
│       ├── EmployeeBasicValidator.cs
│       └── ManagerTeamValidator.cs
│
└── Support/
    └── Test.cs
```

---

# 1. `VarianceSimpleHrDemo.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
```

---

# 2. `Domain/Employee.cs`

```csharp
namespace VarianceSimpleHrDemo.Domain;

public class Employee
{
    public Guid Id { get; }
    public string Name { get; }
    public string Department { get; }
    public decimal Salary { get; }

    public Employee(
        Guid id,
        string name,
        string department,
        decimal salary)
    {
        Id = id;
        Name = name;
        Department = department;
        Salary = salary;
    }
}
```

---

# 3. `Domain/Manager.cs`

```csharp
namespace VarianceSimpleHrDemo.Domain;

public sealed class Manager : Employee
{
    public int TeamSize { get; }

    public Manager(
        Guid id,
        string name,
        string department,
        decimal salary,
        int teamSize)
        : base(id, name, department, salary)
    {
        TeamSize = teamSize;
    }
}
```

Important relationship:

```csharp
Manager : Employee
```

That means:

```text
Manager is an Employee
```

---

# 4. `Application/IReadOnlyDataSource.cs`

This is the **covariance** interface.

```csharp
namespace VarianceSimpleHrDemo.Application;

public interface IReadOnlyDataSource<out T>
{
    IReadOnlyList<T> GetAll();
}
```

Important part:

```csharp
out T
```

Meaning:

```text
This interface only returns T.
It does not accept T as input.
```

So this is safe:

```csharp
IReadOnlyDataSource<Manager> managers
```

can be used as:

```csharp
IReadOnlyDataSource<Employee> employees
```

---

# 5. `Application/IValidator.cs`

This is the **contravariance** interface.

```csharp
namespace VarianceSimpleHrDemo.Application;

public interface IValidator<in T>
{
    ValidationResult Validate(T item);
}
```

Important part:

```csharp
in T
```

Meaning:

```text
This interface accepts T as input.
It does not return T.
```

So this is safe:

```csharp
IValidator<Employee>
```

can be used as:

```csharp
IValidator<Manager>
```

Because a validator that can validate any employee can also validate a manager.

---

# 6. `Application/ValidationResult.cs`

```csharp
namespace VarianceSimpleHrDemo.Application;

public sealed class ValidationResult
{
    public bool IsValid { get; }
    public string Message { get; }

    private ValidationResult(bool isValid, string message)
    {
        IsValid = isValid;
        Message = message;
    }

    public static ValidationResult Success()
    {
        return new ValidationResult(true, "Valid");
    }

    public static ValidationResult Fail(string message)
    {
        return new ValidationResult(false, message);
    }
}
```

---

# 7. `Application/EmployeeDashboard.cs`

This dashboard works with employees.

```csharp
using VarianceSimpleHrDemo.Domain;

namespace VarianceSimpleHrDemo.Application;

public sealed class EmployeeDashboard
{
    public int PrintEmployees(IReadOnlyDataSource<Employee> employeeDataSource)
    {
        Console.WriteLine("=== Employee Dashboard ===");

        IReadOnlyList<Employee> employees = employeeDataSource.GetAll();

        foreach (Employee employee in employees)
        {
            Console.WriteLine(
                $"Employee: {employee.Name}, " +
                $"Department: {employee.Department}, " +
                $"Salary: {employee.Salary:N2}");
        }

        Console.WriteLine();

        return employees.Count;
    }
}
```

This class expects:

```csharp
IReadOnlyDataSource<Employee>
```

But later we will pass:

```csharp
IReadOnlyDataSource<Manager>
```

That is covariance.

---

# 8. `Application/ManagerBonusService.cs`

This service calculates manager bonuses.

```csharp
using VarianceSimpleHrDemo.Domain;

namespace VarianceSimpleHrDemo.Application;

public sealed class ManagerBonusService
{
    private readonly IReadOnlyList<IValidator<Manager>> _validators;

    public ManagerBonusService(IReadOnlyList<IValidator<Manager>> validators)
    {
        _validators = validators;
    }

    public decimal CalculateBonus(Manager manager)
    {
        foreach (IValidator<Manager> validator in _validators)
        {
            ValidationResult result = validator.Validate(manager);

            if (!result.IsValid)
            {
                throw new InvalidOperationException(result.Message);
            }
        }

        decimal bonusRate = manager.TeamSize >= 10
            ? 0.20m
            : 0.10m;

        return manager.Salary * bonusRate;
    }
}
```

This service expects validators for:

```csharp
IValidator<Manager>
```

But later we will pass an employee validator:

```csharp
IValidator<Employee>
```

That is contravariance.

---

# 9. `Infrastructure/InMemoryManagerDataSource.cs`

This data source returns managers only.

```csharp
using VarianceSimpleHrDemo.Application;
using VarianceSimpleHrDemo.Domain;

namespace VarianceSimpleHrDemo.Infrastructure;

public sealed class InMemoryManagerDataSource : IReadOnlyDataSource<Manager>
{
    private readonly IReadOnlyList<Manager> _managers;

    public InMemoryManagerDataSource(IReadOnlyList<Manager> managers)
    {
        _managers = managers;
    }

    public IReadOnlyList<Manager> GetAll()
    {
        return _managers;
    }
}
```

This implements:

```csharp
IReadOnlyDataSource<Manager>
```

Because it returns managers.

---

# 10. `Infrastructure/Validators/EmployeeBasicValidator.cs`

This validator validates any employee.

```csharp
using VarianceSimpleHrDemo.Application;
using VarianceSimpleHrDemo.Domain;

namespace VarianceSimpleHrDemo.Infrastructure.Validators;

public sealed class EmployeeBasicValidator : IValidator<Employee>
{
    public ValidationResult Validate(Employee employee)
    {
        if (employee.Id == Guid.Empty)
        {
            return ValidationResult.Fail("Employee Id is required.");
        }

        if (string.IsNullOrWhiteSpace(employee.Name))
        {
            return ValidationResult.Fail("Employee name is required.");
        }

        if (string.IsNullOrWhiteSpace(employee.Department))
        {
            return ValidationResult.Fail("Employee department is required.");
        }

        if (employee.Salary <= 0)
        {
            return ValidationResult.Fail("Employee salary must be greater than zero.");
        }

        return ValidationResult.Success();
    }
}
```

This implements:

```csharp
IValidator<Employee>
```

But because of contravariance, we can use it as:

```csharp
IValidator<Manager>
```

Why?

Because every manager is also an employee.

---

# 11. `Infrastructure/Validators/ManagerTeamValidator.cs`

This validator is manager-specific.

```csharp
using VarianceSimpleHrDemo.Application;
using VarianceSimpleHrDemo.Domain;

namespace VarianceSimpleHrDemo.Infrastructure.Validators;

public sealed class ManagerTeamValidator : IValidator<Manager>
{
    public ValidationResult Validate(Manager manager)
    {
        if (manager.TeamSize <= 0)
        {
            return ValidationResult.Fail("Manager team size must be greater than zero.");
        }

        return ValidationResult.Success();
    }
}
```

This only works with:

```csharp
Manager
```

Because `TeamSize` exists only in `Manager`.

---

# 12. `Support/Test.cs`

```csharp
namespace VarianceSimpleHrDemo.Support;

public static class Test
{
    public static void Equal<T>(T expected, T actual, string testName)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(
                $"FAILED: {testName}. Expected: {expected}, Actual: {actual}");
        }

        Console.WriteLine($"PASSED: {testName}");
    }

    public static void True(bool condition, string testName)
    {
        if (!condition)
        {
            throw new InvalidOperationException($"FAILED: {testName}");
        }

        Console.WriteLine($"PASSED: {testName}");
    }
}
```

---

# 13. `Program.cs`

```csharp
using VarianceSimpleHrDemo.Application;
using VarianceSimpleHrDemo.Domain;
using VarianceSimpleHrDemo.Infrastructure;
using VarianceSimpleHrDemo.Infrastructure.Validators;
using VarianceSimpleHrDemo.Support;

namespace VarianceSimpleHrDemo;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("C# Covariance and Contravariance - Simple HR Demo");
        Console.WriteLine();

        IReadOnlyList<Manager> managers = CreateManagers();

        RunCovarianceExample(managers);

        RunContravarianceExample(managers);

        Console.WriteLine();
        Console.WriteLine("All validations completed successfully.");
    }

    private static IReadOnlyList<Manager> CreateManagers()
    {
        return new List<Manager>
        {
            new Manager(
                id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                name: "Dinuka",
                department: "Engineering",
                salary: 500000m,
                teamSize: 12),

            new Manager(
                id: Guid.Parse("22222222-2222-2222-2222-222222222222"),
                name: "Kamal",
                department: "Quality Assurance",
                salary: 300000m,
                teamSize: 5)
        };
    }

    private static void RunCovarianceExample(IReadOnlyList<Manager> managers)
    {
        Console.WriteLine("======================================");
        Console.WriteLine("COVARIANCE EXAMPLE");
        Console.WriteLine("======================================");

        IReadOnlyDataSource<Manager> managerDataSource =
            new InMemoryManagerDataSource(managers);

        /*
            Covariance:

            IReadOnlyDataSource<out T> only returns T.

            Because Manager is an Employee,
            a data source that returns Manager can be used as
            a data source that returns Employee.
        */

        IReadOnlyDataSource<Employee> employeeDataSource = managerDataSource;

        EmployeeDashboard dashboard = new EmployeeDashboard();

        int printedCount = dashboard.PrintEmployees(employeeDataSource);

        Test.Equal(
            expected: 2,
            actual: printedCount,
            testName: "Dashboard printed 2 employees from manager data source");

        Test.True(
            condition: employeeDataSource.GetAll().All(employee => employee is Employee),
            testName: "All managers are safely treated as employees");

        Console.WriteLine();

        /*
            This is NOT allowed:

            List<Employee> employees = new List<Manager>();

            Why?

            Because List<T> allows adding items.
            If this was allowed, we could add a normal Employee into a List<Manager>.
            That would break type safety.

            Mutable collections are invariant.
            Read-only/producers can be covariant.
        */
    }

    private static void RunContravarianceExample(IReadOnlyList<Manager> managers)
    {
        Console.WriteLine("======================================");
        Console.WriteLine("CONTRAVARIANCE EXAMPLE");
        Console.WriteLine("======================================");

        IValidator<Employee> employeeBasicValidator =
            new EmployeeBasicValidator();

        /*
            Contravariance:

            IValidator<in T> accepts T as input.

            A validator that can validate any Employee
            can also validate a Manager.

            Because Manager is an Employee.
        */

        IValidator<Manager> managerBasicValidator =
            employeeBasicValidator;

        IValidator<Manager> managerTeamValidator =
            new ManagerTeamValidator();

        IReadOnlyList<IValidator<Manager>> validators =
            new List<IValidator<Manager>>
            {
                managerBasicValidator,
                managerTeamValidator
            };

        ManagerBonusService bonusService =
            new ManagerBonusService(validators);

        Manager firstManager = managers[0];
        Manager secondManager = managers[1];

        decimal firstBonus = bonusService.CalculateBonus(firstManager);
        decimal secondBonus = bonusService.CalculateBonus(secondManager);

        Console.WriteLine(
            $"Manager: {firstManager.Name}, " +
            $"Team Size: {firstManager.TeamSize}, " +
            $"Bonus: {firstBonus:N2}");

        Console.WriteLine(
            $"Manager: {secondManager.Name}, " +
            $"Team Size: {secondManager.TeamSize}, " +
            $"Bonus: {secondBonus:N2}");

        Test.Equal(
            expected: 100000m,
            actual: firstBonus,
            testName: "Manager with 12 team members gets 20% bonus");

        Test.Equal(
            expected: 30000m,
            actual: secondBonus,
            testName: "Manager with 5 team members gets 10% bonus");

        Console.WriteLine();
    }
}
```

---

# How to Run

```bash
dotnet run
```

---

# Expected Output

```text
C# Covariance and Contravariance - Simple HR Demo

======================================
COVARIANCE EXAMPLE
======================================
=== Employee Dashboard ===
Employee: Dinuka, Department: Engineering, Salary: 500,000.00
Employee: Kamal, Department: Quality Assurance, Salary: 300,000.00

PASSED: Dashboard printed 2 employees from manager data source
PASSED: All managers are safely treated as employees

======================================
CONTRAVARIANCE EXAMPLE
======================================
Manager: Dinuka, Team Size: 12, Bonus: 100,000.00
Manager: Kamal, Team Size: 5, Bonus: 30,000.00
PASSED: Manager with 12 team members gets 20% bonus
PASSED: Manager with 5 team members gets 10% bonus

All validations completed successfully.
```

---

# The Core Idea with This Example

## Relationship

```text
Manager is an Employee
```

```csharp
public sealed class Manager : Employee
```

---

# Covariance Explained Simply

## Code

```csharp
IReadOnlyDataSource<Manager> managerDataSource =
    new InMemoryManagerDataSource(managers);

IReadOnlyDataSource<Employee> employeeDataSource =
    managerDataSource;
```

## Why this works

Because this interface only returns data:

```csharp
public interface IReadOnlyDataSource<out T>
{
    IReadOnlyList<T> GetAll();
}
```

A source that returns managers can safely be used as a source that returns employees.

Because every manager is an employee.

```text
Manager data source gives Manager objects.
Dashboard expects Employee objects.
That is safe because Manager is Employee.
```

---

# Covariance Mental Picture

```text
IReadOnlyDataSource<Manager>
        |
        | because Manager is Employee
        v
IReadOnlyDataSource<Employee>
```

Real-world meaning:

```text
Manager list can be shown in an employee dashboard.
```

---

# Contravariance Explained Simply

## Code

```csharp
IValidator<Employee> employeeBasicValidator =
    new EmployeeBasicValidator();

IValidator<Manager> managerBasicValidator =
    employeeBasicValidator;
```

## Why this works

Because this interface only accepts data:

```csharp
public interface IValidator<in T>
{
    ValidationResult Validate(T item);
}
```

A validator that can validate any employee can validate a manager.

Because every manager is an employee.

```text
Employee validator accepts Employee.
Manager is Employee.
Therefore Employee validator can accept Manager.
```

---

# Contravariance Mental Picture

This direction feels opposite:

```text
IValidator<Employee>
        |
        | because Manager is Employee
        v
IValidator<Manager>
```

Real-world meaning:

```text
General employee validation rules can be reused for managers.
```

For example:

```text
Employee name required
Employee department required
Employee salary must be greater than zero
```

These rules apply to managers too.

---

# Why the Direction Feels Opposite

Because covariance is about **returning**.

```text
Give me employees.
Giving managers is okay because managers are employees.
```

Contravariance is about **accepting**.

```text
I can validate employees.
So I can also validate managers because managers are employees.
```

---

# Final Simple Comparison

## Covariance

```csharp
IReadOnlyDataSource<Manager> managerSource;
IReadOnlyDataSource<Employee> employeeSource = managerSource;
```

Meaning:

```text
Specific output can be used as general output.
```

Use when:

```text
Your generic type only returns T.
```

Keyword:

```csharp
out T
```

---

## Contravariance

```csharp
IValidator<Employee> employeeValidator;
IValidator<Manager> managerValidator = employeeValidator;
```

Meaning:

```text
General input handler can be used as specific input handler.
```

Use when:

```text
Your generic type only accepts T.
```

Keyword:

```csharp
in T
```

---

# Very Short Memory Trick

```text
out = output = covariance
in = input = contravariance
```

```text
Producer<T> gives T      => out T
Consumer<T> accepts T    => in T
```

In this project:

```text
IReadOnlyDataSource<out T>
returns T
so it is covariant
```

```text
IValidator<in T>
accepts T
so it is contravariant
```
