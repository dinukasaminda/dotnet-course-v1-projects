var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



var products = new List<Product>
{
    new Product(1, "Laptop", 2500000m),
    new Product(2, "Mouse", 5000m),
    new Product(3, "Keyboard", 10500m)
};


app.MapGet("/", () => "Hello Sri Lanka2");

app.MapGet("/products", () =>
{
    return Results.Ok(products);
});

app.Run();



record Product(int Id, string Name, decimal Price);