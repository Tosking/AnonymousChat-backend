using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace TRPO
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public ObjectId UserId { get; set; }

        [BsonElement("chatId")]
        public ObjectId ChatId { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }
    }
}