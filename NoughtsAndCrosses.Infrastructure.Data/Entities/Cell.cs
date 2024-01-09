namespace NoughtsAndCrosses.Infrastructure.Data.Entities;

public struct Cell
{
    public int Id { get; set; }
    
    public PlayerSide? Side { get; set; }
}