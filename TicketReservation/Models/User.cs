using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketReservation.Models;

public enum UserTypeEnum
{
    [Description("Backoffice")] Backoffice,
    [Description("Travel Agent")] TravelAgent,
    [Description("Customer")] Customer
}

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    [BsonElement("name")] public string Name { get; set; } = string.Empty;
    [BsonElement("age")] public int Age { get; set; }
    [BsonElement("nic")] public string Nic { get; set; } = string.Empty;
    [BsonElement("user_type")] public UserTypeEnum UserType { get; set; } = UserTypeEnum.Customer;
}

public class CreateUserRequest
{
    [BsonElement("name")] public string Name { get; set; } = string.Empty;
    [BsonElement("age")] public int Age { get; set; }
    [BsonElement("nic")] public string Nic { get; set; } = string.Empty;
    [BsonElement("user_type")] public string UserType { get; set; } = string.Empty;
}

public class EditUserRequest
{
    [BsonElement("name")] public string? Name { get; set; } = string.Empty;
    [BsonElement("age")] public int Age { get; set; } = 0;
    [BsonElement("user_type")] public string UserType { get; set; } = string.Empty;
}