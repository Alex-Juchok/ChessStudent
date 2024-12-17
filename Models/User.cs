using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChessSchoolAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; } // Хранится хэш пароля
    }
}
