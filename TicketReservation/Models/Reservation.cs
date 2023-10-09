using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketReservation.Models;

public class Reservation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("train_id")] public string TrainId { get; set; } = string.Empty;
    [BsonElement("user_nic")] public string UserNic { get; set; } = string.Empty;
    [BsonElement("seats")] public int Seats { get; set; } = 0;
}

public class CreateReservationRequest
{
    [BsonElement("train_id")] public string TrainId { get; set; } = string.Empty;
    [BsonElement("user_nic")] public string UserNic { get; set; } = string.Empty;
    [BsonElement("seats")] public int Seats { get; set; } = 0;
}

public class EditReservationRequest
{
    [BsonElement("train_id")] public string TrainId { get; set; } = string.Empty;
    [BsonElement("user_nic")] public string UserNic { get; set; } = string.Empty;
    [BsonElement("seats")] public int Seats { get; set; } = 0;
}