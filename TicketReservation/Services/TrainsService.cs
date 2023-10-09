using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TicketReservation.Config;
using TicketReservation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketReservation.Services
{
    public class TrainService
    {
        private readonly IMongoCollection<Train> _trainCollection;

        public TrainService(IOptions<DatabaseSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);

            _trainCollection = database.GetCollection<Train>("trains");
        }

        public async Task<List<Train>> GetAll() =>
            await _trainCollection.Find(train => true).ToListAsync();
        
        public async Task<Train?> GetSingle(string trainId) =>
            await _trainCollection.Find(train => train.Id == trainId).FirstOrDefaultAsync();
        
        public async Task<List<Train>> GetByOwner(string ownerNic) =>
            await _trainCollection.Find(train => train.OwnerNic == ownerNic).ToListAsync();

        public async Task Create(Train train) =>
            await _trainCollection.InsertOneAsync(train);

        public async Task Update(string trainId, Train trainIn) =>
            await _trainCollection.ReplaceOneAsync(train => train.Id == trainId, trainIn);

        public async Task Remove(string trainId) =>
            await _trainCollection.DeleteOneAsync(train => train.Id == trainId);
    }
}