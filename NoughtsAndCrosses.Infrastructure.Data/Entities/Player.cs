using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoughtsAndCrosses.Infrastructure.Data.Entities;

public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; } = new();

    [BsonRepresentation(BsonType.String)]
    public PlayerSide Side { get; set; }
}