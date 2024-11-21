using HotelReservationFrontOffice.Data; // Ensure the namespace for ApplicationDbContext is included
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register ApplicationDbContext with the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor(); // Ensure this is added for session and HttpContext usage

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Enable session middleware
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
