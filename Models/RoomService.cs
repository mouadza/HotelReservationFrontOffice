using System.Collections.Generic;
using System.Data.SqlClient;
public class RoomService
{
    private const string ConnectionString = DataBaseConnection.ConnectionString;
    public List<RoomType> GetRoomTypes()
    {
        List<RoomType> roomTypes = new List<RoomType>();

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            string query = "SELECT Id, Nom FROM TypeChambre";

            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                RoomType roomType = new RoomType
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };

                roomTypes.Add(roomType);
            }
        }

        return roomTypes;
    }

    public List<Room> GetAvailableRooms()
    {
        List<Room> rooms = new List<Room>();

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            string query = @"
        SELECT TOP 3 c.Id, c.Numero, t.PrixParNuit, c.IdTypeChambre 
        FROM Chambre c
        JOIN TypeChambre t ON c.IdTypeChambre = t.Id
        WHERE c.Statut = 'Disponible'";

            SqlCommand command = new SqlCommand(query, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Room room = new Room
                {
                    Id = reader.GetInt32(0),
                    Numero = reader.GetString(1),  // Replace Numero with RoomTypeName
                    Prix = reader.GetInt32(2),
                    IdTypeChambre = reader.GetInt32(3)
                };

                rooms.Add(room);
            }
        }


        return rooms;
    }
    public List<Room> GetFilteredRooms(string roomType, int? priceRange, string availability)
    {
        List<Room> rooms = new List<Room>();

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            string query = @"
        SELECT c.Id, c.Numero, t.PrixParNuit, c.IdTypeChambre 
        FROM Chambre c
        JOIN TypeChambre t ON c.IdTypeChambre = t.Id
        WHERE 1=1";

            if (!string.IsNullOrEmpty(roomType))
            {
                query += " AND t.Nom = @RoomType";
            }
            if (priceRange.HasValue)
            {
                query += " AND t.PrixParNuit <= @PriceRange";
            }
            if (availability == "true")
            {
                query += " AND c.Statut = 'Disponible'";
            }

            SqlCommand command = new SqlCommand(query, connection);
            if (!string.IsNullOrEmpty(roomType))
            {
                command.Parameters.AddWithValue("@RoomType", roomType);
            }
            if (priceRange.HasValue)
            {
                command.Parameters.AddWithValue("@PriceRange", priceRange.Value);
            }

            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Room room = new Room
                {
                    Id = reader.GetInt32(0),
                    Numero = reader.GetString(1),
                    Prix = reader.GetInt32(2),
                    IdTypeChambre = reader.GetInt32(3)
                };

                rooms.Add(room);
            }
        }

        return rooms;
    }

    public Room GetRoomById(int id)
    {
        Room room = null;

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            string query = @"
            SELECT c.Id, c.Numero, c.Statut, t.Nom AS RoomTypeName, t.PrixParNuit 
            FROM Chambre c
            JOIN TypeChambre t ON c.IdTypeChambre = t.Id
            WHERE c.Id = @Id";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                room = new Room
                {
                    Id = reader.GetInt32(0),
                    Numero = reader.GetString(1),
                    Status = reader.GetString(2),
                    RoomTypeName = reader.GetString(3),
                    Prix = reader.GetInt32(4)
                };
            }
        }

        return room;
    }
    private const string connectionString = DataBaseConnection.ConnectionString;

    public bool IsRoomAvailable(int roomId, DateTime checkin, DateTime checkout)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = @"
                SELECT COUNT(*)
                FROM Room
                WHERE Id = @RoomId AND Statut = 'Disponible'";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RoomId", roomId);
                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
    }

    public void UpdateRoomStatus(int roomId, string newStatus)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "UPDATE Chambre SET Statut = @NewStatus WHERE Id = @RoomId";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NewStatus", newStatus);
                command.Parameters.AddWithValue("@RoomId", roomId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}