using NoughtsAndCrosses.Application.Services.Interfaces;
using NoughtsAndCrosses.Infrastructure.Data.Entities;

namespace NoughtsAndCrosses.Application.Services;

public class Bot : IBot
{
    public async Task<Cell> HitAsync(IEnumerable<Cell> field, PlayerSide botSide)
    {
        var cell = field.First(x => x.Side == null);
        cell.Side = botSide;
        await Task.Delay(1000); // Simulate bot thinking
        return cell;
    }
}