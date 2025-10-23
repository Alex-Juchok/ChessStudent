using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;


namespace ChessSchoolAPI.Models
{

    public class ChessStudent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Name { get; set; }
        public int Rating { get; set; }
        public DateTime EnrollmentDate { get; set; }

        public string? User_id { get; set; }
        public string? ConfirmationTime { get; set; } 
    }

}
