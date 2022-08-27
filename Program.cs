
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(option=>option.AddDefaultPolicy(
    option=>option.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
));
builder.Services.AddDbContext<HotelDb>(opt => opt.UseInMemoryDatabase("HotelDb"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();
app.UseCors();

app.MapGet("/",()=>"Hello World");

app.MapGet("/rooms", async (HotelDb db) =>
    await db.Rooms.Include(r=>r.Bookings).ToListAsync());

app.MapGet("/room/{Id}",
    async(int Id,HotelDb db) => 
    await db.Rooms.Include(r=>r.Bookings)
    .Where(r=>r.Id == Id).SingleOrDefaultAsync()
);

app.MapPost("/room", async (Room room, HotelDb db) =>
{
    db.Rooms.Add(room);
    await db.SaveChangesAsync();
    return Results.Created($"/rooms/{room.Id}", room);
});


app.MapPut("/room/{id}", async (int id, Room roomToUpdate, HotelDb db) =>
{
    var room = await db.Rooms.FindAsync(id);

    if (room is null) return Results.NotFound();

    room.RoomNo = roomToUpdate.RoomNo;
    room.AdultCapacity = roomToUpdate.AdultCapacity;
    room.ChildCapacity = roomToUpdate.ChildCapacity;
    room.Price = roomToUpdate.Price;

    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapDelete("/room/{id}", async (int id, HotelDb db) =>
{
    if (await db.Rooms.FindAsync(id) is Room room)
    {
        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
        return Results.Ok(room);
    }

    return Results.NotFound();
});

//booking
app.MapGet("/bookings", async (HotelDb db) =>
    await db.Bookings.ToListAsync());

app.MapPost("/booking", async (Booking booking, HotelDb db) =>
{
    var room = await db.Rooms.FindAsync(booking.RoomId);
    if(room is null) return Results.NotFound();
    room.Bookings.Add(booking);
    await db.SaveChangesAsync();
    return Results.Created($"/booking/{booking.Id}", booking);
});

app.Run();

class Room
{
    public Room(){
        Bookings = new List<Booking>();
        CreatedAt = DateTime.Now;
    }
    public int Id {get; set;}
    public int RoomNo { get; set; }
    public int AdultCapacity { get; set; }
    public int ChildCapacity {get; set;}
    public int Price {get; set;}
    public DateTime CreatedAt { get; internal set; }
    public ICollection<Booking> Bookings { get; set; }
}
class Booking
{
    public int Id {get; set;}
    public string? LastName {get; set;}
    public string? FirstName { get; set; }
    public DateTime CheckedInDate { get; set; }
    public DateTime CheckedOutDate {get; set;}
    public int AdultNumber {get; set;}
    public int ChildNumber {get; set;}
    public int RoomId { get; set; }
}

class HotelDb : DbContext
{
    public HotelDb(DbContextOptions<HotelDb> options)
        : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
}