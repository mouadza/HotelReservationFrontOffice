using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationFrontOffice.Models;
using HotelReservationFrontOffice.Data;
using BCrypt.Net;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string fname, string lname, string tele, string email, string password, string Cpassword)
    {
        if (string.IsNullOrWhiteSpace(fname) || string.IsNullOrWhiteSpace(lname) ||
            string.IsNullOrWhiteSpace(tele) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(Cpassword))
        {
            ModelState.AddModelError("", "All fields are required.");
            return View();
        }

        if (password != Cpassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return View();
        }

        var existingClient = await _context.Client.FirstOrDefaultAsync(c => c.Email == email);
        if (existingClient != null)
        {
            ModelState.AddModelError("", "Email is already registered.");
            return View();
        }

        var client = new Client
        {
            FirstName = fname,
            LastName = lname,
            Phone = tele,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Client.Add(client);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Registered successfully!";
        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            TempData["ErrorMessage"] = "Email and Password are required.";
            return RedirectToAction("Login", "Account");
        }

        var client = await _context.Client.FirstOrDefaultAsync(c => c.Email == email);
        if (client == null || !BCrypt.Net.BCrypt.Verify(password, client.PasswordHash))
        {
            TempData["ErrorMessage"] = "Invalid email or password.";
            return RedirectToAction("Login", "Account");
        }

        HttpContext.Session.SetString("UserName", $"{client.FirstName} {client.LastName}");
        HttpContext.Session.SetString("ClientId", client.Id.ToString());
        return RedirectToAction("Index", "Home");
    }



    [HttpPost]
    public IActionResult Logout()
    {
        // Clear all session data
        HttpContext.Session.Clear();

        // Redirect the user to the Login page
        return RedirectToAction("Login", "Account");
    }
}
