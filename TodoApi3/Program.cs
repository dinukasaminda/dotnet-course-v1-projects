using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

var todos = new List<Todo>
{
    new(1, "Learn minimal APIs", false),
    new(2, "Build TodoApiV1", true)
};

var nextId = todos.Max(todo => todo.Id) + 1;

app.MapGet("/", () => Results.Ok(new
{
    App = "TodoApiV1",
    Message = "Todo app API using ASP.NET wsCore minimal APIs.",
    OpenApi = "/openapi/v1.json",
    ApiReference = "/scalar/v1"
}));

app.MapGet("/todos", (bool? completed) =>
{
    var result = completed is null
        ? todos
        : todos.Where(todo => todo.IsCompleted == completed);

    return Results.Ok(result);
});

app.MapGet("/todos/search", (string? text, bool? completed) =>
{
    var result = todos.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(text))
    {
        result = result.Where(todo =>
            todo.Title.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    if (completed is not null)
    {
        result = result.Where(todo => todo.IsCompleted == completed);
    }

    return Results.Ok(result);
});

app.MapGet("/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(todo => todo.Id == id);

    return todo is null ? Results.NotFound() : Results.Ok(todo);
});

app.MapPost("/todos", (CreateTodoRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(new { Error = "Title is required." });
    }

    var todo = new Todo(nextId++, request.Title.Trim(), false);
    todos.Add(todo);

    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPatch("/todos/{id:int}", (int id, UpdateTodoRequest request) =>
{
    var todo = todos.FirstOrDefault(todo => todo.Id == id);

    if (todo is null)
    {
        return Results.NotFound();
    }

    if (request.Title is not null)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest(new { Error = "Title cannot be empty." });
        }

        todo.Title = request.Title.Trim();
    }

    if (request.IsCompleted is not null)
    {
        todo.IsCompleted = request.IsCompleted.Value;
    }

    return Results.Ok(todo);
});

app.MapDelete("/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(todo => todo.Id == id);

    if (todo is null)
    {
        return Results.NotFound();
    }

    todos.Remove(todo);

    return Results.NoContent();
});

app.Run();

class Todo(int id, string title, bool isCompleted)
{
    public int Id { get; init; } = id;
    public string Title { get; set; } = title;
    public bool IsCompleted { get; set; } = isCompleted;
}

record CreateTodoRequest(string Title);

record UpdateTodoRequest(string? Title, bool? IsCompleted);
