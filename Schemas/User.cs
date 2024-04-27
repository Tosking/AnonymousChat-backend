using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace TRPO
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("hash")]
        public string Hash { get; set; }
        [BsonElement("salt")]
        public string Salt { get; set; }
    }
}