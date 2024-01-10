using MongoDB.Bson;
using NoughtsAndCrosses.Infrastructure.Data.Entities;
using OneOf;
using OneOf.Types;

namespace NoughtsAndCrosses.Application.Services.Interfaces;

using GameResponse = OneOf<Game, Error<string>>;

public interface IGameService
{
    Task<GameResponse> InitializeAsync(ObjectId userId, PlayerSide side, CancellationToken ct = default);
    
    Task<GameResponse> ResumeAsync(ObjectId gameId, ObjectId userId, CancellationToken ct = default);
    
    Task<GameResponse> ProcessAsync(
        ObjectId gameId,
        ObjectId userId,
        int cellId,
        CancellationToken ct = default);
}