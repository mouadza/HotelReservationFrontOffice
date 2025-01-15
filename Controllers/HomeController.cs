using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservationFrontOffice.Data;
using HotelReservationFrontOffice.Models;
using MimeKit;
using MailKit.Net.Smtp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
namespace HotelReservationFrontOffice.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            RoomService roomService = new RoomService();
            List<Room> availableRooms = roomService.GetAvailableRooms();
            List<RoomType> roomTypes = roomService.GetRoomTypes();
            ViewBag.RoomTypes = roomTypes;
            return View(availableRooms);
        }
        public IActionResult About()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Rooms(string roomType, int? priceRange, String? availability)
        {
            RoomService roomService = new RoomService();
            List<RoomType> roomTypes = roomService.GetRoomTypes();
            List<Room> filteredRooms = roomService.GetFilteredRooms(roomType, priceRange, availability);

            ViewBag.RoomTypes = roomTypes;

            return View(filteredRooms);
        }
        public ActionResult Book(int id)
        {
            RoomService roomService = new RoomService();
            Room room = roomService.GetRoomById(id);
            return View(room);
        }
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public JsonResult SubmitReservation(int roomId, DateTime checkin, DateTime checkout)
        {
            // Ensure the client is logged in
            var clientIdString = _httpContextAccessor.HttpContext.Session.GetString("ClientId");
            if (string.IsNullOrEmpty(clientIdString))
            {
                return Json(new { success = false, message = "Please log in to complete your booking.", redirectTo = "/Account/Login" });
            }
            int clientId = Convert.ToInt32(clientIdString);

            // Check room availability by roomId and status
            RoomService roomService = new RoomService();
            Room room = roomService.GetRoomById(roomId);
            if (room.Status != "Disponible")
            {
                return Json(new { success = false, message = "The selected room is not available.", redirectTo = "/Home/Rooms" });
            }

            // Calculate total price
            int numberOfNights = (checkout - checkin).Days;
            decimal total = (decimal)(numberOfNights * room.Prix);

            // Add reservation
            ReservationService reservationService = new ReservationService();
            reservationService.AddReservation(clientId, roomId, checkin, checkout, total);
            var reservationEntity = new
            {
                Id = reservationService.LastReservationId,
                IdClient = clientId,
                DateDebut = checkin,
                DateFin = checkout,
                Total = total,
                Statut = "Confirmed"
            };

            // Update room status to "Occupée"
            roomService.UpdateRoomStatus(roomId, "Occupée");

            // Send confirmation email
            var client = _context.Client.FirstOrDefault(c => c.Id == clientId);
            if (client != null)
            {
                var emailService = new EmailService();
                var subject = "Reservation Confirmation";
                var body = $@"
                <p>Dear {client.FirstName} {client.LastName},</p>
                <p>Your reservation with ID {reservationService.LastReservationId} has been successfully made.</p>
                <p>Details:</p>
                <ul>
                    <li>Check-in Date: {checkin:dd/MM/yyyy}</li>
                    <li>Check-out Date: {checkout:dd/MM/yyyy}</li>
                    <li>Total: ${total}</li>
                </ul>
                <p>Thank you for choosing our hotel!</p>
                <p>Best regards,<br/>HotelIMAR</p>
                ";
                emailService.SendEmail(client.Email, subject, body);
            }

            return Json(new { success = true, message = "Your reservation has been successfully submitted, Check Email Confirmation!" });
        }




    }
}
