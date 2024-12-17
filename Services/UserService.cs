using ChessSchoolAPI.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace ChessSchoolAPI.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IConfiguration config)
        {
            var connectionString = config.GetSection("MongoDB:ConnectionString").Value;
            var databaseName = config.GetSection("MongoDB:DatabaseName").Value;

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _users = database.GetCollection<User>("Users");
        }

        public User GetByUsername(string username)
        {
            return _users.Find(user => user.Username == username).FirstOrDefault();
        }

        public void Create(User user)
        {
            _users.InsertOne(user);
        }

        // Метод для хэширования пароля
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
