using StackExchange.Redis;  // Необходим для работы с HashEntry
using ChessSchoolAPI.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ChessSchoolAPI.Services
{
    public class ChessStudentService
    {
        private readonly IMongoCollection<ChessStudent> _students;
        private readonly IDistributedCache _cache;
        private readonly ConnectionMultiplexer _redisConnection;  // Для подключения к Redis

        public ChessStudentService(IConfiguration config, IDistributedCache cache)
        {
            _cache = cache;

            var connectionString = config.GetSection("MongoDB:ConnectionString").Value;
            var databaseName = config.GetSection("MongoDB:DatabaseName").Value;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string is not configured.");
            }

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _students = database.GetCollection<ChessStudent>("ChessStudents");

            // Создаем подключение к Redis
            var redisConnectionString = config.GetSection("Redis:ConnectionString").Value;
            _redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        public async Task<List<ChessStudent>> GetAllAsync()
        {
            const string cacheKey = "ChessStudents";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<ChessStudent>>(cachedData);
            }

            var students = _students.Find(student => true).ToList();
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(students), options);

            return students;
        }

        public async Task<ChessStudent> GetByIdAsync(string id)
        {
            string cacheKey = $"ChessStudent_{id}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<ChessStudent>(cachedData);
            }

            var student = _students.Find(student => student.Id == id).FirstOrDefault();
            if (student != null)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(student), options);
            }

            return student;
        }

        public async Task<ChessStudent> Create(ChessStudent newStudent)
        {
            // Вставляем нового студента в базу данных
            _students.InsertOne(newStudent);

            // Кешируем студента как хэш в Redis
            string cacheKey = $"ChessStudent_{newStudent.Id}";

            var db = _redisConnection.GetDatabase();

            // Используем Redis Hashes для хранения данных студента
            var redisHash = new HashEntry[]
            {
                new HashEntry("Id", newStudent.Id),
                new HashEntry("Name", newStudent.Name),
                new HashEntry("Rating", newStudent.Rating),
                new HashEntry("EnrollmentDate", newStudent.EnrollmentDate.ToString("o"))  // Преобразуем в строку ISO 8601
            };

            await db.HashSetAsync(cacheKey, redisHash);

            return newStudent;
        }

        public void InsertMany(List<ChessStudent> students) =>
            _students.InsertMany(students);

        public void Update(string id, ChessStudent updatedStudent)
        {
            var filter = Builders<ChessStudent>.Filter.Eq(s => s.Id, id);
            var update = Builders<ChessStudent>.Update
                .Set(s => s.Name, updatedStudent.Name)
                .Set(s => s.Rating, updatedStudent.Rating)
                .Set(s => s.EnrollmentDate, updatedStudent.EnrollmentDate);

            // Обновляем запись в MongoDB
            _students.UpdateOne(filter, update);

            // Обновляем хеш в Redis
            var cacheKey = $"ChessStudent_{id}";
            var db = _redisConnection.GetDatabase();

            // Проверяем каждое значение на null
            var redisHash = new HashEntry[]
            {
                new HashEntry("Id", updatedStudent.Id ?? ""),
                new HashEntry("Name", updatedStudent.Name ?? ""),
                new HashEntry("Rating", updatedStudent.Rating != null ? updatedStudent.Rating.ToString() : "0"),
                new HashEntry("EnrollmentDate", updatedStudent.EnrollmentDate != null
                    ? updatedStudent.EnrollmentDate.ToString("o")
                    : "") // ISO 8601 или пустая строка
            };

            // Удаляем старый хеш и добавляем обновленный
            db.KeyDelete(cacheKey);
            db.HashSet(cacheKey, redisHash);
        }

        public void Delete(string id)
        {
            _students.DeleteOne(student => student.Id == id);
            _cache.Remove($"ChessStudent_{id}");
        }

        public void DeleteAll()
        {
            _students.DeleteMany(_ => true);

            // Удаляем все ключи из Redis
            var endpoints = _redisConnection.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redisConnection.GetServer(endpoint);
                var keys = server.Keys();
                var db = _redisConnection.GetDatabase();

                foreach (var key in keys)
                {
                    db.KeyDelete(key);
                }
            }
        }
    }
}
