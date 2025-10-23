using UserService.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace UserService.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        //  private readonly IDistributedCache _cache;
        // private readonly ConnectionMultiplexer _redisConnection;

        public UserService(IConfiguration config)
        {
            // _cache = cache;

            var connectionString = config.GetSection("MongoDB:ConnectionString").Value;
            var databaseName = config.GetSection("MongoDB:DatabaseName").Value;

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _users = database.GetCollection<User>("Users");

            //  var redisConnectionString = config.GetSection("Redis:ConnectionString").Value;
            // _redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        public User GetByUsername(string username)
        {
            return _users.Find(user => user.Username == username).FirstOrDefault();
        }

        public void Create(User user)
        {
            _users.InsertOne(user);

            // Кешируем студента как хэш в Redis
            // string cacheKey = $"User_{user.Id}";

            // var db = _redisConnection.GetDatabase();

            // // Используем Redis Hashes для хранения данных студента
            // var redisHash = new HashEntry[]
            // {
            //     new HashEntry("Id", newStudent.Id),
            //     new HashEntry("Username", newStudent.Name),
            //     new HashEntry("PasswordHash", newStudent.Rating),
            //     new HashEntry("EnrollmentDate", newStudent.EnrollmentDate.ToString("o"))  // Преобразуем в строку ISO 8601
            // };

            // await db.HashSetAsync(cacheKey, redisHash);

            // return newStudent;
        }

        // Метод для хэширования пароля
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task<List<User>> GetAllAsync()
        {
            // const string cacheKey = "Users";

            // var cachedData = await _cache.GetStringAsync(cacheKey);
            // if (!string.IsNullOrEmpty(cachedData))
            // {
            //     return JsonSerializer.Deserialize<List<User>>(cachedData);
            // }
            var users = _users.Find(user => true).ToList();
            // var options = new DistributedCacheEntryOptions
            // {
            //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            // };
            // await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(users), options);

            return users;
        }

        public async Task<User> GetByIdAsync(string id)
        {
            // string cacheKey = $"User_{id}";

            // var cachedData = await _cache.GetStringAsync(cacheKey);
            // if (!string.IsNullOrEmpty(cachedData))
            // {
            //     return JsonSerializer.Deserialize<Users>(cachedData);
            // }

            var user = _users.Find(user => user.Id == id).FirstOrDefault();
            // if (user != null)
            // {
            //     var options = new DistributedCacheEntryOptions
            //     {
            //         AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            //     };
            //     await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), options);
            // }

            return user;
        }

        public void Update(string id, User updatedUser)
        {
            var filter = Builders<User>.Filter.Eq(s => s.Id, id);
            var updateDef = new List<UpdateDefinition<User>>();

            if (!string.IsNullOrEmpty(updatedUser.Username))
            {
                updateDef.Add(Builders<User>.Update.Set(s => s.Username, updatedUser.Username));
            }

            if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
            {
                updateDef.Add(Builders<User>.Update.Set(s => s.PasswordHash, HashPassword(updatedUser.PasswordHash)));
            }

            if (updateDef.Count == 0)
            {
                // Если ничего не передано — просто выходим
                return;
            }

            var update = Builders<User>.Update.Combine(updateDef);
            _users.UpdateOne(filter, update);
        }

        public void IncrementObject(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update.Inc(u => u.RegisteredObjects, 1);
            _users.UpdateOne(filter, update);
        }


        public void Delete(string id)
        {
            _users.DeleteOne(user => user.Id == id);
            // _cache.Remove($"User_{id}");
        }

    }
}
