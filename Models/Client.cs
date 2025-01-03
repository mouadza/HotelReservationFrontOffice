﻿using Microsoft.EntityFrameworkCore;

namespace HotelReservationFrontOffice.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Client
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
    }
}
