using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace TRPO
{
    public class Chatroom
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("userIds")]
        public ObjectId[] UserIds { get; set; }
    }
}