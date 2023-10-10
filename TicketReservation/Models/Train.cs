using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketReservation.Models;

public static class TrainTypeCl
{
    public const string Express = "Express";
    public const string Intercity = "Intercity";
    public const string NightMail = "Night Mail";
    public const string Local = "Local";
}

public class Train
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("train_name")] public string TrainName { get; set; } = string.Empty;
    [BsonElement("train_type")] public string TrainType { get; set; } = TrainTypeCl.Local;
    [BsonElement("start_station")] public string StartStation { get; set; } = string.Empty;
    [BsonElement("end_station")] public string EndStation { get; set; } = string.Empty;
    [BsonElement("start_time")] public DateTime StartTime { get; set; } = DateTime.UtcNow;
    [BsonElement("end_time")] public DateTime EndTime { get; set; } = DateTime.UtcNow;
    [BsonElement("price")] public int Price { get; set; } = 0;
    [BsonElement("districts")] public List<string> Districts { get; set; } = new List<string>();
    [BsonElement("distance")] public int Seats { get; set; } = 0;
    [BsonElement("owner_nic")] public string OwnerNic { get; set; } = string.Empty;
    [BsonElement("is_active")] public bool IsActive { get; set; } = false;
}

public class CreateTrainRequest
{
    [BsonElement("train_name")] public string TrainName { get; set; } = string.Empty;
    [BsonElement("train_type")] public string TrainType { get; set; } = TrainTypeCl.Local;
    [BsonElement("start_station")] public string StartStation { get; set; } = string.Empty;
    [BsonElement("end_station")] public string EndStation { get; set; } = string.Empty;
    [BsonElement("start_time")] public DateTime StartTime { get; set; } = DateTime.UtcNow;
    [BsonElement("end_time")] public DateTime EndTime { get; set; } = DateTime.UtcNow;
    [BsonElement("price")] public int Price { get; set; } = 0;
    [BsonElement("districts")] public List<string> Districts { get; set; } = new List<string>();
    [BsonElement("distance")] public int Seats { get; set; } = 0;
    [BsonElement("owner_nic")] public string OwnerNic { get; set; } = string.Empty;
    [BsonElement("is_active")] public bool IsActive { get; set; } = false;

}

public class EditTrainRequest
{
    [BsonElement("id")] public string Id { get; set; } = string.Empty;
    [BsonElement("train_name")] public string TrainName { get; set; } = string.Empty;
    [BsonElement("train_type")] public string TrainType { get; set; } = TrainTypeCl.Local;
    [BsonElement("start_station")] public string StartStation { get; set; } = string.Empty;
    [BsonElement("end_station")] public string EndStation { get; set; } = string.Empty;
    [BsonElement("start_time")] public DateTime StartTime { get; set; } = DateTime.UtcNow;
    [BsonElement("end_time")] public DateTime EndTime { get; set; } = DateTime.UtcNow;
    [BsonElement("price")] public int Price { get; set; } = 0;
    [BsonElement("districts")] public List<string> Districts { get; set; } = new List<string>();
    [BsonElement("editing_nic")] public string EditingNic { get; set; } = string.Empty;
    [BsonElement("distance")] public int Seats { get; set; } = 0;
    [BsonElement("is_active")] public bool IsActive { get; set; } = false;
    
}