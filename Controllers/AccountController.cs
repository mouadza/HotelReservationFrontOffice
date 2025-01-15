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
        var verificationCode = Guid.NewGuid().ToString().Substring(0, 6);

        var newClient = new Client
        {
            FirstName = fname,
            LastName = lname,
            Phone = tele,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        };

        // Save user to the database
        _context.Client.Add(newClient);
        await _context.SaveChangesAsync();

        // Store the verification details temporarily in session
        HttpContext.Session.SetString("VerificationEmail", email);
        HttpContext.Session.SetString("VerificationCode", verificationCode);

        // Send verification email
        await SendVerificationEmail(email, verificationCode);

        TempData["SuccessMessage"] = "Please verify your email to complete the registration.";
        return RedirectToAction("Verify");
    }
    public IActionResult Verify()
    {
        var email = HttpContext.Session.GetString("VerificationEmail");
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Register");
        }
        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    public IActionResult Verify(string email, string code)
    {
        var sessionEmail = HttpContext.Session.GetString("VerificationEmail");
        var sessionCode = HttpContext.Session.GetString("VerificationCode");

        if (sessionEmail != email || sessionCode != code)
        {
            ModelState.AddModelError("", "Invalid or expired verification code.");
            return View();
        }

        // Verification successful; clear session
        HttpContext.Session.Remove("VerificationEmail");
        HttpContext.Session.Remove("VerificationCode");

        TempData["SuccessMessage"] = "Email verified successfully! You can now log in.";
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

Verification Code: {code}

If you did not request this verification, please ignore this email.

Best regards,
IMAR Hotel Team

--------------------------------------------------------
For support, please contact us at support@imarhotel.com
        ",
            IsBodyHtml = false,  // Set to false for plain text email
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