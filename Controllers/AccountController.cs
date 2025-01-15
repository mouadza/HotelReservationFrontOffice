﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationFrontOffice.Models;
using HotelReservationFrontOffice.Data;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;

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
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError("", "Invalid input. Please correct the errors.");
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

        // Generate a verification token
        var random = new Random();
        var verificationCode = random.Next(100000, 999999).ToString();

        // Prepare client data (not saving to DB yet)
        var newClient = new Client
        {
            FirstName = fname,
            LastName = lname,
            Phone = tele,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        };

        // Serialize client data and store it in session
        HttpContext.Session.SetString("PendingClient", System.Text.Json.JsonSerializer.Serialize(newClient));
        HttpContext.Session.SetString("VerificationCode", verificationCode);

        // Send verification email
        await SendVerificationEmail(email, verificationCode);

        TempData["SuccessMessage"] = "Please verify your email to complete the registration.";
        return RedirectToAction("Verify");
    }


    [HttpGet]
    public IActionResult Verify()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Verify(string code)
    {
        var clientDataJson = HttpContext.Session.GetString("PendingClient");
        var storedCode = HttpContext.Session.GetString("VerificationCode");

        if (string.IsNullOrEmpty(clientDataJson) || string.IsNullOrEmpty(storedCode))
        {
            ModelState.AddModelError("", "Invalid or expired verification process. Please register again.");
            return RedirectToAction("Register");
        }
        if (code != storedCode)
        {
            ModelState.AddModelError("", "Invalid or expired verification code.");
            return View();
        }
        var clientData = System.Text.Json.JsonSerializer.Deserialize<Client>(clientDataJson);
        if (clientData == null)
        {
            ModelState.AddModelError("", "Error verifying email. Please register again.");
            return RedirectToAction("Register");
        }

        _context.Client.Add(clientData);
        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("PendingClient");
        HttpContext.Session.Remove("VerificationCode");
        TempData["SuccessMessage"] = "Registration complete. You can now log in.";
        return RedirectToAction("Login");
    }






    private async Task SendVerificationEmail(string email, string code)
    {
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("elidrissiabdallah689@gmail.com", "unqn bqbf zaeh egbz"),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("your-email@example.com", "IMAR Hotel"),
            Subject = "IMAR Hotel - Email Verification Code",
            Body = $@"
            Hello,

            Thank you for registering with IMAR Hotel. To complete your registration, please verify your email address using the code below:

            Verification Code: <p class='text-gray-600'>{code}</p>

            If you did not request this verification, please ignore this email.

            Best regards,
            IMAR Hotel Team

            --------------------------------------------------------
            For support, please contact us at support@imarhotel.com
        ",
            IsBodyHtml = false,
        };

        mailMessage.To.Add(email);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            // Log exception if needed
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
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
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }
}