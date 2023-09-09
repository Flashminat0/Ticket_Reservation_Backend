using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TicketReservation.Config;
using TicketReservation.Models;

namespace TicketReservation.Services;

public class UserService
{
    private readonly IMongoCollection<User> _userCollection;

    public UserService(IOptions<DatabaseSettings> settings)
    {
        // Initialize the database connection
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);

        _userCollection = database.GetCollection<User>("users");
    }

    public async Task<List<User>> GetAll() =>
        await _userCollection.Find(user => true).ToListAsync();
    
    
     
}