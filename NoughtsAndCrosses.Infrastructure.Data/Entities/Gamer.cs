using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoughtsAndCrosses.Infrastructure.Data.Entities;

public class Gamer : User
{
    [BsonRepresentation(BsonType.String)]
    public PlayerSide Side { get; set; } = PlayerSide.Cross;
}