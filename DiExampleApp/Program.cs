using DiExampleApp.Services;

var builder = WebApplication.CreateBuilder(args);


// Di 
// builder.Services.AddSingleton<IGreetingService, GreetingService>();
builder.Services.AddScoped<IGreetingService, SinhalaGreetingService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/hello/{name}", (string name, IGreetingService greetingService) =>
{
    
    return Results.Ok(new
    {
        Message = greetingService.GreetingMessage(name)
    });
});

app.MapGet("/GreetingCount", ( IGreetingService greetingService) =>
{
    
    return Results.Ok(new
    {
        Count = greetingService.CountMessages()
    });
});

app.Run();




// maintainability 
// testability 
// scalability 


// product related functions ( features) -> ProductService class   inferface  IProductService
// - list products 
// - create producct 
// - update product 

// user related functions  -> UserService class interface IUserService
// - register new user 
// - reset user password 