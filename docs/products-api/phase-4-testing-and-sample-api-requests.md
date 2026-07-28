# Products API - Phase 4: Testing and Sample API Requests

This phase tests the Products API using the browser, Scalar, and curl.

Start from the project completed in Phase 3:

```bash
cd ProductMinimalApi
```

## 1. Run API

```bash
dotnet run --urls http://localhost:5000
```

Open:

- App info: `http://localhost:5000/`
- OpenAPI JSON: `http://localhost:5000/openapi/v1.json`
- Scalar API docs: `http://localhost:5000/scalar/v1`

## 2. Test API with Scalar

Open:

```text
http://localhost:5000/scalar/v1
```

You can test:

- `GET /products`
- `GET /products/{id}`
- `POST /products`
- `PATCH /products/{id}`
- `DELETE /products/{id}`

Scalar uses the automatically generated OpenAPI document.

## 3. Create product

```bash
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Keyboard","description":"Mechanical keyboard","price":75000,"stocks":10,"status":"Active"}'
```

Create another product:

```bash
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Mouse","description":"Wireless mouse","price":25000,"stocks":20,"status":"Active"}'
```

## 4. List products

```bash
curl http://localhost:5000/products
```

## 5. Search and filter products

Search products:

```bash
curl "http://localhost:5000/products?search=key"
```

Filter by price:

```bash
curl "http://localhost:5000/products?minPrice=20000&maxPrice=80000"
```

Filter in-stock products:

```bash
curl "http://localhost:5000/products?inStockOnly=true"
```

Filter by status:

```bash
curl "http://localhost:5000/products?status=Active"
```

## 6. Get product by id

Replace `PRODUCT_ID_HERE` with a real product id from the list response.

```bash
curl http://localhost:5000/products/PRODUCT_ID_HERE
```

## 7. Update product

```bash
curl -X PATCH http://localhost:5000/products/PRODUCT_ID_HERE \
  -H "Content-Type: application/json" \
  -d '{"price":80000,"stocks":8,"status":"Inactive"}'
```

## 8. Delete product

```bash
curl -X DELETE http://localhost:5000/products/PRODUCT_ID_HERE
```

## 9. Test FluentValidation errors

Missing status:

```bash
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Monitor","description":"4K monitor","price":120000,"stocks":5}'
```

Expected result:

```text
400 Bad Request
```

Invalid name and price:

```bash
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name":"","description":"Bad product","price":0,"stocks":-1,"status":"Active"}'
```

Expected validation messages:

- Product name is required.
- Price must be greater than 0.
- Stocks cannot be negative.

Missing description:

```bash
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Monitor","description":"","price":120000,"stocks":5,"status":"Active"}'
```

Expected validation message:

```text
Description is required.
```

Invalid PATCH with no fields:

```bash
curl -X PATCH http://localhost:5000/products/PRODUCT_ID_HERE \
  -H "Content-Type: application/json" \
  -d '{}'
```

Expected validation message:

```text
At least one field is required.
```

Invalid status:

```bash
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Invalid Product","description":"Wrong status","price":1000,"stocks":1,"status":"Deleted"}'
```

Expected result:

```text
400 Bad Request
```

## 10. Test IEnumerable example

```bash
curl http://localhost:5000/examples/ienumerable
```

This endpoint loads products first and filters in memory.

It is only for learning the difference between `IEnumerable` and `IQueryable`.

## 11. Stop database

```bash
docker compose down
```

To delete the database volume also:

```bash
docker compose down -v
```

Only use `-v` when you want to remove saved PostgreSQL data.
