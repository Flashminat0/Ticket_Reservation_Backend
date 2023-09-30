using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketReservation.Models;

public class Login
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("password")] public string Password { get; set; } = string.Empty;

    [BsonElement("nic")] public string Nic { get; set; } = string.Empty;

    [BsonElement("is_active")] public bool IsActive { get; set; }

    [BsonElement("is_admin")] public bool IsAdmin { get; set; }

    [BsonElement("last_login")] public DateTime LastLogin { get; set; }
    [BsonElement("salt")] public string Salt { get; set; } = string.Empty;
}

public class LoginRequest
{
    [BsonElement("password")] public string Password { get; set; } = string.Empty;

    [BsonElement("nic")] public string Nic { get; set; } = string.Empty;
}

public class ActivateRequest
{
    [BsonElement("nic")] public string Nic { get; set; } = string.Empty;

    [BsonElement("is_active")] public bool IsActive { get; set; }

    [BsonElement("is_admin")] public bool IsAdmin { get; set; }
}