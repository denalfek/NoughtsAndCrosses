using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoughtsAndCrosses.Infrastructure.Data.Entities;

public class Player
{
    [BsonId]
    public ObjectId Id { get; set; }

    public PlayerSide Side { get; set; }
}