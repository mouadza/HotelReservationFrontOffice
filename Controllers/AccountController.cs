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

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            ModelState.AddModelError("", "Email is already registered.");
            return View();
        }

        var user = new User
        {
            FirstName = fname,
            LastName = lname,
            Phone = tele,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Registered successfully!";
        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email and Password are required.");
            return View();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }
        HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
        HttpContext.Session.SetString("UserId", user.Id.ToString());
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
