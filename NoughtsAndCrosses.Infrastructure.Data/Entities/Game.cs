using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoughtsAndCrosses.Infrastructure.Data.Entities;

public class Game
{
    public Game(Gamer[] gamers)
    {
        Id = new ObjectId();
        StaTime = DateTime.UtcNow;
        Gamers = gamers;
        Field = new Cell[9];

        for (var i = 0; i < 9; i++)
        {
            Field[i] = new Cell
            {
                Id = i,
                Side = null,
            };
        }
    }
    
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId? WinnerId { get; set; }
    
    public Cell[] Field { get; set; }
    
    public DateTime StaTime { get; set; }
    
    public DateTime? FinishTime { get; set; }
    
    public Gamer[] Gamers { get; set; }
}