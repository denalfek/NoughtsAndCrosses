using NoughtsAndCrosses.Infrastructure.Data.Entities;

namespace NoughtsAndCrosses.Application.Services.Interfaces;

public interface IBot
{
    Task<Cell> HitAsync(IEnumerable<Cell> field, PlayerSide botSide);
}