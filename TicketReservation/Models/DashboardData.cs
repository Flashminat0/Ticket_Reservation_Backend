using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketReservation.Models;

public class DashboardData
{
    [BsonElement("customer_count")] public int CustomerCount { get; set; } 
    [BsonElement("travel_agent_count")] public int TravelAgentCount { get; set; }
    [BsonElement("train_count")] public int TrainCount { get; set; }
    [BsonElement("reservation_count")] public int ReservationCount { get; set; }    
}