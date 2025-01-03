using HotelReservationFrontOffice.Models;
using System;
using System.Data.SqlClient;

public class ReservationService
{
    private const string connectionString = DataBaseConnection.ConnectionString;
    public int LastReservationId { get; private set; }

    public void AddReservation(int idClient, int roomId, DateTime checkin, DateTime checkout, decimal total)
    {
        // Validate dates to ensure they are within the acceptable SQL Server DateTime range
        if (checkin < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue || checkin > (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue ||
            checkout < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue || checkout > (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException("Dates must be between 1/1/1753 and 12/31/9999.");
        }

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = @"
                INSERT INTO Reservation (DateDebut, DateFin, Total, Statut, IdClient, IdChambre)
                OUTPUT INSERTED.Id
                VALUES (@DateDebut, @DateFin, @Total, @Statut, @IdClient, @IdChambre)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DateDebut", checkin);
                command.Parameters.AddWithValue("@DateFin", checkout);
                command.Parameters.AddWithValue("@Total", total);
                command.Parameters.AddWithValue("@Statut", "Confirmed");
                command.Parameters.AddWithValue("@IdClient", idClient);
                command.Parameters.AddWithValue("@IdChambre", roomId);

                connection.Open();
                LastReservationId = (int)command.ExecuteScalar();
            }
        }
    }
}
