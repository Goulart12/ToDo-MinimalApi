using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/todoItems", async (ToDoDb db) => await db.ToDos.Select(x => new ToDoItemDTO(x)).ToListAsync());

app.MapGet("/todoItems/complete", async (ToDoDb db) => await db.ToDos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todoItems/{id}", async (int id, ToDoDb db) => await db.ToDos.FindAsync(id) 
    is ToDo todo 
    ? Results.Ok(new ToDoItemDTO(todo)) 
    : Results.NotFound());

app.MapPost("/todoItems", async (ToDoItemDTO toDoItemDto, ToDoDb db) =>
{
    var todoItem = new ToDo()
    {
        IsComplete = toDoItemDto.IsComplete,
        Name = toDoItemDto.Name
    };
    
    db.ToDos.Add(todoItem);
    await db.SaveChangesAsync();

    return Results.Created($"/todoItems/{todoItem.Id}", new ToDoItemDTO(todoItem));
});

app.MapPut("/todoItems/{id}", async (int id, ToDoItemDTO toDoItemDto, ToDoDb db) =>
{
    var todo = await db.ToDos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = toDoItemDto.Name;
    todo.IsComplete = toDoItemDto.IsComplete;

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