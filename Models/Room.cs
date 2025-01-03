using System;


public class Room
{
    public int Id { get; set; }
    public string Numero { get; set; }
    public Double Prix { get; set; }
    public string Status { get; set; }
    public string RoomTypeName { get; set; }
    public int IdTypeChambre { get; set; }
}
public class RoomType
{
    public int Id { get; set; }
    public string Name { get; set; }
}
