using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoughtsAndCrosses.Infrastructure.Data.Entities;

public class Game
{
    public Game(Gamer[] gamers, int fieldSize)
    {
        Id = new ObjectId();
        StaTime = DateTime.UtcNow;
        Gamers = gamers;
        Field = new Cell[fieldSize];

        for (var i = 0; i < fieldSize; i++)
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
    
    public Gamer? Winner { get; set; }
    
    public Cell[] Field { get; set; }
    
    public DateTime StaTime { get; set; }
    
    public DateTime? FinishTime { get; set; }
    
    public Gamer[] Gamers { get; set; }
}