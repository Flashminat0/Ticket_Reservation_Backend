using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketReservation.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("age")] public int Age { get; set; }

    [BsonElement("nic")] public string Nic { get; set; } = string.Empty;

    [BsonElement("is_active")] public bool IsActive { get; set; }
}

public class CreateUserRequest
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;

    [BsonElement("age")] public int Age { get; set; }

    [BsonElement("nic")] public string Nic { get; set; } = string.Empty;

    [BsonElement("is_active")] public bool IsActive { get; set; }
}

public class EditUserRequest
{
    [BsonElement("name")] public string? Name { get; set; } = string.Empty;

    [BsonElement("age")] public int Age { get; set; } = 0;
    
    [BsonElement("is_active")] public bool IsActive { get; set; } = false;
}