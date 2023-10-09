using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TicketReservation.Config;
using TicketReservation.Models;

namespace TicketReservation.Services;

public class ReservationService
{
    private readonly IMongoCollection<Reservation> _reservationCollection;

    public ReservationService(IOptions<DatabaseSettings> settings)
    {
        // Initialize the database connection
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);

        _reservationCollection = database.GetCollection<Reservation>("reservations");
    }

    public async Task<List<Reservation>> GetAll() =>
        await _reservationCollection.Find(reservation => true).ToListAsync();

    public async Task<Reservation?> GetByReservationID(string id) =>
        await _reservationCollection.Find(reservation => reservation.Id == id).FirstOrDefaultAsync();
    
    public async Task<List<Reservation>> GetByNic(string nic) =>
        await _reservationCollection.Find(reservation => reservation.UserNic == nic).ToListAsync();

    public async Task<List<Reservation>> GetByTrainID(string id) =>
        await _reservationCollection.Find(reservation => reservation.TrainId == id).ToListAsync();

    public async Task Create(Reservation reservation) =>
        await _reservationCollection.InsertOneAsync(reservation);
    
    public async Task Update(string nic, Reservation reservationIn) =>
        await _reservationCollection.ReplaceOneAsync(reservation => reservation.UserNic == nic, reservationIn);
    
    public async Task Remove(string nic) =>
        await _reservationCollection.DeleteOneAsync(reservation => reservation.UserNic == nic);
}