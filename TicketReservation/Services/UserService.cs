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
    
    public async Task<List<User>> GetAllByType(string userType) =>
        await _userCollection.Find(user => user.UserType.ToLower() == userType.ToLower()).ToListAsync();

    public async Task<User?> GetSingle(string nic) =>
        await _userCollection.Find(user => user.Nic == nic).FirstOrDefaultAsync();


    public async Task Create(User user) =>
        await _userCollection.InsertOneAsync(user);

    
    public async Task Update(string nic, User userIn) =>
        await _userCollection.ReplaceOneAsync(user => user.Nic == nic, userIn);


    public async Task Remove(string nic) =>
        await _userCollection.DeleteOneAsync(user => user.Nic == nic);
}