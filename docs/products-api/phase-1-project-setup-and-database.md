# Products API - Phase 1: Project Setup and Database

This phase creates the Minimal API project and starts PostgreSQL with pgAdmin.

## Learning outcomes

By the end of this products API module, you will know how to:

- Create an ASP.NET Core Minimal API project.
- Run PostgreSQL and pgAdmin with Docker Compose.
- Connect EF Core to PostgreSQL.
- Create entity, DTO, validator, extension, and DbContext classes in separate folders.
- Map enums with EF Core.
- Convert `DateTime` values to UTC before saving.
- Validate request DTOs with FluentValidation.
- Generate OpenAPI automatically.
- Use Scalar to test API documentation.
- Test CRUD endpoints with sample requests.

Phases:

- Phase 1: project setup and database
- Phase 2: EF Core model, DbContext, DTOs, validators, and migrations
- Phase 3: CRUD routes, FluentValidation, Scalar, and API concepts
- Phase 4: testing and sample API requests

## Final project structure

```text
ProductMinimalApi/
  docker-compose.yml
  appsettings.json
  ProductMinimalApi.csproj
  Program.cs
  Models/
    Product.cs
    ProductStatus.cs
  Extensions/
    DateTimeExtensions.cs
  Data/
    AppDbContext.cs
  DTOs/
    CreateProductRequest.cs
    UpdateProductRequest.cs
  Validators/
    CreateProductRequestValidator.cs
    UpdateProductRequestValidator.cs
```

## 1. Create web project

```bash
dotnet new web -n ProductMinimalApi
cd ProductMinimalApi
```

`dotnet new web` creates a small ASP.NET Core app. This is good for Minimal API because we do not need controllers.

## 2. Add packages

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Scalar.AspNetCore
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```

Package usage:

| Package | Purpose |
| --- | --- |
| `Microsoft.EntityFrameworkCore` | EF Core base package |
| `Microsoft.EntityFrameworkCore.Design` | Needed for migrations |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL provider for EF Core |
| `Microsoft.AspNetCore.OpenApi` | Automatic OpenAPI generation |
| `Scalar.AspNetCore` | API documentation UI |
| `FluentValidation` | Request DTO validation rules |
| `FluentValidation.DependencyInjectionExtensions` | Registers validators with dependency injection |

## 3. Add PostgreSQL and pgAdmin

Create `docker-compose.yml` in the project root:

```yaml
services:
  dotnet_class_postgres:
    image: postgres:16
    container_name: dotnet_class_product_api_postgres
    environment:
      POSTGRES_DB: product_api_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - dotnet_class_product_api_postgres_data:/var/lib/postgresql/data

  dotnet_class_pgadmin:
    image: dpage/pgadmin4
    container_name: dotnet_class_product_api_pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@dotnetclass.com
      PGADMIN_DEFAULT_PASSWORD: admin
    ports:
      - "5050:80"
    depends_on:
      - dotnet_class_postgres

volumes:
  dotnet_class_product_api_postgres_data:
```

Start database:

```bash
docker compose up -d
```

Open pgAdmin:

- URL: `http://localhost:5050`
- Email: `admin@dotnetclass.com`
- Password: `admin`

Register PostgreSQL server in pgAdmin:

- Host name/address: `dotnet_class_postgres`
- Port: `5432`
- Maintenance database: `product_api_db`
- Username: `postgres`
- Password: `postgres`

## 4. Add connection string

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=product_api_db;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

The API runs on your machine, so it connects to PostgreSQL using `localhost`.

Inside pgAdmin, use the Docker service name `dotnet_class_postgres`.

## Phase 1 checkpoint

Run:

```bash
dotnet build
```

If the build works and Docker is running, continue to Phase 2.
