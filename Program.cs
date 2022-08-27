
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(option=>option.AddDefaultPolicy(
    option=>option.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
));
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<DataDb>(opt => opt.UseInMemoryDatabase("DataList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();
app.UseCors();
app.MapGet("/rooms", async (TodoDb db) =>
    await db.Todos.ToListAsync());



app.MapPost("/rooms", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/rooms/{todo.Id}", todo);
});


app.MapPut("/rooms/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.roomno = inputTodo.roomno;
    todo.adultno = inputTodo.adultno;
    todo.childno = inputTodo.childno;
    todo.price = inputTodo.price;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/rooms/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});
//booking
app.MapGet("/booking", async (DataDb db) =>
    await db.Datas.ToListAsync());
app.MapPost("/booking", async (Data Data, DataDb db) =>
{
    db.Datas.Add(Data);
    await db.SaveChangesAsync();

    return Results.Created($"/booking/{Data.Id}", Data);
});
app.MapPut("/booking/{id}", async (int id, Data inputData, DataDb db) =>
{
    var data = await db.Datas.FindAsync(id);

    if (data is null) return Results.NotFound();

    data.lastname = inputData.lastname;
    data.firstname = inputData.firstname;
    data.checkedin = inputData.checkedin;
    data.checkedout = inputData.checkedout;
    data.adultnumber = inputData.adultnumber;
    data.childnumber = inputData.childnumber;
    

    await db.SaveChangesAsync();

    return Results.NoContent();
});
app.MapDelete("/booking/{id}", async (int id, DataDb db) =>
{
    if (await db.Datas.FindAsync(id) is Data data)
    {
        db.Datas.Remove(data);
        await db.SaveChangesAsync();
        return Results.Ok(data);
    }
    return Results.NotFound();
});
app.Run();

class Todo
{
    public int Id {get; set;}
    public int roomno { get; set; }
    public int adultno { get; set; }
    public int childno {get; set;}
    public int price {get; set;}
}
class Data
{
    public int Id {get; set;}
    public string? lastname {get; set;}
    public string? firstname { get; set; }
    public string? checkedin { get; set; }
    public string? checkedout {get; set;}
    public int adultnumber {get; set;}
    public int childnumber {get; set;}

}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}
class DataDb : DbContext
{
    public DataDb(DbContextOptions<DataDb> options)
        : base(options) { }

    public DbSet<Data> Datas => Set<Data>();
}