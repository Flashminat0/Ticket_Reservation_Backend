namespace TicketReservation.Services;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TicketReservation.Config;
using TicketReservation.Models;

public class LoginService
{
    private readonly IMongoCollection<Login> _loginCollection;

    public LoginService(IOptions<DatabaseSettings> settings)
    {
        // Initialize the database connection
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);

        _loginCollection = database.GetCollection<Login>("logins");
    }


    public async Task<Login> Login(string nic, string password) =>
        await _loginCollection.Find(login => login.Nic == nic && login.Password == password).FirstOrDefaultAsync();

    public async Task<Login> GetSingle(string nic) =>
        await _loginCollection.Find(login => login.Nic == nic).FirstOrDefaultAsync();
    
    public async Task Register(Login login) =>
        await _loginCollection.InsertOneAsync(login);

    public async Task Update(string nic, Login loginIn) =>
        await _loginCollection.ReplaceOneAsync(login => login.Nic == nic, loginIn);

    public async Task Remove(string nic) =>
        await _loginCollection.DeleteOneAsync(login => login.Nic == nic);
}