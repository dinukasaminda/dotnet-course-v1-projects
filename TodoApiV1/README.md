# TodoApiV1

Todo app API using ASP.NET Core minimal APIs. Data is stored in memory. No login yet.

## 1. Create project

```bash
dotnet new web -n TodoApiV1
cd TodoApiV1
```

## 2. Add OpenAPI and Scalar packages

```bash
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Scalar.AspNetCore
```

## 3. Run

```bash
dotnet run
```

Open:

- App info: `http://localhost:5000/`
- OpenAPI spec: `http://localhost:5000/openapi/v1.json`
- Scalar API reference: `http://localhost:5000/scalar/v1`

If `dotnet run` shows another port, use that port instead.

## API examples

List todos:

```bash
curl http://localhost:5000/todos
```

List completed todos with query param:

```bash
curl "http://localhost:5000/todos?completed=true"
```

Search todos with query params:

```bash
curl "http://localhost:5000/todos/search?text=api&completed=false"
```

Get todo by path param:

```bash
curl http://localhost:5000/todos/1
```

Create todo with POST data:

```bash
curl -X POST http://localhost:5000/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Create frontend later"}'
```

Update todo with PATCH:

```bash
curl -X PATCH http://localhost:5000/todos/1 \
  -H "Content-Type: application/json" \
  -d '{"title":"Learn minimal API routes","isCompleted":true}'
```

Delete todo:

```bash
curl -X DELETE http://localhost:5000/todos/1
```
