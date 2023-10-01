using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/todoItems", async (ToDoDb db) => await db.ToDos.ToListAsync());

app.MapGet("/todoItems/complete", async (ToDoDb db) => await db.ToDos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoItems/{id}", async (int id, ToDoDb db) => await db.ToDos.FindAsync(id) 
    is ToDo todo 
    ? Results.Ok(todo) 
    : Results.NotFound());

app.MapPost("/todoItems", async (ToDo todo, ToDoDb db) =>
{
    db.ToDos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoItems/{todo.Id}", todo);
});

app.MapPut("/todoItems/{id}", async (int id, ToDo inputTodo, ToDoDb db) =>
{
    var todo = await db.ToDos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
})  ;

app.MapDelete("/todoItems/{id}", async (int id, ToDoDb db) =>
{
    if (await db.ToDos.FindAsync(id) is ToDo todo)
    {
        db.ToDos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();